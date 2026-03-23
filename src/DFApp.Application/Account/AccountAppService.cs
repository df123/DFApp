using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly IPasswordHasher<IdentityUser> _passwordHasher;

    public AccountAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IConfiguration configuration,
        IMemoryCache cache,
        IPasswordHasher<IdentityUser> passwordHasher)
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
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", input.Password);
            if (result != PasswordVerificationResult.Success)
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
}
