using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.DTOs;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DFApp.Web.Services.Rss;

/// <summary>
/// RSS订阅下载记录管理服务
/// </summary>
public class RssSubscriptionDownloadAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<RssSubscriptionDownload, long> _rssSubscriptionDownloadRepository;
    private readonly ISqlSugarRepository<RssSubscription, long> _rssSubscriptionRepository;
    private readonly ISqlSugarRepository<RssMirrorItem, long> _rssMirrorItemRepository;
    private readonly ISqlSugarRepository<RssSource, long> _rssSourceRepository;
    private readonly ILogger<RssSubscriptionDownloadAppService> _logger;

    // TODO: IRssSubscriptionService 未迁移，RetryAsync 方法中用伪代码替代
    // private readonly IRssSubscriptionService _rssSubscriptionService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="rssSubscriptionDownloadRepository">RSS订阅下载记录仓储</param>
    /// <param name="rssSubscriptionRepository">RSS订阅仓储</param>
    /// <param name="rssMirrorItemRepository">RSS镜像条目仓储</param>
    /// <param name="rssSourceRepository">RSS源仓储</param>
    /// <param name="logger">日志记录器</param>
    public RssSubscriptionDownloadAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<RssSubscriptionDownload, long> rssSubscriptionDownloadRepository,
        ISqlSugarRepository<RssSubscription, long> rssSubscriptionRepository,
        ISqlSugarRepository<RssMirrorItem, long> rssMirrorItemRepository,
        ISqlSugarRepository<RssSource, long> rssSourceRepository,
        ILogger<RssSubscriptionDownloadAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _rssSubscriptionDownloadRepository = rssSubscriptionDownloadRepository;
        _rssSubscriptionRepository = rssSubscriptionRepository;
        _rssMirrorItemRepository = rssMirrorItemRepository;
        _rssSourceRepository = rssSourceRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取下载记录分页列表
    /// </summary>
    /// <param name="input">查询请求DTO</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<RssSubscriptionDownloadDto>> GetListAsync(GetRssSubscriptionDownloadsRequestDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.RssSubscription.Default);

        var queryable = _rssSubscriptionDownloadRepository.GetQueryable();

        // 应用过滤条件
        if (input.SubscriptionId.HasValue)
        {
            queryable = queryable.Where(x => x.SubscriptionId == input.SubscriptionId.Value);
        }

        if (input.RssMirrorItemId.HasValue)
        {
            queryable = queryable.Where(x => x.RssMirrorItemId == input.RssMirrorItemId.Value);
        }

        if (input.DownloadStatus.HasValue)
        {
            queryable = queryable.Where(x => x.DownloadStatus == input.DownloadStatus.Value);
        }

        if (input.StartTime.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime >= input.StartTime.Value);
        }

        if (input.EndTime.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime <= input.EndTime.Value);
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.CreationTime);
        }

        // 分页
        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var dtos = items.Select(MapToDto).ToList();

        // 加载关联数据（替代导航属性查询）
        var subscriptionIds = dtos.Select(d => d.SubscriptionId).Distinct().ToList();
        var mirrorItemIds = dtos.Select(d => d.RssMirrorItemId).Distinct().ToList();

        var subscriptions = await _rssSubscriptionRepository.GetListAsync(s => subscriptionIds.Contains(s.Id));
        var mirrorItems = await _rssMirrorItemRepository.GetListAsync(i => mirrorItemIds.Contains(i.Id));

        var sourceIds = mirrorItems.Select(i => i.RssSourceId).Distinct().ToList();
        var sources = await _rssSourceRepository.GetListAsync(s => sourceIds.Contains(s.Id));

        // 填充关联名称
        foreach (var dto in dtos)
        {
            dto.SubscriptionName = subscriptions.FirstOrDefault(s => s.Id == dto.SubscriptionId)?.Name;

            var item = mirrorItems.FirstOrDefault(i => i.Id == dto.RssMirrorItemId);
            dto.RssMirrorItemTitle = item?.Title;
            dto.RssMirrorItemLink = item?.Link;
            dto.RssSourceName = sources.FirstOrDefault(s => s.Id == item?.RssSourceId)?.Name;

            dto.DownloadStatusText = GetDownloadStatusText(dto.DownloadStatus);
        }

        return new PagedResultDto<RssSubscriptionDownloadDto>(totalCount, dtos);
    }

    /// <summary>
    /// 根据ID获取下载记录
    /// </summary>
    /// <param name="id">下载记录ID</param>
    /// <returns>下载记录DTO</returns>
    public async Task<RssSubscriptionDownloadDto> GetAsync(long id)
    {
        await CheckPermissionAsync(DFAppPermissions.RssSubscription.Default);

        var download = await _rssSubscriptionDownloadRepository.GetByIdAsync(id);
        EnsureEntityExists(download, id);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var dto = MapToDto(download!);

        // 加载关联数据（替代导航属性查询）
        var subscription = await _rssSubscriptionRepository.GetFirstOrDefaultAsync(s => s.Id == download!.SubscriptionId);
        dto.SubscriptionName = subscription?.Name;

        var item = await _rssMirrorItemRepository.GetFirstOrDefaultAsync(i => i.Id == download!.RssMirrorItemId);
        dto.RssMirrorItemTitle = item?.Title;
        dto.RssMirrorItemLink = item?.Link;

        if (item != null)
        {
            var source = await _rssSourceRepository.GetFirstOrDefaultAsync(s => s.Id == item.RssSourceId);
            dto.RssSourceName = source?.Name;
        }

        dto.DownloadStatusText = GetDownloadStatusText(dto.DownloadStatus);

        return dto;
    }

    /// <summary>
    /// 删除下载记录
    /// </summary>
    /// <param name="id">下载记录ID</param>
    public async Task DeleteAsync(long id)
    {
        await CheckPermissionAsync(DFAppPermissions.RssSubscription.Delete);

        await _rssSubscriptionDownloadRepository.DeleteAsync(id);
        _logger.LogInformation("删除订阅下载记录: {Id}", id);
    }

    /// <summary>
    /// 批量删除下载记录
    /// </summary>
    /// <param name="ids">下载记录ID列表</param>
    public async Task DeleteManyAsync(List<long> ids)
    {
        await CheckPermissionAsync(DFAppPermissions.RssSubscription.Delete);

        foreach (var id in ids)
        {
            await _rssSubscriptionDownloadRepository.DeleteAsync(id);
        }

        _logger.LogInformation("批量删除订阅下载记录: {Count} 条", ids.Count);
    }

    /// <summary>
    /// 清空所有下载记录
    /// </summary>
    public async Task ClearAllAsync()
    {
        await CheckPermissionAsync(DFAppPermissions.RssSubscription.Delete);

        await _rssSubscriptionDownloadRepository.DeleteAsync(x => true);
        _logger.LogInformation("清空所有订阅下载记录");
    }

    /// <summary>
    /// 重试下载
    /// </summary>
    /// <param name="id">下载记录ID</param>
    public async Task RetryAsync(long id)
    {
        await CheckPermissionAsync(DFAppPermissions.RssSubscription.Default);

        var download = await _rssSubscriptionDownloadRepository.GetByIdAsync(id);
        EnsureEntityExists(download, id);

        if (download!.DownloadStatus != 3)
        {
            throw new BusinessException("只能重试失败的下载任务");
        }

        // 先删除旧的下载记录，避免与后续创建的记录产生冲突
        await _rssSubscriptionDownloadRepository.DeleteAsync(id);

        // TODO: IRssSubscriptionService 未迁移，以下为伪代码
        // await _rssSubscriptionService.CreateDownloadTaskAsync(download.SubscriptionId, download.RssMirrorItemId);
        _logger.LogWarning("IRssSubscriptionService 未迁移，重试下载功能暂不可用。SubscriptionId: {SubscriptionId}, RssMirrorItemId: {RssMirrorItemId}",
            download.SubscriptionId, download.RssMirrorItemId);

        _logger.LogInformation("重试订阅下载: {Id}", id);
    }

    /// <summary>
    /// 获取下载状态文本
    /// </summary>
    /// <param name="status">状态码</param>
    /// <returns>状态文本</returns>
    private static string GetDownloadStatusText(int status)
    {
        return status switch
        {
            0 => "待下载",
            1 => "下载中",
            2 => "下载完成",
            3 => "下载失败",
            _ => "未知状态"
        };
    }

    /// <summary>
    /// 将 RssSubscriptionDownload 实体映射为 RssSubscriptionDownloadDto
    /// </summary>
    /// <param name="entity">下载记录实体</param>
    /// <returns>下载记录DTO</returns>
    private static RssSubscriptionDownloadDto MapToDto(RssSubscriptionDownload entity)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return new RssSubscriptionDownloadDto
        {
            Id = entity.Id,
            SubscriptionId = entity.SubscriptionId,
            RssMirrorItemId = entity.RssMirrorItemId,
            Aria2Gid = entity.Aria2Gid,
            DownloadStatus = entity.DownloadStatus,
            IsPendingDueToLowDiskSpace = entity.IsPendingDueToLowDiskSpace,
            ErrorMessage = entity.ErrorMessage,
            DownloadStartTime = entity.DownloadStartTime,
            DownloadCompleteTime = entity.DownloadCompleteTime,
            CreationTime = entity.CreationTime
        };
    }
}
