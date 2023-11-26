using System;
using System.Collections.Generic;
using System.Text;

namespace DF.Telegram.Lottery.Statistics
{
    public class StatisticsWinItemDto
    {
        public StatisticsWinItemDto() 
        { 
            BuyLottery = new LotteryStructure();
            WinLottery = new LotteryStructure();
            BuyLotteryString = string.Empty;
            WinLotteryString = string.Empty;
        }

        public string? Code { get;set; }
        public string? WinCode { get;set; }  
        public int WinAmount { get;set; }

        public LotteryStructure BuyLottery { get;set; }
        public string BuyLotteryString { get; set; }

        public LotteryStructure WinLottery { get;set; }
        public string WinLotteryString { get; set; }

    }
}
