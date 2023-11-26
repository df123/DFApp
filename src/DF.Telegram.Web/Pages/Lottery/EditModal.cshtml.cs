using DF.Telegram.Lottery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;

namespace DF.Telegram.Web.Pages.Lottery
{
    public class EditModalModel : TelegramPageModel
    {

        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public CreateUpdateLotteryDto LotteryDto { get; set; }

        private readonly ILotteryService _lotteryService;

        public EditModalModel(ILotteryService lotteryService)
        {
            _lotteryService = lotteryService;
        }
        public async Task OnGetAsync()
        {
            var lottery = await _lotteryService.GetAsync(Id);
            LotteryDto = ObjectMapper.Map<LotteryDto, CreateUpdateLotteryDto>(lottery);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _lotteryService.UpdateAsync(Id, LotteryDto);
            return NoContent();
        }
    }
}
