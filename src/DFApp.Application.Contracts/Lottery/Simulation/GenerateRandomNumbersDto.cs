using System.ComponentModel.DataAnnotations;

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
        [Range(1, 100)]
        public int Count { get; set; }

        /// <summary>
        /// 彩票类型
        /// </summary>
        [Required]
        public LotteryGameType GameType { get; set; }

        /// <summary>
        /// 期号
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int TermNumber { get; set; }
    }
}
