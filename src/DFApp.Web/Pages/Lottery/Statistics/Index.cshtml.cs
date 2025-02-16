using DFApp.Lottery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Web.Pages.Lottery.Statistics
{
    public class IndexModel : PageModel
    {
        private readonly ILotteryService _lotteryAppService;

        public List<SelectListItem> LotteryTypes { get; set; } = new();

        public IndexModel(ILotteryService lotteryService)
        {
            _lotteryAppService = lotteryService;
        }

        public void OnGet()
        {
            var types = _lotteryAppService.GetLotteryConst();
            LotteryTypes = types.Select(t => new SelectListItem
            {
                Value = t.LotteryTypeEng,
                Text = t.LotteryType
            }).ToList();
        }

        public async Task<JsonResult> OnGetStatisticsDataAsync(string purchasedPeriod, string winningPeriod, string lotteryType)
        {
            var data = await _lotteryAppService.GetStatisticsWin(purchasedPeriod, winningPeriod, lotteryType);
            return new JsonResult(data);
        }
    }
}
