using Volo.Abp.Account;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.BackgroundWorkers.Quartz;
using DFApp.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace DFApp;

[DependsOn(
    typeof(DFAppDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(DFAppApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
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
    }
}
