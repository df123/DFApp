using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Lottery
{
    public class LotteryGroupDto : AuditedEntityDto<long>
    {
        public int IndexNo { get; set; }
        public string LotteryType { get; set; }
        public int GroupId { get; set; }
        public string RedNumbers { get; set; } // 红球号码
        public string BlueNumber { get; set; } // 蓝球号码
    }
}
