using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Aria2.Response
{
    public class ResultDto
    {
        [JsonPropertyName("downloadSpeed")]
        public string DownloadSpeed { get; set; }

        [JsonPropertyName("numActive")]
        public string NumActive { get; set; }

        [JsonPropertyName("numStopped")]
        public string NumStopped { get; set; }

        [JsonPropertyName("numWaiting")]
        public string NumWaiting { get; set; }

        [JsonPropertyName("uploadSpeed")]
        public string UploadSpeed { get; set; }
    }
}
