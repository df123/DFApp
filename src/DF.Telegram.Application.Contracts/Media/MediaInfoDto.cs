using System;
using Volo.Abp.Application.Dtos;

namespace DF.Telegram.Media
{
    public class MediaInfoDto: AuditedEntityDto<long>
    {
        public long AccessHash { get; set; }
        public long TID { get; set; }
        public long Size { get; set; }
        public bool IsDownload { get; set; }
        public bool IsReturn { get; set; }
        public DateTime TaskCreate { get; set; }
        public DateTime? TaskComplete { get; set; }
#nullable disable
        public string SavePath { get; set; }
        public string ValueSHA1 { get; set; }
#nullable restore
    }
}
