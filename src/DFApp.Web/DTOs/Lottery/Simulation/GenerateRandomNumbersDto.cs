using DFApp.Lottery;

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
        /// 期号
        /// </summary>
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
