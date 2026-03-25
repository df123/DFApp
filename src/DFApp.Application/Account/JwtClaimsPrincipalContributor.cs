using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace DFApp.Account;

/// <summary>
/// JWT Claims Principal 贡献者
/// 从 JWT token 的 claims 中提取权限并添加到 ClaimsPrincipal
/// </summary>
public class JwtClaimsPrincipalContributor : IAbpClaimsPrincipalContributor, ITransientDependency
{
    private readonly ILogger<JwtClaimsPrincipalContributor> _logger;

    public JwtClaimsPrincipalContributor(
        ILogger<JwtClaimsPrincipalContributor> logger)
    {
        _logger = logger;
    }

    public Task ContributeAsync(AbpClaimsPrincipalContributorContext context)
    {
        var identity = context.ClaimsPrincipal.Identities.FirstOrDefault();
        if (identity == null)
        {
            return Task.CompletedTask;
        }

        // 从 JWT token 的 claims 中提取权限
        var permissionClaims = context.ClaimsPrincipal.FindAll("Permission").ToList();
        if (permissionClaims.Any())
        {
            // 将权限 claims 添加到 identity
            foreach (var claim in permissionClaims)
            {
                identity.AddClaim(new Claim("Permission", claim.Value));
            }

            _logger.LogDebug($"从 JWT token 中提取了 {permissionClaims.Count} 个权限");
        }

        return Task.CompletedTask;
    }
}
