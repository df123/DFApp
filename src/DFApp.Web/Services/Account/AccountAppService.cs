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
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 同一来源（用户名+IP）连续失败次数上限
    /// </summary>
    private const int PerSourceLoginFailureLimit = 5;

    /// <summary>
    /// 同一用户名跨来源失败次数上限（防多源暴力破解的兜底）
    /// </summary>
    private const int PerUsernameLoginFailureLimit = 50;

    /// <summary>
    /// 登录失败计数窗口
    /// </summary>
    private static readonly TimeSpan LoginFailureWindow = TimeSpan.FromMinutes(15);

    public AccountAppService(
        ISqlSugarRepository<User, Guid> userRepository,
        ISqlSugarRepository<Role, Guid> roleRepository,
        ISqlSugarRepository<AppPermissionGrant, long> appPermissionGrantRepository,
        ISqlSugarRepository<UserRole, Guid> userRoleRepository,
        IConfiguration configuration,
        IMemoryCache cache,
        IPasswordHasher passwordHasher,
        ILogger<AccountAppService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _appPermissionGrantRepository = appPermissionGrantRepository;
        _userRoleRepository = userRoleRepository;
        _configuration = configuration;
        _cache = cache;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [AllowAnonymous]
    public async Task<LoginResultDto> LoginAsync(LoginDto input)
    {
        try
        {
            // 检查登录尝试次数：按"用户名+来源IP"计数，避免匿名者仅凭用户名锁定任意账户；
            // 另按用户名设跨来源上限作为暴力破解兜底。
            // 计数器在窗口创建时固定过期时间，后续失败只递增不续期，
            // 攻击者无法通过持续失败把受害者的锁定窗口无限延长。
            var clientIp = GetClientIpAddress();
            var attemptKey = $"LoginAttempts_{input.Username}_{clientIp}";
            var wideKey = $"LoginWideAttempts_{input.Username}";
            var attempts = GetOrCreateFailureCounter(attemptKey);
            var wideAttempts = GetOrCreateFailureCounter(wideKey);

            if (attempts.Count >= PerSourceLoginFailureLimit ||
                wideAttempts.Count >= PerUsernameLoginFailureLimit)
            {
                _logger.LogWarning("登录失败：用户尝试次数过多（用户名: {UserName}, 来源: {Ip}）", input.Username, clientIp);
                throw new BusinessException("登录尝试次数过多，请15分钟后再试");
            }

            // 查找用户
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.UserName == input.Username);

            if (user == null)
            {
                _logger.LogWarning("登录失败：用户名不存在");
                throw new BusinessException("用户名或密码错误");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("登录失败：用户 {UserName} 已停用", user.UserName);
                throw new BusinessException("用户名或密码错误");
            }

            // 验证密码
            var result = _passwordHasher.VerifyPassword(user.PasswordHash ?? "", input.Password);
            if (!result)
            {
                _logger.LogWarning("登录失败：密码错误");
                // 递增登录失败次数（仅递增已缓存的计数器，不重设过期时间）
                attempts.Count++;
                wideAttempts.Count++;
                throw new BusinessException("用户名或密码错误");
            }

            // 登录成功，清除尝试次数
            _cache.Remove(attemptKey);
            _cache.Remove(wideKey);

            var (token, roles, permissions) = await GenerateJwtTokenAsync(user);

            return new LoginResultDto
            {
                AccessToken = token,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("Jwt:ExpirationMinutes"))
                    .ToUnixTimeSeconds(),
                Username = user.UserName,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions
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
    /// 获取或创建登录失败计数器；过期时间在首次创建时固定，之后只递增不续期
    /// </summary>
    private LoginFailureCounter GetOrCreateFailureCounter(string key)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LoginFailureWindow;
            return new LoginFailureCounter();
        }) ?? new LoginFailureCounter();
    }

    /// <summary>
    /// 获取客户端 IP（经反向代理转发时取 X-Forwarded-For 首个地址）
    /// </summary>
    private string GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Connection.RemoteIpAddress is null)
        {
            return "unknown";
        }

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var values))
        {
            var first = values.ToString().Split(',', StringSplitOptions.TrimEntries).FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                return first;
            }
        }

        return context.Connection.RemoteIpAddress.ToString();
    }

    /// <summary>
    /// 可变登录失败计数器（利用引用类型原地递增，避免重新 Set 刷新过期时间）
    /// </summary>
    private sealed class LoginFailureCounter
    {
        public int Count { get; set; }
    }

    /// <summary>
    /// 生成 JWT 令牌
    /// </summary>
    /// <remarks>
    /// 从新的 AppPermissionGrants 表加载权限。
    /// 角色级权限的 ProviderKey 存储角色名称（非 GUID），避免大小写匹配问题。
    /// 用户级权限的 ProviderKey 存储用户 ID 字符串（小写）。
    /// </remarks>
    private async Task<(string token, List<string> roles, List<string> permissions)> GenerateJwtTokenAsync(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
        {
            throw new InvalidOperationException("JWT Secret Key 未配置或长度不足 32 字节，请设置环境变量 Jwt__SecretKey");
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

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, roleNames, permissionSet.ToList());
    }

    /// <summary>
    /// 发送密码重置码
    /// </summary>
    /// <remarks>
    /// 响应对账号是否存在保持一致，防止用户名枚举；限速按来源 IP 计数，
    /// 避免攻击者变换提交串绕过按字符串的限速。
    /// </remarks>
    [AllowAnonymous]
    public async Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
    {
        try
        {
            // 按来源 IP 检查密码重置请求次数
            var clientIp = GetClientIpAddress();
            var resetAttemptsCacheKey = $"PasswordResetAttempts_{clientIp}";
            var resetAttempts = _cache.GetOrCreate(resetAttemptsCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return 0;
            });

            if (resetAttempts >= 5)
            {
                _logger.LogWarning("发送密码重置码失败：来源 {Ip} 请求次数过多", clientIp);
                throw new BusinessException("密码重置请求次数过多，请1小时后再试");
            }

            // 增加尝试次数
            _cache.Set(resetAttemptsCacheKey, resetAttempts + 1, TimeSpan.FromHours(1));

            // 查找用户（通过用户名或邮箱）
            var user = await _userRepository.GetFirstOrDefaultAsync(
                u => u.UserName == input.UserNameOrEmail || u.Email == input.UserNameOrEmail);

            if (user == null)
            {
                // 不向调用方透露账号是否存在
                _logger.LogWarning("发送密码重置码：账号不存在（不对外区分响应）");
                return;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("发送密码重置码：账号 {UserName} 已停用（不对外区分响应）", user.UserName);
                return;
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

            if (!user.IsActive)
            {
                _logger.LogWarning("验证密码重置令牌失败：用户 {UserNameOrEmail} 已停用", input.UserNameOrEmail);
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

            if (!user.IsActive)
            {
                _logger.LogWarning("重置密码失败：用户 {UserNameOrEmail} 已停用", input.UserNameOrEmail);
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
