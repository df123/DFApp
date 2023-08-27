using Volo.Abp.Modularity;

namespace DF.Telegram;

[DependsOn(
    typeof(TelegramApplicationModule),
    typeof(TelegramDomainTestModule)
    )]
public class TelegramApplicationTestModule : AbpModule
{

}
