using DFApp.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace DFApp.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(DFAppEntityFrameworkCoreModule),
    typeof(DFAppApplicationContractsModule)
    )]
public class DFAppDbMigratorModule : AbpModule
{
}
