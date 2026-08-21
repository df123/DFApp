using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Lottery
{
    [SugarTable("AppLotteryPrizegrades")]
    public class LotteryPrizegrades : AuditedEntity<long>
    {
        public long LotteryResultId { get; set; }
        public string? Type { get; set; }

        public string? TypeNum { get; set; }

        public string? TypeMoney { get; set; }

        // 存量表结构保留列（NOT NULL 无默认值），实体不携带会导致插入失败，固定写 "{}"
        public string ExtraProperties { get; set; } = "{}";

        [SugarColumn(IsIgnore = true)]
        public LotteryResult Result { get; set; }
    }
}
