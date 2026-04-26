namespace DFApp.Lottery.Simulation.KL8
{
    /// <summary>
    /// 快乐8模拟购买创建/更新 DTO（旧命名空间，用于服务层过渡期兼容）
    /// </summary>
    public class CreateUpdateLotterySimulationDto
    {
        public int TermNumber { get; set; }
        public int Number { get; set; }
        public LotteryBallType BallType { get; set; }
        public LotteryGameType GameType { get; set; }
        public int GroupId { get; set; }
    }
}
