using System.Collections.Generic;
using DFApp.Lottery;

namespace DFApp.Web.DTOs.Lottery.Simulation
{
    /// <summary>
    /// 统计数据 DTO，包含各期号的投注金额和中奖金额汇总
    /// </summary>
    public class StatisticsDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public StatisticsDto()
        {
            Terms = new List<int>();
            PurchaseAmounts = new List<decimal>();
            WinningAmounts = new List<decimal>();
            PurchaseAmountsByType = new Dictionary<LotteryKL8PlayType, List<decimal>>();
            WinningAmountsByType = new Dictionary<LotteryKL8PlayType, List<decimal>>();
        }

        /// <summary>
        /// 期号列表
        /// </summary>
        public List<int> Terms { get; set; }

        /// <summary>
        /// 各期号的投注金额
        /// </summary>
        public List<decimal> PurchaseAmounts { get; set; }

        /// <summary>
        /// 各期号的中奖金额
        /// </summary>
        public List<decimal> WinningAmounts { get; set; }

        /// <summary>
        /// 按玩法类型分组的投注金额（快乐8统计使用）
        /// </summary>
        public Dictionary<LotteryKL8PlayType, List<decimal>> PurchaseAmountsByType { get; set; }

        /// <summary>
        /// 按玩法类型分组的中奖金额（快乐8统计使用）
        /// </summary>
        public Dictionary<LotteryKL8PlayType, List<decimal>> WinningAmountsByType { get; set; }
    }
}
