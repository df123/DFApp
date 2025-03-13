using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;

namespace DFApp.Web.Pages.Lottery.Simulation.K8
{
    public class K8GenerateModalModel : DFAppPageModel
    {
        [BindProperty]
        public GenerateViewModel Input { get; set; }

        public void OnGet()
        {
            Input = new GenerateViewModel();
        }

        public class GenerateViewModel
        {
            [Required]
            [Display(Name = "GroupId")]
            public string GroupId { get; set; }

            [Required]
            [Display(Name = "Count")]
            public int Count { get; set; }
        }
    }
}