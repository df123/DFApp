using System.Text.Json.Serialization;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.DTOs.Lottery
{
    public class PrizegradesItemDto
    {

        [JsonPropertyName("type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Type { get; set; }

        [JsonPropertyName("typenum")]
        public string? TypeNum { get; set; }

        [JsonPropertyName("typemoney")]
        public string? TypeMoney { get; set; }
    }
}
