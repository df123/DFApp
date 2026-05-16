using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Web.DTOs.Aria2
{
    public class Aria2ResponseDto : ResponseBaseDto
    {
        [JsonPropertyName("jsonrpc")]
        public string JSONRPC { get; set; }
    }
}
