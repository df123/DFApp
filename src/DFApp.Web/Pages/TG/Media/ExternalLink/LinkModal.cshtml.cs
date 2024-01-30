using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using DFApp.Media.ExternalLink;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.TG.Media.ExternalLink
{
    public class LinkModalModel : PageModel
    {
        //[HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public ContentViewModel ContentDto { get; set; }

        private readonly IExternalLinkService _externalLinkService;

        public LinkModalModel(IExternalLinkService externalLinkService)
        {
            _externalLinkService = externalLinkService;
        }

        public async Task OnGetAsync()
        {
            ContentDto = new ContentViewModel();

            var dto = await _externalLinkService.GetAsync(Id);

            ContentDto.Content = dto.LinkContent;
        }

        public class ContentViewModel
        {
            [DisplayName("连接地址")]
            [TextArea(Rows = 4)]
            public string Content { get; set; }

        }

    }
}
