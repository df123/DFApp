using System.Collections.Generic;

namespace DFApp.Web.DTOs.Lottery.Simulation
{
    /// <summary>
    /// 中奖统计 DTO，包含各投注组的中奖详情和总中奖金额
    /// </summary>
    public class WinningStatisticsDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public WinningStatisticsDto()
        {
            WinningDetails = new List<WinningDetailDto>();
            TotalAmount = 0;
        }

        /// <summary>
        /// 各投注组的中奖详情
        /// </summary>
        public List<WinningDetailDto> WinningDetails { get; set; }

        /// <summary>
        /// 总中奖金额
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// 单组投注的中奖详情
    /// </summary>
    public class WinningDetailDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public WinningDetailDto()
        {
        }

        /// <summary>
        /// 投注组ID
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 红球命中数
        /// </summary>
        public int RedMatches { get; set; }

        /// <summary>
        /// 蓝球命中数
        /// </summary>
        public int BlueMatches { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal WinningAmount { get; set; }
    }
}
