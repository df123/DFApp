using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Permissions;

/// <summary>
/// 权限检查实现
/// </summary>
public class PermissionChecker : IPermissionChecker
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PermissionChecker> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器</param>
    /// <param name="logger">日志记录器</param>
    public PermissionChecker(IHttpContextAccessor httpContextAccessor, ILogger<PermissionChecker> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 检查当前用户是否拥有指定权限
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    /// <returns>如果拥有权限返回 true，否则返回 false</returns>
    public Task<bool> IsGrantedAsync(string permissionName)
    {
        if (string.IsNullOrEmpty(permissionName))
        {
            _logger.LogWarning("权限名称为空");
            return Task.FromResult(false);
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
        {
            _logger.LogWarning("HTTP 上下文或用户为空");
            return Task.FromResult(false);
        }

        var user = httpContext.User;

        // 从 JWT Token 的 Claims 中读取权限
        var permissionClaims = user.FindAll("Permission");
        if (permissionClaims == null || !permissionClaims.Any())
        {
            _logger.LogDebug("用户没有权限声明");
            return Task.FromResult(false);
        }

        var hasPermission = permissionClaims.Any(c => c.Value == permissionName);
        _logger.LogDebug("用户 {UserName} 检查权限 {PermissionName}: {HasPermission}",
            user.Identity?.Name ?? "Unknown",
            permissionName,
            hasPermission);

        return Task.FromResult(hasPermission);
    }
}
