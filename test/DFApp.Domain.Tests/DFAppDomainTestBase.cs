using Volo.Abp.Modularity;

namespace DFApp;

/* Inherit from this class for your domain layer tests. */
public abstract class DFAppDomainTestBase<TStartupModule> : DFAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
