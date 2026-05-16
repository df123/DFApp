using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Lottery
{
    /// <summary>
    /// 模拟购买彩票
    /// </summary>
    [SugarTable("AppLotterySimulation")]
    public class LotterySimulation : AuditedEntity<Guid>
    {
        /// <summary>
        /// Guid 类型主键不支持数据库自增，覆盖基类属性移除 IsIdentity
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public new Guid Id { get; set; }

        /// <summary>
        /// 期号 (格式：yyyyxxx，例如：2023001)
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
