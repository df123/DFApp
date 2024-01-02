using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Lottery
{
    public class CreateUpdateLotteryDto
    {
        public int IndexNo { get; set; }
        public string? Number { get; set; }
        public string? ColorType { get; set; }
        public int GroupId { get; set; }
    }
}
