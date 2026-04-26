using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Lottery.Statistics
{
    public class StatisticsWinItemRequestDto : PagedAndSortedResultRequestDto
    {
        public string? PurchasedPeriod { get; set; }
        public string? WinningPeriod { get; set; }
        public string? LotteryType { get; set; }
    }
}
