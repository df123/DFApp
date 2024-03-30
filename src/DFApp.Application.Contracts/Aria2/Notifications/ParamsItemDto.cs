using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DFApp.Aria2.Notifications
{
    public class ParamsItemDto
    {
        [JsonPropertyName("gid")]
        public string GID { get; set; }
    }
}
