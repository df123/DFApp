using Volo.Abp.Modularity;

namespace DFApp;

[DependsOn(
    typeof(DFAppDomainModule),
    typeof(DFAppTestBaseModule)
)]
public class DFAppDomainTestModule : AbpModule
{

}
