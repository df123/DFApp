using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace DFApp.Web;

[Dependency(ReplaceServices = true)]
public class DFAppBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "DFApp";
}
