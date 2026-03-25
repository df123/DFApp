using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.BackgroundWorkers.Quartz;
using DFApp.Queue;
using DFApp.Aria2;
using DFApp.Account;
using Volo.Abp.Authorization.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace DFApp;

[DependsOn(
    typeof(DFAppDomainModule),
    typeof(DFAppApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
[DependsOn(typeof(AbpBackgroundWorkersQuartzModule))]
public class DFAppApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IQueueManagement, QueueManagement>();
        context.Services.AddHttpClient<Aria2RpcClient>();

        // 注册 JWT 权限值提供者
        Configure<AbpPermissionOptions>(options =>
        {
            options.ValueProviders.Add<JwtPermissionValueProvider>();
        });
    }
}
