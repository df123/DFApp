using System;
using Volo.Abp.Application.Dtos;

namespace DFApp.Media
{
    public class MediaInfoDto: AuditedEntityDto<long>
    {
        public long AccessHash { get; set; }
        public long TID { get; set; }
        public long Size { get; set; }
        public string? SavePath { get; set; }
        public string? ValueSHA1 { get; set; }
        public string? MimeType { get; set; }
        public string? Title { get; set; }
        public bool IsExternalLinkGenerated { get; set; }
        public bool IsFileDeleted { get; set; }
    }
}
