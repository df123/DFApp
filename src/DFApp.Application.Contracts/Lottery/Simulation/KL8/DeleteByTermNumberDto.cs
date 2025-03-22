using System.ComponentModel.DataAnnotations;
using DFApp.Lottery.Validation;

namespace DFApp.Lottery.Simulation.KL8
{
    public class DeleteByTermNumberDto
    {
        [Required]
        [TermNumberFormat]
        [Display(Name = "LotterySimulation:TermNumber")]
        public int TermNumber { get; set; }
    }
}
