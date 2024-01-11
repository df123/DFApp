using DFApp.Bookkeeping.Expenditure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Bookkeeping.Expenditure
{
    public class EditModalModel : DFAppPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public CreateUpdateBookkeepingExpenditureDto ExpenditureDto { get; set; }

        private readonly IBookkeepingExpenditureService _bookkeepingExpenditureService;

        public EditModalModel(IBookkeepingExpenditureService bookkeepingExpenditureService)
        {
            _bookkeepingExpenditureService = bookkeepingExpenditureService;
        }
        
        public async Task OnGetAsync()
        {
            var ex = await _bookkeepingExpenditureService.GetAsync(Id);
            ExpenditureDto = ObjectMapper.Map<BookkeepingExpenditureDto,CreateUpdateBookkeepingExpenditureDto>(ex);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _bookkeepingExpenditureService.UpdateAsync(Id, ExpenditureDto);
            return NoContent();
        }



    }
}
