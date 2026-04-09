using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DFApp.Account;
using DFApp.Identity;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;

// 使用别名明确引用新 DTO，避免与 ABP 层旧类型冲突
using LoginDto = DFApp.Web.DTOs.Account.LoginDto;
using LoginResultDto = DFApp.Web.DTOs.Account.LoginResultDto;
using SendPasswordResetCodeDto = DFApp.Web.DTOs.Account.SendPasswordResetCodeDto;
using VerifyPasswordResetTokenDto = DFApp.Web.DTOs.Account.VerifyPasswordResetTokenDto;
using ResetPasswordDto = DFApp.Web.DTOs.Account.ResetPasswordDto;
using IPasswordHasher = DFApp.Web.Infrastructure.IPasswordHasher;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DFApp.Web.Services.Account;

/// <summary>
/// 账户应用服务
/// </summary>
public class AccountAppService
{
    private readonly ISqlSugarRepository<User, Guid> _userRepository;
    private readonly ISqlSugarRepository<Role, Guid> _roleRepository;
    private readonly ISqlSugarRepository<AppPermissionGrant, long> _appPermissionGrantRepository;
    private readonly ISqlSugarRepository<UserRole, Guid> _userRoleRepository;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AccountAppService> _logger;

    public AccountAppService(
        ISqlSugarRepository<User, Guid> userRepository,
        ISqlSugarRepository<Role, Guid> roleRepository,
        ISqlSugarRepository<AppPermissionGrant, long> appPermissionGrantRepository,
        ISqlSugarRepository<UserRole, Guid> userRoleRepository,
        IConfiguration configuration,
        IMemoryCache cache,
        IPasswordHasher passwordHasher,
        ILogger<AccountAppService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _appPermissionGrantRepository = appPermissionGrantRepository;
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
                // 递增登录失败次数
                _cache.Set(cacheKey, attempts + 1, TimeSpan.FromMinutes(15));
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
    /// <remarks>
    /// 从新的 AppPermissionGrants 表加载权限。
    /// 角色级权限的 ProviderKey 存储角色名称（非 GUID），避免大小写匹配问题。
    /// 用户级权限的 ProviderKey 存储用户 ID 字符串（小写）。
    /// </remarks>
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
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userIdUpper = user.Id.ToString().ToUpperInvariant();

        // 查询用户角色关联，使用 UPPER() 避免 GUID 大小写问题
        var userRoles = await _userRoleRepository.GetQueryable()
            .Where("UPPER(UserId) = @UserId", new { UserId = userIdUpper })
            .ToListAsync();

        _logger.LogDebug("用户 {UserName} 查到 {RoleCount} 条角色关联", user.UserName, userRoles.Count);

        // 获取所有角色并在内存中匹配名称（角色数量少，内存匹配避免 SqlSugar Contains 翻译问题）
        var allRoles = await _roleRepository.GetQueryable().ToListAsync();
        var roleNames = allRoles
            .Where(r => userRoles.Any(ur => string.Equals(ur.RoleId.ToString(), r.Id.ToString(), StringComparison.OrdinalIgnoreCase)))
            .Select(r => r.Name)
            .ToList();

        // 将角色名称添加到 JWT claims
        foreach (var roleName in roleNames)
        {
            claims.Add(new Claim(DFAppClaimTypes.Role, roleName));
        }

        // 从新表 AppPermissionGrants 加载权限
        var permissionSet = new HashSet<string>();

        // 用户级权限（ProviderKey 为用户 ID 字符串）
        var userPermissions = await _appPermissionGrantRepository.GetQueryable()
            .Where(pg => pg.ProviderType == "User" && pg.ProviderKey == user.Id.ToString())
            .Select(pg => pg.PermissionName)
            .ToListAsync();

        foreach (var p in userPermissions)
        {
            permissionSet.Add(p);
        }

        _logger.LogDebug("用户级权限: {Count} 个", userPermissions.Count);

        // 角色级权限（ProviderKey 为角色名称）
        if (roleNames.Count > 0)
        {
            // 查询所有角色级权限，在内存中匹配（避免 SqlSugar Contains 翻译问题）
            var rolePermissionList = await _appPermissionGrantRepository.GetQueryable()
                .Where(pg => pg.ProviderType == "Role")
                .ToListAsync();

            foreach (var pg in rolePermissionList)
            {
                if (roleNames.Contains(pg.ProviderKey))
                {
                    permissionSet.Add(pg.PermissionName);
                }
            }

            _logger.LogDebug("角色级权限: {Count} 个", rolePermissionList.Count(pg => roleNames.Contains(pg.ProviderKey)));
        }

        _logger.LogInformation("用户 {UserName} 令牌中共有 {PermCount} 个权限", user.UserName, permissionSet.Count);

        // 将权限添加到 JWT claims（HashSet 已去重）
        foreach (var permission in permissionSet)
        {
            claims.Add(new Claim(DFAppClaimTypes.Permission, permission));
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
