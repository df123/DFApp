using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Web.DTOs.Aria2
{
    public class Aria2NotificationDto : ResponseBaseDto
    {
        [JsonPropertyName("jsonrpc")]
        public string JSONRPC { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("params")]
        public List<ParamsItemDto> Params { get; set; }
    }
}
