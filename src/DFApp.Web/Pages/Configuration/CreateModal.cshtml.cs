using DFApp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Configuration
{
    public class CreateModalModel : DFAppPageModel
    {
        [BindProperty]
        public CreateConfigurationInfoViewModel ConfigurationInfoViewModel { get; set; }

        private readonly IConfigurationInfoService _configurationInfoService;

        public CreateModalModel(IConfigurationInfoService configurationInfoService)
        {
            _configurationInfoService = configurationInfoService;
        }

        public async Task OnGetAsync()
        {
            ConfigurationInfoViewModel = new CreateConfigurationInfoViewModel();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _configurationInfoService.CreateAsync(
                ObjectMapper.Map<CreateConfigurationInfoViewModel, CreateUpdateConfigurationInfoDto>(ConfigurationInfoViewModel)
                );
            return NoContent();
        }

        public class CreateConfigurationInfoViewModel
        {
            [Required]
            public string ModuleName { get; set; }
            [Required]
            public string ConfigurationName { get; set; }
            [Required]
            public string ConfigurationValue { get; set; }
        }

    }
}
