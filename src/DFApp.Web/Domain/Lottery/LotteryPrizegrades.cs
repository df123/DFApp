using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Lottery
{
    [SugarTable("LotteryPrizegrades")]
    public class LotteryPrizegrades : AuditedEntity<long>
    {
        public long LotteryResultId { get; set; }
        public string? Type { get; set; }

        public string? TypeNum { get; set; }

        public string? TypeMoney { get; set; }

        [SugarColumn(IsIgnore = true)]
        public LotteryResult Result { get; set; }
    }
}
