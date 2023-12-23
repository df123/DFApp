using System;
using System.Collections.Generic;
using System.Text;

namespace DF.Telegram.Lottery
{
    public class LotteryCombinationDto
    {
        public int Period { get; set; }
        public List<string>? Blues { get; set; }
        public List<string>? Reds { get; set; }

    }
}
