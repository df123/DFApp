using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Lottery.Statistics
{
    public class StatisticsWinItemRequestDto : PagedAndSortedResultRequestDto
    {
        public string? PurchasedPeriod { get; set; }
        public string? WinningPeriod { get; set; }
        public string? LotteryType { get; set; }
    }
}