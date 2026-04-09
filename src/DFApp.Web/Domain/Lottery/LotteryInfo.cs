using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Lottery
{
    [SugarTable("AppLottery")]
    public class LotteryInfo : AuditedEntity<long>
    {
        public int IndexNo { get; set; }
        public string Number { get; set; }
        public string ColorType { get; set; }
        public string LotteryType { get; set; }
        public int GroupId { get; set; }
    }
}
