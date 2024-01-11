using DFApp.Bookkeeping.Category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Bookkeeping.Category
{
    public class EditModalModel : DFAppPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public CreateUpdateBookkeepingCategoryDto CategoryDto { get; set; }

        private readonly IBookkeepingCategoryService _bookkeepingCategoryService;

        public EditModalModel(IBookkeepingCategoryService bookkeepingCategoryService)
        {
            _bookkeepingCategoryService = bookkeepingCategoryService;
        }

        public async Task OnGetAsync()
        {
            var catetory = await _bookkeepingCategoryService.GetAsync(Id);
            CategoryDto = ObjectMapper.Map<BookkeepingCategoryDto, CreateUpdateBookkeepingCategoryDto>(catetory);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _bookkeepingCategoryService.UpdateAsync(Id, CategoryDto);
            return NoContent();
        }


    }
}
