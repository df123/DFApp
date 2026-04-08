using System;

namespace DFApp.Lottery.Simulation.KL8
{
    /// <summary>
    /// 快乐8模拟购买 DTO（旧命名空间，用于服务层过渡期兼容）
    /// </summary>
    public class LotterySimulationDto
    {
        public Guid Id { get; set; }
        public int TermNumber { get; set; }
        public LotteryBallType BallType { get; set; }
        public LotteryGameType GameType { get; set; }
        public int GroupId { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public Guid? LastModifierId { get; set; }
    }
}
