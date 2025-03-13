using System;

namespace DFApp.Lottery.Simulation.KL8
{
    public class CreateUpdateLotterySimulationDto
    {
        /// <summary>
        /// 期号
        /// </summary>
        public int TermNumber { get; set; }

        /// <summary>
        /// 号码
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 彩票球类型
        /// </summary>
        public LotteryBallType BallType { get; set; }

        /// <summary>
        /// 彩票类型
        /// </summary>
        public LotteryGameType GameType { get; set; }

        /// <summary>
        /// 分组ID
        /// </summary>
        public int GroupId { get; set; }
    }
}
