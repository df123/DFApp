using System;

namespace DFApp.Lottery
{
    /// <summary>
    /// 彩票信息 DTO（旧命名空间，用于服务层过渡期兼容）
    /// </summary>
    public class LotteryDto
    {
        public long Id { get; set; }
        public int IndexNo { get; set; }
        public string Number { get; set; } = string.Empty;
        public string ColorType { get; set; } = string.Empty;
        public string LotteryType { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public Guid? LastModifierId { get; set; }
    }
}
