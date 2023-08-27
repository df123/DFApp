using System;

namespace DF.Telegram.Media
{
    public class CreateUpdateMediaInfoDto
    {
        public long AccessHash { get; set; }
        public long TID { get; set; }
        public long Size { get; set; }
        public bool IsDownload { get; set; }
        public bool IsReturn { get; set; }
        public DateTime? TaskComplete { get; set; }
#nullable disable
        public string SavePath { get; set; }
        public string ValueSHA1 { get; set; }
#nullable restore
    }
}
