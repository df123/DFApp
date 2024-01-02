using System;
using System.Collections.Generic;
using System.Text;
using DFApp.Localization;
using Volo.Abp.Application.Services;

namespace DFApp;

/* Inherit your application services from this class.
 */
public abstract class DFAppAppService : ApplicationService
{
    protected DFAppAppService()
    {
        LocalizationResource = typeof(DFAppResource);
    }
}
