using System.ComponentModel.DataAnnotations;
using DFApp.Lottery.Validation;

namespace DFApp.Lottery.Simulation
{
    /// <summary>
    /// 生成随机号码的请求参数
    /// </summary>
    public class GenerateRandomNumbersDto
    {
        /// <summary>
        /// 生成组数
        /// </summary>
        [Required]
        [Range(1, 1000)]
        public int Count { get; set; }

        /// <summary>
        /// 彩票类型
        /// </summary>
        [Required]
        public LotteryGameType GameType { get; set; }

        /// <summary>
        /// 期号 (格式：yyyyxxx，例如：2023001)
        /// </summary>
        [Required]
        [TermNumberFormat]
        public int TermNumber { get; set; }
    }
}
