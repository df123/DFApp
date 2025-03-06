using System.ComponentModel.DataAnnotations;
using DFApp.Lottery.Validation;
using Volo.Abp.Validation;

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
        [Display(Name = "LotterySimulation:Generate:Count")]
        public int Count { get; set; }

        /// <summary>
        /// 彩票类型
        /// </summary>
        [Required]
        [Display(Name = "LotterySimulation:Generate:GameType")]
        public LotteryGameType GameType { get; set; }

        /// <summary>
        /// 期号 (格式：yyyyxxx，例如：2023001)
        /// </summary>
        [Required]
        [TermNumberFormat]
        [Display(Name = "LotterySimulation:Generate:TermNumber")]
        public int TermNumber { get; set; }

        /// <summary>
        /// 玩法类型
        /// </summary>
        [Display(Name = "LotterySimulation:Generate:PlayType")]
        public LotteryK8PlayType PlayType { get; set; }
    }
}
