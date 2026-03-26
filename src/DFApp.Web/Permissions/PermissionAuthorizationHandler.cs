using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Permissions;

/// <summary>
/// 权限授权处理器，用于检查用户是否拥有所需权限
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 处理授权需求
    /// </summary>
    /// <param name="context">授权上下文</param>
    /// <param name="requirement">权限需求</param>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            _logger.LogWarning("用户未认证");
            return Task.CompletedTask;
        }

        // 从 JWT Token 的 Claims 中读取权限
        var permissionClaims = context.User.FindAll("Permission");
        if (permissionClaims == null || !permissionClaims.Any())
        {
            _logger.LogDebug("用户 {UserName} 没有权限声明", context.User.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
        }

        var hasPermission = permissionClaims.Any(c => c.Value == requirement.PermissionName);

        if (hasPermission)
        {
            _logger.LogDebug("用户 {UserName} 拥有权限 {PermissionName}",
                context.User.Identity?.Name ?? "Unknown",
                requirement.PermissionName);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("用户 {UserName} 不拥有权限 {PermissionName}",
                context.User.Identity?.Name ?? "Unknown",
                requirement.PermissionName);
        }

        return Task.CompletedTask;
    }
}
