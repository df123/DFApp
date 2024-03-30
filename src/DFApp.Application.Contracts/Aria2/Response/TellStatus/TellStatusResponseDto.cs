using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Aria2.Response.TellStatus
{
    public class TellStatusResponseDto:Aria2Response
    {
        [JsonPropertyName("result")]
        public Result Result { get; set; }
    }
}
