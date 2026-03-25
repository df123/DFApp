using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;

namespace DFApp.Account;

/// <summary>
/// JWT 权限值提供者
/// 从 JWT token 的 claims 中读取权限
/// </summary>
public class JwtPermissionValueProvider : PermissionValueProvider, ITransientDependency
{
    public const string ProviderName = "Jwt";

    private readonly ILogger<JwtPermissionValueProvider> _logger;

    public JwtPermissionValueProvider(
        IPermissionStore permissionStore,
        ILogger<JwtPermissionValueProvider> logger)
        : base(permissionStore)
    {
        _logger = logger;
    }

    public override string Name => ProviderName;

    /// <summary>
    /// 检查多个权限的授予情况
    /// </summary>
    public override Task<MultiplePermissionGrantResult> CheckAsync(PermissionValuesCheckContext context)
    {
        var permissionClaims = context.Principal?.FindAll("Permission");
        if (permissionClaims != null && permissionClaims.Any())
        {
            var results = new MultiplePermissionGrantResult();

            foreach (var permission in context.Permissions)
            {
                var hasPermission = permissionClaims.Any(c => c.Value == permission.Name);
                results.Result[permission.Name] = hasPermission ? PermissionGrantResult.Granted : PermissionGrantResult.Undefined;
            }

            _logger.LogDebug($"JWT 权限检查完成，检查了 {context.Permissions.Count} 个权限");
            return Task.FromResult(results);
        }

        _logger.LogDebug($"用户不拥有任何权限");
        return Task.FromResult(new MultiplePermissionGrantResult());
    }

    /// <summary>
    /// 检查单个权限的授予情况
    /// </summary>
    public override Task<PermissionGrantResult> CheckAsync(PermissionValueCheckContext context)
    {
        var permissionClaims = context.Principal?.FindAll("Permission");
        if (permissionClaims != null && permissionClaims.Any())
        {
            var hasPermission = permissionClaims.Any(c => c.Value == context.Permission.Name);
            _logger.LogDebug($"JWT 权限检查完成，权限：{context.Permission.Name}，结果：{(hasPermission ? "已授予" : "未定义")}");
            return Task.FromResult(hasPermission ? PermissionGrantResult.Granted : PermissionGrantResult.Undefined);
        }

        _logger.LogDebug($"用户不拥有任何权限，权限：{context.Permission.Name}");
        return Task.FromResult(PermissionGrantResult.Undefined);
    }
}
