using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace DFApp.Lottery
{
    /// <summary>
    /// 模拟购买彩票
    /// </summary>
    public class LotterySimulation : AuditedAggregateRoot<Guid>
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
        public required LotteryBallType BallType { get; set; }
        /// <summary>
        /// 彩票类型
        /// </summary>
        public required LotteryGameType GameType { get; set; }
        /// <summary>
        /// 分组ID
        /// </summary>
        public int GroupId { get; set; }
    }
}
