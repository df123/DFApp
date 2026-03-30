using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Aria2;
using DFApp.Rss;
using DFApp.Web.Data;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Rss;

/// <summary>
/// RSS订阅服务 - 负责订阅匹配、下载任务创建和暂存下载处理
/// </summary>
public class RssSubscriptionService : IRssSubscriptionService
{
    private readonly ILogger<RssSubscriptionService> _logger;
    private readonly ISqlSugarRepository<RssSubscription, long> _rssSubscriptionRepository;
    private readonly ISqlSugarRepository<RssMirrorItem, long> _rssMirrorItemRepository;
    private readonly ISqlSugarRepository<RssSubscriptionDownload, long> _rssSubscriptionDownloadRepository;

    // TODO: IAria2Service 未迁移，后续替换为实际接口
    private readonly IAria2Service? _aria2Service;

    /// <summary>
    /// 最小磁盘空间（GB）
    /// </summary>
    private const long MinDiskSpaceGB = 2;

    /// <summary>
    /// 最小磁盘空间（字节）
    /// </summary>
    private const long MinDiskSpaceBytes = MinDiskSpaceGB * 1024 * 1024 * 1024;

    public RssSubscriptionService(
        ILogger<RssSubscriptionService> logger,
        ISqlSugarRepository<RssSubscription, long> rssSubscriptionRepository,
        ISqlSugarRepository<RssMirrorItem, long> rssMirrorItemRepository,
        ISqlSugarRepository<RssSubscriptionDownload, long> rssSubscriptionDownloadRepository,
        IAria2Service? aria2Service = null)
    {
        _logger = logger;
        _rssSubscriptionRepository = rssSubscriptionRepository;
        _rssMirrorItemRepository = rssMirrorItemRepository;
        _rssSubscriptionDownloadRepository = rssSubscriptionDownloadRepository;
        _aria2Service = aria2Service;
    }

    /// <summary>
    /// 匹配RSS镜像条目与所有启用的订阅规则
    /// </summary>
    /// <param name="item">RSS镜像条目</param>
    /// <returns>每个订阅的匹配结果列表</returns>
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

            // 检查RSS源是否匹配
            if (subscription.RssSourceId.HasValue && subscription.RssSourceId != item.RssSourceId)
            {
                result.Matched = false;
                result.MatchReason = "RSS源不匹配";
                results.Add(result);
                continue;
            }

            // 检查发布日期是否在订阅日期范围内
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

            // 检查关键词匹配
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

            // 检查质量过滤
            if (!string.IsNullOrEmpty(subscription.QualityFilter) &&
                !item.Title.Contains(subscription.QualityFilter, StringComparison.OrdinalIgnoreCase))
            {
                result.Matched = false;
                result.MatchReason = "质量过滤不匹配";
                results.Add(result);
                continue;
            }

            // 检查字幕组过滤
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

            // 检查做种者数量范围
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

            // 检查下载者数量范围
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

            // 检查完成下载数量范围
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

    /// <summary>
    /// 根据订阅ID和镜像条目ID创建下载任务
    /// </summary>
    /// <param name="subscriptionId">订阅ID</param>
    /// <param name="rssMirrorItemId">RSS镜像条目ID</param>
    public async Task CreateDownloadTaskAsync(long subscriptionId, long rssMirrorItemId)
    {
        var subscription = await _rssSubscriptionRepository.GetByIdAsync(subscriptionId)
            ?? throw new InvalidOperationException($"订阅 {subscriptionId} 不存在");
        var item = await _rssMirrorItemRepository.GetByIdAsync(rssMirrorItemId)
            ?? throw new InvalidOperationException($"RSS镜像条目 {rssMirrorItemId} 不存在");

        // 检查是否已存在下载记录
        var existingDownload = await _rssSubscriptionDownloadRepository.GetFirstOrDefaultAsync(
            d => d.SubscriptionId == subscriptionId && d.RssMirrorItemId == rssMirrorItemId);

        if (existingDownload != null)
        {
            _logger.LogInformation("订阅 {SubscriptionName} 的下载任务已存在: {Title}",
                subscription.Name, item.Title);
            return;
        }

        var availableSpace = GetAvailableDiskSpace();

        // 磁盘空间不足时暂存下载记录
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

        // TODO: IAria2Service 未迁移，以下为伪代码
        // var downloadRequest = new AddDownloadRequestDto
        // {
        //     Urls = new List<string> { item.Link },
        //     VideoOnly = subscription.VideoOnly,
        //     EnableKeywordFilter = subscription.EnableKeywordFilter,
        //     SavePath = subscription.SavePath
        // };
        // var result = await _aria2Service.AddDownloadAsync(downloadRequest);

        var downloadRecord = new RssSubscriptionDownload
        {
            SubscriptionId = subscriptionId,
            RssMirrorItemId = rssMirrorItemId,
            Aria2Gid = string.Empty, // TODO: 替换为 result.Id
            DownloadStatus = 1,
            DownloadStartTime = DateTime.Now,
            CreationTime = DateTime.Now
        };

        await _rssSubscriptionDownloadRepository.InsertAsync(downloadRecord);

        _logger.LogInformation("订阅 {SubscriptionName} 自动下载: {Title}",
            subscription.Name, item.Title);
    }

    /// <summary>
    /// 处理因磁盘空间不足而暂存的下载任务
    /// </summary>
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
                var subscription = await _rssSubscriptionRepository.GetByIdAsync(download.SubscriptionId);
                var item = await _rssMirrorItemRepository.GetByIdAsync(download.RssMirrorItemId);

                if (subscription == null || item == null)
                {
                    _logger.LogWarning("暂存下载 {Id} 关联的订阅或镜像条目不存在，跳过", download.Id);
                    continue;
                }

                // TODO: IAria2Service 未迁移，以下为伪代码
                // var downloadRequest = new AddDownloadRequestDto
                // {
                //     Urls = new List<string> { item.Link },
                //     VideoOnly = subscription.VideoOnly,
                //     EnableKeywordFilter = subscription.EnableKeywordFilter,
                //     SavePath = subscription.SavePath
                // };
                // var result = await _aria2Service.AddDownloadAsync(downloadRequest);

                download.Aria2Gid = string.Empty; // TODO: 替换为 result.Id
                download.DownloadStatus = 1;
                download.DownloadStartTime = DateTime.Now;
                download.IsPendingDueToLowDiskSpace = false;

                await _rssSubscriptionDownloadRepository.UpdateAsync(download);

                _logger.LogInformation("已处理暂存下载: {Id}", download.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理暂存下载失败: {Id}", download.Id);
            }
        }

        _logger.LogInformation("已处理 {Count} 个暂存下载任务", pendingDownloads.Count);
    }

    /// <summary>
    /// 获取当前磁盘可用空间
    /// </summary>
    /// <returns>可用空间字节数</returns>
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
}
