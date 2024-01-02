using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace DFApp.Media
{
    public class MediaInfo : AuditedAggregateRoot<long>, ISoftDelete
    {
        public long AccessHash { get; set; }
        public long TID { get; set; }
        public long Size { get; set; }
        public string? SavePath { get; set; }
        public string? ValueSHA1 { get; set; }
        public string? MimeType { get; set; }
        public string? Title { get; set; }
        public bool IsDeleted { get; set; }
    }
}
