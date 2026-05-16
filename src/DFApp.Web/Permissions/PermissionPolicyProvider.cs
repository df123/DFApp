using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DFApp.Web.Permissions;

/// <summary>
/// 权限策略提供者，用于动态生成权限策略
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">授权选项</param>
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    /// <summary>
    /// 获取默认策略
    /// </summary>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    /// <summary>
    /// 获取回退策略
    /// </summary>
    public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    /// <summary>
    /// 获取指定名称的策略
    /// </summary>
    /// <param name="policyName">策略名称</param>
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        // 如果策略名称以 "Permission:" 开头，则创建权限策略
        if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
        {
            var permissionName = policyName.Substring("Permission:".Length);
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permissionName));
            return Task.FromResult(policy.Build());
        }

        // 否则使用默认策略提供者
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
