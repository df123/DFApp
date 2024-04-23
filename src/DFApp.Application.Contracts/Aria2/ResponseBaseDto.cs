using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DFApp.Aria2
{
    public class ResponseBaseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
