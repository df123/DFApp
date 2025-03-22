using DFApp.Lottery.Simulation.KL8;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Lottery.Simulation.K8
{
    public class K8GenerateModalModel : DFAppPageModel
    {
        [BindProperty]
        public GenerateRandomNumbersDto Input { get; set; }

        private readonly ILotteryKL8SimulationService _lotterySimulationService;

        public K8GenerateModalModel(ILotteryKL8SimulationService lotterySimulationService)
        {
            _lotterySimulationService = lotterySimulationService;
        }

        public void OnGet()
        {
            Input = new GenerateRandomNumbersDto();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _lotterySimulationService.GenerateRandomNumbersAsync(Input);
            return NoContent();
        }
    }
}