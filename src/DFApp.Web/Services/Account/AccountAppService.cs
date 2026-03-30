using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DFApp.Identity;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DFApp.Web.Services.Account;

/// <summary>
/// 账户应用服务
/// </summary>
public class AccountAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<User, Guid> _userRepository;
    private readonly ISqlSugarRepository<PermissionGrant, Guid> _permissionGrantRepository;
    private readonly ISqlSugarRepository<UserRole, Guid> _userRoleRepository;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AccountAppService> _logger;

    public AccountAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<User, Guid> userRepository,
        ISqlSugarRepository<PermissionGrant, Guid> permissionGrantRepository,
        ISqlSugarRepository<UserRole, Guid> userRoleRepository,
        IConfiguration configuration,
        IMemoryCache cache,
        IPasswordHasher passwordHasher,
        ILogger<AccountAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _userRepository = userRepository;
        _permissionGrantRepository = permissionGrantRepository;
        _userRoleRepository = userRoleRepository;
        _configuration = configuration;
        _cache = cache;
        _passwordHasher = passwordHasher;
        _logger = logger;
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
                _logger.LogWarning("登录失败：用户尝试次数过多");
                throw new BusinessException("登录尝试次数过多，请15分钟后再试");
            }

            // 查找用户
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.UserName == input.Username);

            if (user == null)
            {
                _logger.LogWarning("登录失败：用户名不存在");
                throw new BusinessException("用户名或密码错误");
            }

            // 验证密码
            var result = _passwordHasher.VerifyPassword(user.PasswordHash ?? "", input.Password);
            if (!result)
            {
                _logger.LogWarning("登录失败：密码错误");
                throw new BusinessException("用户名或密码错误");
            }

            // 登录成功，清除尝试次数
            _cache.Remove(cacheKey);

            var token = await GenerateJwtTokenAsync(user);

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
        catch (BusinessException)
        {
            throw; // 重新抛出业务异常
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录过程中发生未知错误");
            throw new BusinessException("登录失败，请稍后再试");
        }
    }

    /// <summary>
    /// 生成 JWT 令牌
    /// </summary>
    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret Key 未配置，请设置环境变量 JWT_SECRET_KEY");
        }

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 从用户角色关联表查询角色 ID
        var userRoles = await _userRepository.GetQueryable()
            .Where(u => u.Id == user.Id)
            .ToListAsync();

        var userRoleIds = await _userRoleRepository.GetQueryable()
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        // 将角色ID转换为字符串列表
        var userRoleIdStrings = userRoleIds.Select(id => id.ToString()).ToList();

        // 查询权限授予记录
        var permissions = await _permissionGrantRepository.GetQueryable()
            .Where(pg =>
                (pg.ProviderName == "U" && pg.ProviderKey == user.Id.ToString()) ||
                (pg.ProviderName == "R" && userRoleIdStrings.Contains(pg.ProviderKey))
            )
            .Select(pg => pg.Name)
            .ToListAsync();

        // 将权限添加到JWT claims中
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

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
                _logger.LogWarning("发送密码重置码失败：用户 {UserNameOrEmail} 尝试次数过多", input.UserNameOrEmail);
                throw new BusinessException("密码重置请求次数过多，请1小时后再试");
            }

            // 增加尝试次数
            _cache.Set(resetAttemptsCacheKey, resetAttempts + 1, TimeSpan.FromHours(1));

            // 查找用户（通过用户名或邮箱）
            var user = await _userRepository.GetFirstOrDefaultAsync(
                u => u.UserName == input.UserNameOrEmail || u.Email == input.UserNameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("发送密码重置码失败：用户 {UserNameOrEmail} 不存在", input.UserNameOrEmail);
                throw new BusinessException("用户名或邮箱不存在");
            }

            // TODO: 实现实际的邮件或短信发送功能
            // 当前为临时实现，仅记录日志，令牌存储在缓存中
            _logger.LogInformation("发送密码重置码到用户：{Email}", user.Email ?? user.UserName);

            // 生成重置令牌（有效期30分钟）
            var token = Guid.NewGuid().ToString();
            var cacheKey = $"PasswordResetToken_{user.Id}";
            _cache.Set(cacheKey, token, new TimeSpan(0, 30, 0));

            _logger.LogInformation("密码重置令牌已生成");
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送密码重置码过程中发生未知错误");
            throw new BusinessException("发送密码重置码失败，请稍后再试");
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
            var user = await _userRepository.GetFirstOrDefaultAsync(
                u => u.UserName == input.UserNameOrEmail || u.Email == input.UserNameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("验证密码重置令牌失败：用户 {UserNameOrEmail} 不存在", input.UserNameOrEmail);
                return false;
            }

            // 验证令牌
            var cacheKey = $"PasswordResetToken_{user.Id}";
            var cachedToken = _cache.Get<string>(cacheKey);

            if (cachedToken == null || cachedToken != input.Token)
            {
                _logger.LogWarning("验证密码重置令牌失败：令牌无效或已过期");
                return false;
            }

            // 验证成功后清除令牌，防止重复使用
            _cache.Remove(cacheKey);

            _logger.LogInformation("密码重置令牌验证成功：{UserName}", user.UserName ?? user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证密码重置令牌过程中发生未知错误");
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
            var user = await _userRepository.GetFirstOrDefaultAsync(
                u => u.UserName == input.UserNameOrEmail || u.Email == input.UserNameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("重置密码失败：用户 {UserNameOrEmail} 不存在", input.UserNameOrEmail);
                throw new BusinessException("用户名或邮箱不存在");
            }

            // 验证令牌
            var cacheKey = $"PasswordResetToken_{user.Id}";
            var cachedToken = _cache.Get<string>(cacheKey);

            if (cachedToken == null || cachedToken != input.Token)
            {
                _logger.LogWarning("重置密码失败：令牌无效或已过期");
                throw new BusinessException("令牌无效或已过期");
            }

            // 更新密码
            user.PasswordHash = _passwordHasher.HashPassword(input.NewPassword);
            await _userRepository.UpdateAsync(user);

            // 清除令牌
            _cache.Remove(cacheKey);

            _logger.LogInformation("密码重置成功：{UserName}", user.UserName ?? user.Email);
            return true;
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置密码过程中发生未知错误");
            throw new BusinessException("重置密码失败，请稍后再试");
        }
    }
}
