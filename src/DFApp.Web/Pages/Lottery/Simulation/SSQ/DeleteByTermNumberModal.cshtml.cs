using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DFApp.Lottery.Simulation;
using DFApp.Lottery.Simulation.KL8;

namespace DFApp.Web.Pages.Lottery.Simulation
{
    public class DeleteByTermNumberModalModel : DFAppPageModel
    {
        [BindProperty]
        public DeleteByTermNumberDto Input { get; set; }

        private readonly ILotteryKL8SimulationService _lotterySimulationService;

        public DeleteByTermNumberModalModel(ILotteryKL8SimulationService lotterySimulationService)
        {
            _lotterySimulationService = lotterySimulationService;
        }

        public void OnGet()
        {
            Input = new DeleteByTermNumberDto();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _lotterySimulationService.DeleteByTermNumberAsync(Input.TermNumber);
            return NoContent();
        }
    }
}
