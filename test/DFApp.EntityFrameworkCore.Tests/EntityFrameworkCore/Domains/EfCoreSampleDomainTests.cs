using DFApp.Samples;
using Xunit;

namespace DFApp.EntityFrameworkCore.Domains;

[Collection(DFAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<DFAppEntityFrameworkCoreTestModule>
{

}
