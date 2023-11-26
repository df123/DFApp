using System;
using System.Collections.Generic;
using System.Text;

namespace DF.Telegram.Lottery
{
    public class CreateUpdateLotteryPrizegradesDto
    {
        public int Type { get; set; }

        public string? TypeNum { get; set; }

        public string? TypeMoney { get; set; }

    }
}
