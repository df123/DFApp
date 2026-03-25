using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace DFApp.Account;

/// <summary>
/// 账户应用服务
/// </summary>
public class AccountAppService : ApplicationService, IAccountAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IPasswordHasher _passwordHasher;

    public AccountAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IConfiguration configuration,
        IMemoryCache cache,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _cache = cache;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [AllowAnonymous]
    public async Task<LoginResultDto> LoginAsync(LoginDto input)
    {
        try
        {
            // 检查登录尝试次数
            var cacheKey = $"LoginAttempts_{input.Username}";
            var attempts = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return 0;
            });

            if (attempts >= 5)
            {
                Logger.LogWarning($"登录失败：用户尝试次数过多");
                throw new UserFriendlyException("登录尝试次数过多，请15分钟后再试");
            }

            // 查找用户
            var queryable = await _userRepository.GetQueryableAsync();
            var user = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(u => u.UserName == input.Username));

            if (user == null)
            {
                Logger.LogWarning($"登录失败：用户名不存在");
                throw new UserFriendlyException("用户名或密码错误");
            }

            // 验证密码
            var result = _passwordHasher.VerifyPassword(user.PasswordHash ?? "", input.Password);
            if (!result)
            {
                Logger.LogWarning($"登录失败：密码错误");
                throw new UserFriendlyException("用户名或密码错误");
            }

            // 登录成功，清除尝试次数
            _cache.Remove(cacheKey);

            var token = GenerateJwtToken(user);

            return new LoginResultDto
            {
                AccessToken = token,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("Jwt:ExpirationMinutes"))
                    .ToUnixTimeSeconds(),
                Username = user.UserName,
                Email = user.Email
            };
        }
        catch (UserFriendlyException)
        {
            throw; // 重新抛出业务异常
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "登录过程中发生未知错误");
            throw new UserFriendlyException("登录失败，请稍后再试");
        }
    }

    /// <summary>
    /// 生成 JWT 令牌
    /// </summary>
    private string GenerateJwtToken(IdentityUser user)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret Key 未配置，请设置环境变量 JWT_SECRET_KEY");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:ExpirationMinutes")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 发送密码重置码
    /// </summary>
    [AllowAnonymous]
    public async Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
    {
        try
        {
            // 检查密码重置请求次数
            var resetAttemptsCacheKey = $"PasswordResetAttempts_{input.UserNameOrEmail}";
            var resetAttempts = _cache.GetOrCreate(resetAttemptsCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return 0;
            });

            if (resetAttempts >= 3)
            {
                Logger.LogWarning($"发送密码重置码失败：用户 {input.UserNameOrEmail} 尝试次数过多");
                throw new UserFriendlyException("密码重置请求次数过多，请1小时后再试");
            }

            // 增加尝试次数
            _cache.Set(resetAttemptsCacheKey, resetAttempts + 1, TimeSpan.FromHours(1));

            // 查找用户（通过用户名或邮箱）
            var queryable = await _userRepository.GetQueryableAsync();
            var user = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(u => u.UserName == input.UserNameOrEmail || u.Email == input.UserNameOrEmail));

            if (user == null)
            {
                Logger.LogWarning($"发送密码重置码失败：用户 {input.UserNameOrEmail} 不存在");
                throw new UserFriendlyException("用户名或邮箱不存在");
            }

            // TODO: 实现实际的邮件或短信发送功能
            // 当前为临时实现，仅记录日志，令牌存储在缓存中
            Logger.LogInformation($"发送密码重置码到用户：{user.Email ?? user.UserName}");

            // 生成重置令牌（有效期30分钟）
            var token = Guid.NewGuid().ToString();
            var cacheKey = $"PasswordResetToken_{user.Id}";
            _cache.Set(cacheKey, token, new TimeSpan(0, 30, 0));

            Logger.LogInformation($"密码重置令牌已生成");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "发送密码重置码过程中发生未知错误");
            throw new UserFriendlyException("发送密码重置码失败，请稍后再试");
        }
    }

    /// <summary>
    /// 验证密码重置令牌
    /// </summary>
    [AllowAnonymous]
    public async Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenDto input)
    {
        try
        {
            // 查找用户（通过用户名或邮箱）
            var queryable = await _userRepository.GetQueryableAsync();
            var user = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(u => u.UserName == input.UserNameOrEmail || u.Email == input.UserNameOrEmail));

            if (user == null)
            {
                Logger.LogWarning($"验证密码重置令牌失败：用户 {input.UserNameOrEmail} 不存在");
                return false;
            }

            // 验证令牌
            var cacheKey = $"PasswordResetToken_{user.Id}";
            var cachedToken = _cache.Get<string>(cacheKey);

            if (cachedToken == null || cachedToken != input.Token)
            {
                Logger.LogWarning($"验证密码重置令牌失败：令牌无效或已过期");
                return false;
            }

            // 验证成功后清除令牌，防止重复使用
            _cache.Remove(cacheKey);

            Logger.LogInformation($"密码重置令牌验证成功：{user.UserName ?? user.Email}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "验证密码重置令牌过程中发生未知错误");
            return false;
        }
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    [AllowAnonymous]
    public async Task<bool> ResetPasswordAsync(ResetPasswordDto input)
    {
        try
        {
            // 查找用户（通过用户名或邮箱）
            var queryable = await _userRepository.GetQueryableAsync();
            var user = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(u => u.UserName == input.UserNameOrEmail || u.Email == input.UserNameOrEmail));

            if (user == null)
            {
                Logger.LogWarning($"重置密码失败：用户 {input.UserNameOrEmail} 不存在");
                throw new UserFriendlyException("用户名或邮箱不存在");
            }

            // 验证令牌
            var cacheKey = $"PasswordResetToken_{user.Id}";
            var cachedToken = _cache.Get<string>(cacheKey);

            if (cachedToken == null || cachedToken != input.Token)
            {
                Logger.LogWarning($"重置密码失败：令牌无效或已过期");
                throw new UserFriendlyException("令牌无效或已过期");
            }

            // 更新密码
            var newPasswordHash = _passwordHasher.HashPassword(input.NewPassword);
            var passwordHashProperty = typeof(IdentityUser).GetProperty("PasswordHash");
            passwordHashProperty?.SetValue(user, newPasswordHash);
            await _userRepository.UpdateAsync(user);

            // 清除令牌
            _cache.Remove(cacheKey);

            Logger.LogInformation($"密码重置成功：{user.UserName ?? user.Email}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "重置密码过程中发生未知错误");
            throw new UserFriendlyException("重置密码失败，请稍后再试");
        }
    }
}
