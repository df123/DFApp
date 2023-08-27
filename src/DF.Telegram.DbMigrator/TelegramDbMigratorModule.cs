using DF.Telegram.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace DF.Telegram.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TelegramEntityFrameworkCoreModule),
    typeof(TelegramApplicationContractsModule)
    )]
public class TelegramDbMigratorModule : AbpModule
{

}
