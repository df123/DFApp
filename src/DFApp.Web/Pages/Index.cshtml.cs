using System.Threading.Tasks;
using DFApp.Configuration;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFApp.Web.Pages;

public class IndexModel : PageModel
{
    public string? RemainingDiskSpace { get; private set; }

    private readonly ConfigurationInfoService _configurationInfoService;
    public IndexModel(ConfigurationInfoService configurationInfoService)
    {
        _configurationInfoService = configurationInfoService;
    }

    public async Task OnGetAsync()
    {
        RemainingDiskSpace = await _configurationInfoService.GetRemainingDiskSpaceAsync();
    }


}
