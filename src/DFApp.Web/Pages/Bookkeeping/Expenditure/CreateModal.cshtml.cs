using DFApp.Bookkeeping.Expenditure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;

namespace DFApp.Web.Pages.Bookkeeping.Expenditure
{
    public class CreateModalModel : DFAppPageModel
    {
        [BindProperty]
        public CreateExpenditureViewModel Expenditures { get; set; }

        public List<SelectListItem> Categorys { get; set; }

        private readonly IBookkeepingExpenditureService _bookkeepingExpenditureService;

        public CreateModalModel(IBookkeepingExpenditureService bookkeepingExpenditureService)
        {
            _bookkeepingExpenditureService = bookkeepingExpenditureService;
        }

        public async Task OnGetAsync()
        {
            Expenditures = new CreateExpenditureViewModel();
            Categorys = (await _bookkeepingExpenditureService.GetCategoryLookupDto())
                .Select(x => new SelectListItem(x.Category,x.CategoryId.ToString()))
                .ToList();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            await _bookkeepingExpenditureService.CreateAsync(
                ObjectMapper.Map<CreateExpenditureViewModel, CreateUpdateBookkeepingExpenditureDto>(Expenditures)
                );
            return NoContent();
        }


        public class CreateExpenditureViewModel
        {
            [Required]
            [DataType(DataType.Date)]
            public DateTime ExpenditureDate { get; set; }

            [Required]
            public decimal Expenditure { get; set; }

            [Required]
            [SelectItems(nameof(Categorys))]
            [DisplayName("Category")]
            public long CategoryId { get; set; }
        }

    }
}
