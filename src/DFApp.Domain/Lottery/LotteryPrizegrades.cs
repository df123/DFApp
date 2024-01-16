﻿using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace DFApp.Lottery
{
    public class LotteryPrizegrades : AuditedAggregateRoot<long>, ISoftDelete
    {
        public long LotteryResultId { get; set; }   
        public string? Type { get; set; }

        public string? TypeNum { get; set; }

        public string? TypeMoney { get; set; }

        public bool IsDeleted { get; set; }

        public LotteryResult Result { get; set; }
    }
}
