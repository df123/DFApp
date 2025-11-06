using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DFApp.Lottery
{
    /// <summary>
    /// 复式投注输入DTO
    /// </summary>
    public class CompoundLotteryInputDto
    {
        /// <summary>
        /// 期号
        /// </summary>
        [Required]
        public int Period { get; set; }

        /// <summary>
        /// 彩票类型
        /// </summary>
        [Required]
        public string LotteryType { get; set; } = string.Empty;

        /// <summary>
        /// 红球号码列表
        /// </summary>
        public List<string> RedNumbers { get; set; } = new List<string>();

        /// <summary>
        /// 蓝球号码列表（仅双色球）
        /// </summary>
        public List<string> BlueNumbers { get; set; } = new List<string>();

        /// <summary>
        /// 快乐8玩法类型
        /// </summary>
        public LotteryKL8PlayType? PlayType { get; set; }

        /// <summary>
        /// 快乐8号码列表
        /// </summary>
        public List<string> KL8Numbers { get; set; } = new List<string>();
    }
}
