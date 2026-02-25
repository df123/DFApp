using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Rss
{
    public class RssSubscription : Entity<long>, IHasCreationTime, IHasModificationTime, IHasConcurrencyStamp
    {
        public string Name { get; set; } = string.Empty;

        public string Keywords { get; set; } = string.Empty;

        public bool IsEnabled { get; set; }

        public int? MinSeeders { get; set; }

        public int? MaxSeeders { get; set; }

        public int? MinLeechers { get; set; }

        public int? MaxLeechers { get; set; }

        public int? MinDownloads { get; set; }

        public int? MaxDownloads { get; set; }

        public string? QualityFilter { get; set; }

        public string? SubtitleGroupFilter { get; set; }

        public bool AutoDownload { get; set; }

        public bool VideoOnly { get; set; }

        public bool EnableKeywordFilter { get; set; }

        public string? SavePath { get; set; }

        public long? RssSourceId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Remark { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public string ConcurrencyStamp { get; set; } = string.Empty;

        public Guid? CreatorId { get; set; }
    }
}
