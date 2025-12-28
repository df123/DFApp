using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace DFApp.Media
{
    public class MediaInfo : Entity<long>, IHasCreationTime, IHasModificationTime, IHasConcurrencyStamp
    {
        public long MediaId { get; set; }
        public long ChatId { get; set; }
        public required string ChatTitle { get; set; }
        public string? Message { get; set; }
        public long Size { get; set; }
        public required string SavePath { get; set; }
        public required string MimeType { get; set; }
        public bool IsExternalLinkGenerated { get; set; }
        public bool IsDownloadCompleted { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string ConcurrencyStamp { get; set; } = string.Empty;

        // 下载速度相关字段
        public long DownloadTimeMs { get; set; }
        public double DownloadSpeedBps { get; set; }
    }
}
