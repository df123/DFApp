using System;
using Volo.Abp.Application.Dtos;

namespace DFApp.Lottery.Simulation.SSQ
{
    public class LotterySimulationDto : AuditedEntityDto<Guid>
    {
        /// <summary>
        /// 期号
        /// </summary>
        public int TermNumber { get; set; }

        /// <summary>
        /// 号码
        /// </summary>
        public string? RedNumbers { get; set; }

        public string? BlueNumber { get; set; }

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
