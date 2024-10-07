using System;

namespace DFApp.Media
{
    public class CreateUpdateMediaInfoDto
    {
        public long MediaId { get; set; }
        public long ChatId { get; set; }
        public required string ChatTitle { get; set; }
        public string? Message { get; set; }
        public long Size { get; set; }
        public required string SavePath { get; set; }
        public required string MD5 { get; set; }
        public required string MimeType { get; set; }
        public bool IsExternalLinkGenerated { get; set; }
    }
}
