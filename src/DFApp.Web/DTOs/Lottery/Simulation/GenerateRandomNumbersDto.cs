using System.ComponentModel.DataAnnotations;
using DFApp.Lottery;
using DFApp.Web.DTOs.Lottery.Validation;

namespace DFApp.Web.DTOs.Lottery.Simulation
{
    /// <summary>
    /// 生成随机号码 DTO
    /// </summary>
    public class GenerateRandomNumbersDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public GenerateRandomNumbersDto()
        {
        }

        /// <summary>
        /// 期号（格式：yyyyxxx，例如：2023001）
        /// </summary>
        [Required]
        [TermNumberFormat]
        public int TermNumber { get; set; }

        /// <summary>
        /// 生成组数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 游戏类型（双色球/快乐8），仅双色球模拟需要指定
        /// </summary>
        public LotteryGameType GameType { get; set; }
    }
}
