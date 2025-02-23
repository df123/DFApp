using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DFApp.Lottery.Simulation;

namespace DFApp.Web.Pages.Lottery.Simulation
{
    public class DeleteByTermNumberModalModel : DFAppPageModel
    {
        [BindProperty]
        public int TermNumber { get; set; }

        private readonly ILotterySimulationService _lotterySimulationService;

        public DeleteByTermNumberModalModel(ILotterySimulationService lotterySimulationService)
        {
            _lotterySimulationService = lotterySimulationService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _lotterySimulationService.DeleteByTermNumberAsync(TermNumber);
            return NoContent();
        }
    }
}
