using DFApp.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace DFApp.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class DFAppPageModel : AbpPageModel
{
    protected DFAppPageModel()
    {
        LocalizationResourceType = typeof(DFAppResource);
    }
}
