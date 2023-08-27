using DF.Telegram.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace DF.Telegram.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class TelegramPageModel : AbpPageModel
{
    protected TelegramPageModel()
    {
        LocalizationResourceType = typeof(TelegramResource);
    }
}
