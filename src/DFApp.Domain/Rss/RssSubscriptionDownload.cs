using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Rss
{
    public class RssSubscriptionDownload : Entity<long>, IHasCreationTime
    {
        public long SubscriptionId { get; set; }

        public long RssMirrorItemId { get; set; }

        public string Aria2Gid { get; set; } = string.Empty;

        public int DownloadStatus { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime? DownloadStartTime { get; set; }

        public DateTime? DownloadCompleteTime { get; set; }

        public bool IsPendingDueToLowDiskSpace { get; set; }

        public DateTime CreationTime { get; set; }

        public Guid? CreatorId { get; set; }
    }
}
