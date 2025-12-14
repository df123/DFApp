using System;
using System.Collections.Generic;

namespace DFApp.Aria2
{
    public class AddDownloadRequestDto
    {
        public List<string> Urls { get; set; } = new List<string>();
        public string? SavePath { get; set; }
        public Dictionary<string, object>? Options { get; set; }
        public bool VideoOnly { get; set; }
    }

    public class AddDownloadResponseDto
    {
        public string Id { get; set; } = string.Empty;
    }
}