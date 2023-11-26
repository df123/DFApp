using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DF.Telegram.Lottery
{
    public class LotteryPrizegradesDto : AuditedEntityDto<long>
    {
        public int Type { get; set; }

        public string? TypeNum { get; set; }

        public string? TypeMoney { get; set; }

    }
}
