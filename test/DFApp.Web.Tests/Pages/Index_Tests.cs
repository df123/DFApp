using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace DFApp.Pages;

public class Index_Tests : DFAppWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
