using DFApp.Lottery.Simulation;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DFApp.Web.Pages.Lottery.Simulation
{
    public class GenerateRandomNumbersModalModel : DFAppPageModel
    {
        [BindProperty]
        public GenerateRandomNumbersDto Input { get; set; }

        private readonly ILotterySimulationService _lotterySimulationService;

        public GenerateRandomNumbersModalModel(ILotterySimulationService lotterySimulationService)
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
