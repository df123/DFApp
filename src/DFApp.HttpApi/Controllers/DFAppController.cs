using DFApp.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace DFApp.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class DFAppController : AbpControllerBase
{
    protected DFAppController()
    {
        LocalizationResource = typeof(DFAppResource);
    }
}
