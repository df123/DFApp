using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace DF.Telegram.Web;

[Dependency(ReplaceServices = true)]
public class TelegramBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Telegram";
}
