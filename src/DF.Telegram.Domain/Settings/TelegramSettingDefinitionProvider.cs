using Volo.Abp.Settings;

namespace DF.Telegram.Settings;

public class TelegramSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(TelegramSettings.MySetting1));
    }
}
