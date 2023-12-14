using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace DF.Telegram.Lottery
{
    public class LotteryInfo : AuditedAggregateRoot<long>, ISoftDelete
    {
        public int IndexNo { get; set; }
        public string? Number { get; set; }
        public string? ColorType { get; set; }
        public int GroupId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
