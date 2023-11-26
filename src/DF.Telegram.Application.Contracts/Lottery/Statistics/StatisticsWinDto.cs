using System;
using System.Collections.Generic;
using System.Text;

namespace DF.Telegram.Lottery.Statistics
{
    public class StatisticsWinDto
    {
        public string? Code { get; set; }
        public int BuyAmount { get; set; }
        public int WinAmount { get; set; }
    }
}
