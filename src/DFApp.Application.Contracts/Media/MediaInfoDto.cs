using System;
using Volo.Abp.Application.Dtos;

namespace DFApp.Media
{
    public class MediaInfoDto : AuditedEntityDto<long>
    {
        public string MediaId { get; set; } = null!;
        public long ChatId { get; set; }
        public string ChatTitle { get; set; } = null!;
        public string? Message { get; set; }
        public long Size { get; set; }
        public string SavePath { get; set; } = null!;
        public string MD5 { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public bool IsExternalLinkGenerated { get; set; }
    }
}
