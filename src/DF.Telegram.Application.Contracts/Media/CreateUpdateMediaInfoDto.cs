using System;

namespace DF.Telegram.Media
{
    public class CreateUpdateMediaInfoDto
    {
        public long AccessHash { get; set; }
        public long TID { get; set; }
        public long Size { get; set; }
        public string? SavePath { get; set; }
        public string? ValueSHA1 { get; set; }
        public string? MimeType { get; set; }
        public string? Title { get; set; }
    }
}
