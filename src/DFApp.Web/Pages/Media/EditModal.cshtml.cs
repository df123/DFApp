using DFApp.Media;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Media
{
    public class EditModalModel : DFAppPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public CreateUpdateMediaInfoDto MediaInfoDto { get; set; }

        private readonly IMediaInfoService _mediaInfoService;

        public EditModalModel(IMediaInfoService mediaInfoService)
        {
            _mediaInfoService = mediaInfoService;
        }

        public async Task OnGetAsync()
        {
            var media = await _mediaInfoService.GetAsync(Id);
            MediaInfoDto = ObjectMapper.Map<MediaInfoDto, CreateUpdateMediaInfoDto>(media);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediaInfoService.UpdateAsync(Id, MediaInfoDto);
            return NoContent();
        }

    }
}
