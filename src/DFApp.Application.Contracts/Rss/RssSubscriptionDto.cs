using System;
using Volo.Abp.Application.Dtos;

namespace DFApp.Rss
{
    public class RssSubscriptionDto : EntityDto<long>
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

        public string? RssSourceName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Remark { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateUpdateRssSubscriptionDto
    {
        public string Name { get; set; } = string.Empty;

        public string Keywords { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public int? MinSeeders { get; set; }

        public int? MaxSeeders { get; set; }

        public int? MinLeechers { get; set; }

        public int? MaxLeechers { get; set; }

        public int? MinDownloads { get; set; }

        public int? MaxDownloads { get; set; }

        public string? QualityFilter { get; set; }

        public string? SubtitleGroupFilter { get; set; }

        public bool AutoDownload { get; set; } = true;

        public bool VideoOnly { get; set; }

        public bool EnableKeywordFilter { get; set; }

        public string? SavePath { get; set; }

        public long? RssSourceId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Remark { get; set; }
    }

    public class RssSubscriptionDownloadDto : EntityDto<long>
    {
        public long SubscriptionId { get; set; }

        public string? SubscriptionName { get; set; }

        public long RssMirrorItemId { get; set; }

        public string? RssMirrorItemTitle { get; set; }

        public string? RssMirrorItemLink { get; set; }

        public string? RssSourceName { get; set; }

        public string Aria2Gid { get; set; } = string.Empty;

        public int DownloadStatus { get; set; }

        public string? DownloadStatusText { get; set; }

        public bool IsPendingDueToLowDiskSpace { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime? DownloadStartTime { get; set; }

        public DateTime? DownloadCompleteTime { get; set; }

        public DateTime CreationTime { get; set; }
    }

    public class GetRssSubscriptionsRequestDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }

        public bool? IsEnabled { get; set; }

        public long? RssSourceId { get; set; }
    }

    public class GetRssSubscriptionDownloadsRequestDto : PagedAndSortedResultRequestDto
    {
        public long? SubscriptionId { get; set; }

        public long? RssMirrorItemId { get; set; }

        public int? DownloadStatus { get; set; }

        public string? Filter { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
