using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Lottery
{
    public class LotteryGroupDto : AuditedEntityDto<long>
    {
        public int IndexNo { get; set; }
        public string LotteryType { get; set; }
        public int GroupId { get; set; }
        public string Numbers { get; set; } // 拼接后的号码，格式如"01,02,03,04,05,06,07"
        public string RedNumbers { get; set; } // 红球号码
        public string BlueNumber { get; set; } // 蓝球号码
    }
}
