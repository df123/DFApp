using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace DFApp.Media
{
    public class MediaInfo : AuditedAggregateRoot<long>
    {
        public long ChatId { get; set; }
        public required string ChatTitle { get; set; }
        public string? Message { get; set; }
        public long Size { get; set; }
        public required string SavePath { get; set; }
        public required string MD5 { get; set; }
        public required string MimeType { get; set; }
        public bool IsExternalLinkGenerated { get; set; }
        public bool IsFileDeleted { get; set; }
    }
}
