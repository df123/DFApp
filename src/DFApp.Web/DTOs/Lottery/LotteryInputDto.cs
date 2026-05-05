using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFApp.Web.DTOs.Lottery
{
    public class LotteryInputDto
    {
        public LotteryInputDto()
        {
            Result = new List<ResultItemDto>();
        }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("pageNo")]
        public int PageNo { get; set; }

        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("result")]
        public List<ResultItemDto> Result { get; set; }
    }
}
