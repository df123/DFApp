using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace DF.Telegram.Pages;

public class Index_Tests : TelegramWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
