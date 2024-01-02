using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DFApp.Lottery
{
    public class LotteryInputDto
    {

        [JsonPropertyName("state")]
        public int State { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }

        [JsonPropertyName("pageNo")]
        public int PageNo { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("Tflag")]
        public int Tflag { get; set; }

        [JsonPropertyName("result")]
        public List<ResultItemDto>? Result { get; set; }
    }
}
