using DFApp.Aria2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Rss
{
    public class RssSubscriptionService : IRssSubscriptionService, ITransientDependency
    {
        private readonly ILogger<RssSubscriptionService> _logger;
        private readonly IRepository<RssSubscription, long> _rssSubscriptionRepository;
        private readonly IRepository<RssMirrorItem, long> _rssMirrorItemRepository;
        private readonly IRepository<RssSource, long> _rssSourceRepository;
        private readonly IRepository<RssSubscriptionDownload, long> _rssSubscriptionDownloadRepository;
        private readonly IAria2Service _aria2Service;

        public RssSubscriptionService(
            ILogger<RssSubscriptionService> logger,
            IRepository<RssSubscription, long> rssSubscriptionRepository,
            IRepository<RssMirrorItem, long> rssMirrorItemRepository,
            IRepository<RssSource, long> rssSourceRepository,
            IRepository<RssSubscriptionDownload, long> rssSubscriptionDownloadRepository,
            IAria2Service aria2Service)
        {
            _logger = logger;
            _rssSubscriptionRepository = rssSubscriptionRepository;
            _rssMirrorItemRepository = rssMirrorItemRepository;
            _rssSourceRepository = rssSourceRepository;
            _rssSubscriptionDownloadRepository = rssSubscriptionDownloadRepository;
            _aria2Service = aria2Service;
        }

        public async Task<List<RssSubscriptionMatchResult>> MatchSubscriptionsAsync(RssMirrorItem item)
        {
            var results = new List<RssSubscriptionMatchResult>();

            var enabledSubscriptions = await _rssSubscriptionRepository.GetListAsync(s => s.IsEnabled);

            foreach (var subscription in enabledSubscriptions)
            {
                var result = new RssSubscriptionMatchResult
                {
                    SubscriptionId = subscription.Id,
                    SubscriptionName = subscription.Name
                };

                if (subscription.RssSourceId.HasValue && subscription.RssSourceId != item.RssSourceId)
                {
                    result.Matched = false;
                    result.MatchReason = "RSS源不匹配";
                    results.Add(result);
                    continue;
                }

                if (subscription.StartDate.HasValue && item.PublishDate < subscription.StartDate)
                {
                    result.Matched = false;
                    result.MatchReason = "早于开始日期";
                    results.Add(result);
                    continue;
                }

                if (subscription.EndDate.HasValue && item.PublishDate > subscription.EndDate)
                {
                    result.Matched = false;
                    result.MatchReason = "晚于结束日期";
                    results.Add(result);
                    continue;
                }

                var keywords = subscription.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
                bool keywordMatched = keywords.Any(k => 
                    item.Title.Contains(k.Trim(), StringComparison.OrdinalIgnoreCase));

                if (!keywordMatched)
                {
                    result.Matched = false;
                    result.MatchReason = "关键词不匹配";
                    results.Add(result);
                    continue;
                }

                if (!string.IsNullOrEmpty(subscription.QualityFilter) && 
                    !item.Title.Contains(subscription.QualityFilter, StringComparison.OrdinalIgnoreCase))
                {
                    result.Matched = false;
                    result.MatchReason = "质量过滤不匹配";
                    results.Add(result);
                    continue;
                }

                if (!string.IsNullOrEmpty(subscription.SubtitleGroupFilter))
                {
                    var groups = subscription.SubtitleGroupFilter.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    bool groupMatched = groups.Any(g => 
                        item.Title.Contains(g.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (!groupMatched)
                    {
                        result.Matched = false;
                        result.MatchReason = "字幕组不匹配";
                        results.Add(result);
                        continue;
                    }
                }

                if (subscription.MinSeeders.HasValue && (!item.Seeders.HasValue || item.Seeders < subscription.MinSeeders))
                {
                    result.Matched = false;
                    result.MatchReason = "做种者数量不足";
                    results.Add(result);
                    continue;
                }

                if (subscription.MaxSeeders.HasValue && (!item.Seeders.HasValue || item.Seeders > subscription.MaxSeeders))
                {
                    result.Matched = false;
                    result.MatchReason = "做种者数量过多";
                    results.Add(result);
                    continue;
                }

                if (subscription.MinLeechers.HasValue && (!item.Leechers.HasValue || item.Leechers < subscription.MinLeechers))
                {
                    result.Matched = false;
                    result.MatchReason = "下载者数量不足";
                    results.Add(result);
                    continue;
                }

                if (subscription.MaxLeechers.HasValue && (!item.Leechers.HasValue || item.Leechers > subscription.MaxLeechers))
                {
                    result.Matched = false;
                    result.MatchReason = "下载者数量过多";
                    results.Add(result);
                    continue;
                }

                if (subscription.MinDownloads.HasValue && (!item.Downloads.HasValue || item.Downloads < subscription.MinDownloads))
                {
                    result.Matched = false;
                    result.MatchReason = "完成下载数量不足";
                    results.Add(result);
                    continue;
                }

                if (subscription.MaxDownloads.HasValue && (!item.Downloads.HasValue || item.Downloads > subscription.MaxDownloads))
                {
                    result.Matched = false;
                    result.MatchReason = "完成下载数量过多";
                    results.Add(result);
                    continue;
                }

                result.Matched = true;
                result.MatchReason = "匹配成功";
                results.Add(result);
            }

            return results;
        }

        private const long MinDiskSpaceGB = 2;
        private const long MinDiskSpaceBytes = MinDiskSpaceGB * 1024 * 1024 * 1024;

        public async Task CreateDownloadTaskAsync(long subscriptionId, long rssMirrorItemId)
        {
            var subscription = await _rssSubscriptionRepository.GetAsync(subscriptionId);
            var item = await _rssMirrorItemRepository.GetAsync(rssMirrorItemId);

            var existingDownload = await _rssSubscriptionDownloadRepository.FirstOrDefaultAsync(
                d => d.SubscriptionId == subscriptionId && d.RssMirrorItemId == rssMirrorItemId);

            if (existingDownload != null)
            {
                _logger.LogInformation("订阅 {SubscriptionName} 的下载任务已存在: {Title}", 
                    subscription.Name, item.Title);
                return;
            }

            var availableSpace = GetAvailableDiskSpace();

            if (availableSpace < MinDiskSpaceBytes)
            {
                _logger.LogWarning("磁盘空间不足 {MinGB} GB，暂存订阅 {SubscriptionName} 的下载: {Title}", 
                    MinDiskSpaceGB, subscription.Name, item.Title);

                var pendingRecord = new RssSubscriptionDownload
                {
                    SubscriptionId = subscriptionId,
                    RssMirrorItemId = rssMirrorItemId,
                    Aria2Gid = string.Empty,
                    DownloadStatus = 0,
                    IsPendingDueToLowDiskSpace = true,
                    CreationTime = DateTime.Now
                };

                await _rssSubscriptionDownloadRepository.InsertAsync(pendingRecord);
                return;
            }

            var downloadRequest = new AddDownloadRequestDto
            {
                Urls = new List<string> { item.Link },
                VideoOnly = subscription.VideoOnly,
                EnableKeywordFilter = subscription.EnableKeywordFilter,
                SavePath = subscription.SavePath
            };

            var result = await _aria2Service.AddDownloadAsync(downloadRequest);

            var downloadRecord = new RssSubscriptionDownload
            {
                SubscriptionId = subscriptionId,
                RssMirrorItemId = rssMirrorItemId,
                Aria2Gid = result.Id,
                DownloadStatus = 1,
                DownloadStartTime = DateTime.Now,
                CreationTime = DateTime.Now
            };

            await _rssSubscriptionDownloadRepository.InsertAsync(downloadRecord);

            _logger.LogInformation("订阅 {SubscriptionName} 自动下载: {Title} (GID: {Gid})", 
                subscription.Name, item.Title, result.Id);
        }

        private long GetAvailableDiskSpace()
        {
            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var driveInfo = new DriveInfo(Path.GetPathRoot(currentDirectory)!);
                return driveInfo.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取磁盘空间失败");
                return 0;
            }
        }

        public async Task ProcessPendingDownloadsAsync()
        {
            var availableSpace = GetAvailableDiskSpace();

            if (availableSpace < MinDiskSpaceBytes)
            {
                _logger.LogInformation("磁盘空间不足 {MinGB} GB，跳过暂存下载处理", MinDiskSpaceGB);
                return;
            }

            var pendingDownloads = await _rssSubscriptionDownloadRepository.GetListAsync(
                d => d.IsPendingDueToLowDiskSpace && d.DownloadStatus == 0);

            if (!pendingDownloads.Any())
            {
                return;
            }

            _logger.LogInformation("找到 {Count} 个暂存的下载任务", pendingDownloads.Count);

            foreach (var download in pendingDownloads)
            {
                try
                {
                    var subscription = await _rssSubscriptionRepository.GetAsync(download.SubscriptionId);
                    var item = await _rssMirrorItemRepository.GetAsync(download.RssMirrorItemId);

                    var downloadRequest = new AddDownloadRequestDto
                    {
                        Urls = new List<string> { item.Link },
                        VideoOnly = subscription.VideoOnly,
                        EnableKeywordFilter = subscription.EnableKeywordFilter,
                        SavePath = subscription.SavePath
                    };

                    var result = await _aria2Service.AddDownloadAsync(downloadRequest);

                    download.Aria2Gid = result.Id;
                    download.DownloadStatus = 1;
                    download.DownloadStartTime = DateTime.Now;
                    download.IsPendingDueToLowDiskSpace = false;

                    await _rssSubscriptionDownloadRepository.UpdateAsync(download);

                    _logger.LogInformation("已处理暂存下载: {Id} (GID: {Gid})", download.Id, result.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理暂存下载失败: {Id}", download.Id);
                }
            }

            _logger.LogInformation("已处理 {Count} 个暂存下载任务", pendingDownloads.Count);
        }
    }
}
