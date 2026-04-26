using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Web.DTOs.Aria2
{
    public class FilesItemDto
    {
        [JsonPropertyName("completedLength")]
        public string CompletedLength { get; set; }

        [JsonPropertyName("index")]
        public string Index { get; set; }

        [JsonPropertyName("length")]
        public string Length { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("selected")]
        public string Selected { get; set; }

        [JsonPropertyName("uris")]
        public List<UrisItemDto> Uris { get; set; }
    }
}
