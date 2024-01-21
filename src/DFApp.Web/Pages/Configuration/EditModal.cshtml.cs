using DFApp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Configuration
{
    public class EditModalModel : DFAppPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public CreateUpdateConfigurationInfoDto ConfigurationInfoDto { get; set; }

        private readonly IConfigurationInfoService _configurationInfoService;

        public EditModalModel(IConfigurationInfoService configurationInfoService)
        {
            _configurationInfoService = configurationInfoService;
        }

        public async Task OnGetAsync()
        {
            var catetory = await _configurationInfoService.GetAsync(Id);
            ConfigurationInfoDto = ObjectMapper.Map<ConfigurationInfoDto, CreateUpdateConfigurationInfoDto>(catetory);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _configurationInfoService.UpdateAsync(Id, ConfigurationInfoDto);
            return NoContent();
        }
    }
}
