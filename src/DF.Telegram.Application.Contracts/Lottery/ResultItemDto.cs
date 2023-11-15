using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DF.Telegram.Lottery
{
    public class ResultItemDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("detailsLink")]
        public string? DetailsLink { get; set; }

        [JsonPropertyName("videoLink")]
        public string? VideoLink { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("week")]
        public string? Week { get; set; }

        [JsonPropertyName("red")]
        public string? Red { get; set; }

        [JsonPropertyName("blue")]
        public string? Blue { get; set; }

        [JsonPropertyName("blue2")]
        public string? Blue2 { get; set; }

        [JsonPropertyName("sales")]
        public string? Sales { get; set; }

        [JsonPropertyName("poolmoney")]
        public string? PoolMoney { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("addmoney")]
        public string? AddMoney { get; set; }

        [JsonPropertyName("addmoney2")]
        public string? AddMoney2 { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("z2add")]
        public string? Z2Add { get; set; }

        [JsonPropertyName("m2add")]
        public string? M2Add { get; set; }

        [JsonPropertyName("prizegrades")]
        public List<PrizegradesItemDto>? Prizegrades { get; set; }
    }
}
