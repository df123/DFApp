using System;
using System.Collections.Generic;

namespace DFApp.Lottery
{
    /// <summary>
    /// 复式投注响应DTO
    /// </summary>
    public class CompoundLotteryResultDto
    {
        /// <summary>
        /// 总组合数
        /// </summary>
        public int TotalCombinations { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 创建的彩票列表
        /// </summary>
        public List<LotteryDto> CreatedLotteries { get; set; } = new List<LotteryDto>();

        /// <summary>
        /// 组合详情
        /// </summary>
        public List<string> CombinationDetails { get; set; } = new List<string>();
    }
}
