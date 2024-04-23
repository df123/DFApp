using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Aria2.Response.TellStatus
{
    public class TellStatusResponseDto : Aria2ResponseDto
    {
        [JsonPropertyName("result")]
        public TellStatusResultDto? Result { get; set; }
    }
}
