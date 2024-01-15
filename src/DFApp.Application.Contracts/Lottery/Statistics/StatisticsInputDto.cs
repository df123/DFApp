using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Lottery.Statistics
{
    public class StatisticsInputDto
    {
        public string? PurchasedPeriod { get; set; }
        public string? WinningPeriod { get; set; }
        public string LotteryType { get; set; }
    }
}
