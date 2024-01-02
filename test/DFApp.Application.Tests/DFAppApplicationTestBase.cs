using Volo.Abp.Modularity;

namespace DFApp;

public abstract class DFAppApplicationTestBase<TStartupModule> : DFAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
