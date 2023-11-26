using System;
using System.Collections.Generic;
using System.Text;

namespace DF.Telegram.Lottery.Statistics
{
    public class LotteryStructure
    {
        public LotteryStructure() 
        {
            Reds = new List<string>(6);
            Blue = string.Empty;
        }
        public List<string> Reds { get; set; }
        public string Blue { get; set; }

    }
}
