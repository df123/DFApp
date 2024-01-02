using Xunit;

namespace DFApp.EntityFrameworkCore;

[CollectionDefinition(DFAppTestConsts.CollectionDefinitionName)]
public class DFAppEntityFrameworkCoreCollection : ICollectionFixture<DFAppEntityFrameworkCoreFixture>
{

}
