using System.Threading.Tasks;
using DFApp.Configuration;
using DFApp.Helper;

namespace DFApp.Web.Pages;

public class IndexModel : DFAppPageModel
{
    public string? RemainingDiskSpace { get; private set; }

    private readonly ConfigurationInfoService _configurationInfoService;
    public IndexModel(ConfigurationInfoService configurationInfoService)
    {
        _configurationInfoService = configurationInfoService;
    }

    public async Task OnGetAsync()
    {
        RemainingDiskSpace = await GetRemainingDiskSpaceAsync();
    }

    private async Task<string> GetRemainingDiskSpaceAsync()
    {
        string SaveDrive = await _configurationInfoService.GetConfigurationInfoValue("SaveDrive",string.Empty);
        return StorageUnitConversionHelper.ByteToGB(SpaceHelper.GetAnyDriveAvailable(SaveDrive)).ToString("F2") + "GB";
    }


}
