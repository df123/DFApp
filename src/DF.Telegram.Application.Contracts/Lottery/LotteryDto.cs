using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DF.Telegram.Lottery
{
    public class LotteryDto : AuditedEntityDto<long>
    {
        public int IndexNo { get; set; }
        public string? Number { get; set; }
        public string? ColorType { get; set; }
        public int GroupId { get; set; }
    }
}
