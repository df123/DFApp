using DFApp.Bookkeeping.Expenditure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;

namespace DFApp.Web.Pages.Bookkeeping.Expenditure
{
    public class EditModalModel : DFAppPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public UpdateExpenditureViewModel ExpenditureDto { get; set; }

        public List<SelectListItem> Categorys { get; set; }

        private readonly IBookkeepingExpenditureService _bookkeepingExpenditureService;

        public EditModalModel(IBookkeepingExpenditureService bookkeepingExpenditureService)
        {
            _bookkeepingExpenditureService = bookkeepingExpenditureService;
        }

        public async Task OnGetAsync()
        {
            var ex = await _bookkeepingExpenditureService.GetAsync(Id);
            ExpenditureDto = ObjectMapper.Map<BookkeepingExpenditureDto, UpdateExpenditureViewModel>(ex);

            Categorys = (await _bookkeepingExpenditureService.GetCategoryLookupDto())
                .Select(x => new SelectListItem(x.Category, x.CategoryId.ToString()))
                .ToList();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _bookkeepingExpenditureService.UpdateAsync(Id, ObjectMapper.Map<UpdateExpenditureViewModel, CreateUpdateBookkeepingExpenditureDto>(ExpenditureDto));
            return NoContent();
        }

        public class UpdateExpenditureViewModel
        {
            [Required]
            [DataType(DataType.Date)]
            [DisplayName("日期")]
            public DateTime ExpenditureDate { get; set; }

            [Required]
            [DisplayName("支出金额")]
            public decimal Expenditure { get; set; }

            [Required]
            [SelectItems(nameof(Categorys))]
            [DisplayName("类别")]
            public long CategoryId { get; set; }

            [DisplayName("备注")]
            public string? Remark { get; set; }

            [Required]
            [DisplayName("自用")]
            public bool IsBelongToSelf { get; set; }

        }

    }
}
