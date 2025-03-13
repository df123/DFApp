using System.Collections.Generic;

namespace DFApp.Lottery.Simulation.KL8
{
    public class StatisticsDto
    {
        public List<int> Terms { get; set; }
        public List<decimal> PurchaseAmounts { get; set; }
        public List<decimal> WinningAmounts { get; set; }
        
        public StatisticsDto()
        {
            Terms = new List<int>();
            PurchaseAmounts = new List<decimal>();
            WinningAmounts = new List<decimal>();
        }
    }
}