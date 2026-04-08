using System.Collections.Generic;

namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 彩票统计 DTO
    /// </summary>
    public class LotteryStatisticsDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LotteryStatisticsDto()
        {
            Details = new List<LotteryStatisticsItemDto>();
        }

        /// <summary>
        /// 购买总额
        /// </summary>
        public decimal TotalPurchaseAmount { get; set; }

        /// <summary>
        /// 中奖总额
        /// </summary>
        public decimal TotalWinningAmount { get; set; }

        /// <summary>
        /// 净收益（中奖总额 - 购买总额）
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// 统计详情列表
        /// </summary>
        public List<LotteryStatisticsItemDto> Details { get; set; }
    }

    /// <summary>
    /// 彩票统计项 DTO
    /// </summary>
    public class LotteryStatisticsItemDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LotteryStatisticsItemDto()
        {
        }

        /// <summary>
        /// 期号
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 购买金额
        /// </summary>
        public decimal PurchaseAmount { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal WinningAmount { get; set; }
    }
}
