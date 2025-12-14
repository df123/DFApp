using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Aria2.Request
{
    public class Aria2RequestDto
    {
        [JsonPropertyName("jsonrpc")]
        public string JSONRPC { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("params")]
        public IList<object> Params { get; set; }
    }
}
