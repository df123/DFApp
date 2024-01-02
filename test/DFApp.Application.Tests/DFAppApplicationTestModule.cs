using Volo.Abp.Modularity;

namespace DFApp;

[DependsOn(
    typeof(DFAppApplicationModule),
    typeof(DFAppDomainTestModule)
)]
public class DFAppApplicationTestModule : AbpModule
{

}
