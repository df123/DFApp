using System.Collections.Generic;

namespace DFApp.Lottery.Simulation
{
    /// <summary>
    /// 中奖统计结果
    /// </summary>
    public class WinningStatisticsDto
    {
        /// <summary>
        /// 总中奖金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 中奖详情列表
        /// </summary>
        public List<WinningDetailDto> WinningDetails { get; set; }
    }

    /// <summary>
    /// 中奖详情
    /// </summary>
    public class WinningDetailDto
    {
        /// <summary>
        /// 投注组号
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 红球匹配数
        /// </summary>
        public int RedMatches { get; set; }

        /// <summary>
        /// 蓝球匹配数
        /// </summary>
        public int BlueMatches { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal WinningAmount { get; set; }
    }
}
