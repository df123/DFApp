using System.Collections.Generic;
using DFApp.Lottery;

namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 复式投注输入 DTO
    /// </summary>
    public class CompoundLotteryInputDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public CompoundLotteryInputDto()
        {
            LotteryType = string.Empty;
            RedNumbers = new List<string>();
            BlueNumbers = new List<string>();
            KL8Numbers = new List<string>();
        }

        /// <summary>
        /// 期号
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// 彩票类型（英文代码，如 "ssq"、"kl8"）
        /// </summary>
        public string LotteryType { get; set; }

        /// <summary>
        /// 双色球红球号码列表
        /// </summary>
        public List<string> RedNumbers { get; set; }

        /// <summary>
        /// 双色球蓝球号码列表
        /// </summary>
        public List<string> BlueNumbers { get; set; }

        /// <summary>
        /// 快乐8号码列表
        /// </summary>
        public List<string> KL8Numbers { get; set; }

        /// <summary>
        /// 快乐8玩法类型（选号个数）
        /// </summary>
        public LotteryKL8PlayType? PlayType { get; set; }
    }
}
