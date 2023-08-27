using DF.Telegram.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace DF.Telegram.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class TelegramController : AbpControllerBase
{
    protected TelegramController()
    {
        LocalizationResource = typeof(TelegramResource);
    }
}
