using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Lottery
{
    public class CreateUpdateLotteryResultDto
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? DetailsLink { get; set; }

        public string? VideoLink { get; set; }

        public string? Date { get; set; }

        public string? Week { get; set; }

        public string? Red { get; set; }

        public string? Blue { get; set; }

        public string? Blue2 { get; set; }

        public string? Sales { get; set; }

        public string? PoolMoney { get; set; }

        public string? Content { get; set; }

        public string? AddMoney { get; set; }

        public string? AddMoney2 { get; set; }

        public string? Msg { get; set; }

        public string? Z2Add { get; set; }

        public string? M2Add { get; set; }

        public List<CreateUpdateLotteryPrizegradesDto>? Prizegrades { get; set; }
    }
}
