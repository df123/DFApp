using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DFApp.Lottery
{
    public class PrizegradesItemDto
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("typenum")]
        public string? TypeNum { get; set; }

        [JsonPropertyName("typemoney")]
        public string? TypeMoney { get; set; }
    }
}
