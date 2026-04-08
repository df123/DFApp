using System.Collections.Generic;

namespace DFApp.Web.DTOs.Lottery.Statistics
{
    public class LotteryStructure
    {
        public LotteryStructure()
        {
            Reds = new List<string>(6);
            Blue = string.Empty;
        }
        public List<string> Reds { get; set; }
        public string? Blue { get; set; }

    }
}
