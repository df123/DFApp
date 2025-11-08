using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Lottery
{
    public class LotteryPrizegradesDto : AuditedEntityDto<long>
    {
        public string? Type { get; set; }

        public string? TypeNum { get; set; }

        public string? TypeMoney { get; set; }

    }
}
