using DF.Telegram.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace DF.Telegram;

[DependsOn(
    typeof(TelegramEntityFrameworkCoreTestModule)
    )]
public class TelegramDomainTestModule : AbpModule
{

}
