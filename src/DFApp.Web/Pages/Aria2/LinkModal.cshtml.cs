using DFApp.Aria2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;

namespace DFApp.Web.Pages.Aria2
{
    public class LinkModalModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public long? Id { get; set; }

        [BindProperty]
        public ContentViewModel? ContentDto { get; set; }

        private readonly IAria2Service _aria2Service;

        public LinkModalModel(IAria2Service aria2Service)
        {
            _aria2Service = aria2Service;
        }

        public async void OnGet()
        {
            ContentDto = new ContentViewModel();

            if (Id.HasValue && Id.Value > 0)
            {
                ContentDto.Content = await _aria2Service.GetExternalLink(Id.Value);
            }
            else
            {
                var allLinks = await _aria2Service.GetAllExternalLinks();
                ContentDto.Content = string.Join("\n", allLinks);
            }
        }

        public class ContentViewModel
        {
            [DisplayName("Aria2:LinkContent")]
            [TextArea(Rows = 4)]
            public string? Content { get; set; }

        }

    }
}
