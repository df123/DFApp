using DFApp.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;

namespace DFApp.Web.Pages.Configuration
{
    public class EditModalModel : DFAppPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public UpdateConfigurationInfoViewModel ConfigurationInfoDto { get; set; }

        private readonly IConfigurationInfoService _configurationInfoService;

        public EditModalModel(IConfigurationInfoService configurationInfoService)
        {
            _configurationInfoService = configurationInfoService;
        }

        public async Task OnGetAsync()
        {
            var catetory = await _configurationInfoService.GetAsync(Id);
            ConfigurationInfoDto = ObjectMapper.Map<ConfigurationInfoDto, UpdateConfigurationInfoViewModel>(catetory);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _configurationInfoService.UpdateAsync(Id, ObjectMapper.Map<UpdateConfigurationInfoViewModel, CreateUpdateConfigurationInfoDto>(ConfigurationInfoDto));
            return NoContent();
        }

        public class UpdateConfigurationInfoViewModel
        {
            [DisplayName("模块名称")]
            public string? ModuleName { get; set; }
            [DisplayName("配置名称")]
            [Required]
            public string ConfigurationName { get; set; }
            [DisplayName("配置值")]
            [Required]
            public string ConfigurationValue { get; set; }
            [DisplayName("备注")]
            public string? Remark { get; set; }
        }

    }
}
