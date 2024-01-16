using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Lottery
{
    public class LotteryDto : AuditedEntityDto<long>
    {
        public int IndexNo { get; set; }
        public string Number { get; set; }
        public string ColorType { get; set; }
        public string LotteryType { get; set; }
        public int GroupId { get; set; }
    }
}
