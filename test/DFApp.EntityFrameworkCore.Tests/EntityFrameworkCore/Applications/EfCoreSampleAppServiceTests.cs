using DFApp.Samples;
using Xunit;

namespace DFApp.EntityFrameworkCore.Applications;

[Collection(DFAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<DFAppEntityFrameworkCoreTestModule>
{

}
