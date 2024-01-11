using DFApp.Bookkeeping.Category;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Bookkeeping.Category
{
    public class CreateModalModel : DFAppPageModel
    {
        [BindProperty]
        public CreateCategoryViewModel Categorys { get; set; }

        private readonly IBookkeepingCategoryService _bookkeepingCategoryService;

        public CreateModalModel(IBookkeepingCategoryService bookkeepingCategoryService)
        {
            _bookkeepingCategoryService = bookkeepingCategoryService;
        }

        public async Task OnGetAsync()
        {
            Categorys = new CreateCategoryViewModel();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _bookkeepingCategoryService.CreateAsync(
                ObjectMapper.Map<CreateCategoryViewModel, CreateUpdateBookkeepingCategoryDto>(Categorys)
                );
            return NoContent();
        }

        public class CreateCategoryViewModel
        {
            [Required]
            public string Category { get; set; }
        }

    }
}
