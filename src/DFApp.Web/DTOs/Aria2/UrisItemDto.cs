using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Web.DTOs.Aria2
{
    public class UrisItemDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}
