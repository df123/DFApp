using System.Collections.Generic;

namespace DFApp.Lottery.Simulation.KL8
{
    public class StatisticsDto
    {
        public List<int> Terms { get; set; } = new();
        public Dictionary<LotteryKL8PlayType, List<decimal>> PurchaseAmountsByType { get; set; } = new();
        public Dictionary<LotteryKL8PlayType, List<decimal>> WinningAmountsByType { get; set; } = new();
    }
}