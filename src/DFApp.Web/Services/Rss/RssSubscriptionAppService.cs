using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.DTOs;
using DFApp.Web.DTOs.Rss;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DFApp.Web.Services.Rss;

/// <summary>
/// RSS订阅管理服务
/// </summary>
public class RssSubscriptionAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<RssSubscription, long> _rssSubscriptionRepository;
    private readonly ISqlSugarRepository<RssSource, long> _rssSourceRepository;
    private readonly ILogger<RssSubscriptionAppService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="rssSubscriptionRepository">RSS订阅仓储</param>
    /// <param name="rssSourceRepository">RSS源仓储</param>
    /// <param name="logger">日志记录器</param>
    public RssSubscriptionAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<RssSubscription, long> rssSubscriptionRepository,
        ISqlSugarRepository<RssSource, long> rssSourceRepository,
        ILogger<RssSubscriptionAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _rssSubscriptionRepository = rssSubscriptionRepository;
        _rssSourceRepository = rssSourceRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取RSS订阅分页列表
    /// </summary>
    /// <param name="input">查询请求DTO</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<RssSubscriptionDto>> GetListAsync(GetRssSubscriptionsRequestDto input)
    {
        var queryable = _rssSubscriptionRepository.GetQueryable();

        if (input.IsEnabled.HasValue)
        {
            queryable = queryable.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        if (input.RssSourceId.HasValue)
        {
            queryable = queryable.Where(x => x.RssSourceId == input.RssSourceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Name.Contains(input.Filter) ||
                                             x.Keywords.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.CreationTime);
        }

        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var dtos = items.Select(MapToDto).ToList();

        // 填充RSS源名称
        var sources = await _rssSourceRepository.GetListAsync();
        foreach (var dto in dtos)
        {
            dto.RssSourceName = sources.FirstOrDefault(s => s.Id == dto.RssSourceId)?.Name;
        }

        return new PagedResultDto<RssSubscriptionDto>(totalCount, dtos);
    }

    /// <summary>
    /// 根据ID获取RSS订阅
    /// </summary>
    /// <param name="id">订阅ID</param>
    /// <returns>订阅DTO</returns>
    public async Task<RssSubscriptionDto> GetAsync(long id)
    {
        var subscription = await _rssSubscriptionRepository.GetByIdAsync(id);
        EnsureEntityExists(subscription, id);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var dto = MapToDto(subscription);

        // 填充RSS源名称
        if (subscription.RssSourceId.HasValue)
        {
            var source = await _rssSourceRepository.GetFirstOrDefaultAsync(s => s.Id == subscription.RssSourceId.Value);
            dto.RssSourceName = source?.Name;
        }

        return dto;
    }

    /// <summary>
    /// 创建RSS订阅
    /// </summary>
    /// <param name="input">创建/更新订阅DTO</param>
    /// <returns>订阅DTO</returns>
    public async Task<RssSubscriptionDto> CreateAsync(CreateUpdateRssSubscriptionDto input)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var subscription = new RssSubscription
        {
            Name = input.Name,
            Keywords = input.Keywords,
            IsEnabled = input.IsEnabled,
            MinSeeders = input.MinSeeders,
            MaxSeeders = input.MaxSeeders,
            MinLeechers = input.MinLeechers,
            MaxLeechers = input.MaxLeechers,
            MinDownloads = input.MinDownloads,
            MaxDownloads = input.MaxDownloads,
            QualityFilter = input.QualityFilter,
            SubtitleGroupFilter = input.SubtitleGroupFilter,
            AutoDownload = input.AutoDownload,
            VideoOnly = input.VideoOnly,
            EnableKeywordFilter = input.EnableKeywordFilter,
            SavePath = input.SavePath,
            RssSourceId = input.RssSourceId,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            Remark = input.Remark,
            CreationTime = DateTime.Now
        };

        await _rssSubscriptionRepository.InsertAsync(subscription);

        _logger.LogInformation("创建RSS订阅: {Name}", input.Name);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return MapToDto(subscription);
    }

    /// <summary>
    /// 更新RSS订阅
    /// </summary>
    /// <param name="id">订阅ID</param>
    /// <param name="input">创建/更新订阅DTO</param>
    /// <returns>订阅DTO</returns>
    public async Task<RssSubscriptionDto> UpdateAsync(long id, CreateUpdateRssSubscriptionDto input)
    {
        _logger.LogInformation("开始更新RSS订阅，ID: {Id}", id);

        var subscription = await _rssSubscriptionRepository.GetByIdAsync(id);
        EnsureEntityExists(subscription, id);

        _logger.LogInformation("获取到的实体，最后修改时间: {Time}",
            subscription.LastModificationTime);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        MapToEntity(input, subscription);
        subscription.LastModificationTime = DateTime.Now;

        try
        {
            await _rssSubscriptionRepository.UpdateAsync(subscription);
        }
        catch (SqlSugarException ex)
        {
            _logger.LogWarning("更新RSS订阅时发生并发冲突，重新获取实体后重试: {Name}, 异常: {Message}",
                input.Name, ex.Message);

            // 等待一小段时间，确保获取到最新数据
            await Task.Delay(100);

            subscription = await _rssSubscriptionRepository.GetByIdAsync(id);
            EnsureEntityExists(subscription, id);
            _logger.LogInformation("重试获取到的实体，最后修改时间: {Time}",
                subscription.LastModificationTime);

            MapToEntity(input, subscription);
            subscription.LastModificationTime = DateTime.Now;

            await _rssSubscriptionRepository.UpdateAsync(subscription);
        }

        _logger.LogInformation("更新RSS订阅成功: {Name}", input.Name);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return MapToDto(subscription);
    }

    /// <summary>
    /// 删除RSS订阅
    /// </summary>
    /// <param name="id">订阅ID</param>
    public async Task DeleteAsync(long id)
    {
        await _rssSubscriptionRepository.DeleteAsync(id);
        _logger.LogInformation("删除RSS订阅: {Id}", id);
    }

    /// <summary>
    /// 切换订阅启用状态
    /// </summary>
    /// <param name="id">订阅ID</param>
    public async Task ToggleEnableAsync(long id)
    {
        var subscription = await _rssSubscriptionRepository.GetByIdAsync(id);
        EnsureEntityExists(subscription, id);

        subscription.IsEnabled = !subscription.IsEnabled;
        subscription.LastModificationTime = DateTime.Now;

        try
        {
            await _rssSubscriptionRepository.UpdateAsync(subscription);
        }
        catch (SqlSugarException)
        {
            _logger.LogWarning("切换订阅状态时发生并发冲突，重新获取实体后重试: {Name}", subscription.Name);

            // 等待一小段时间，确保获取到最新数据
            await Task.Delay(100);

            subscription = await _rssSubscriptionRepository.GetByIdAsync(id);
            EnsureEntityExists(subscription, id);

            subscription.IsEnabled = !subscription.IsEnabled;
            subscription.LastModificationTime = DateTime.Now;

            await _rssSubscriptionRepository.UpdateAsync(subscription);
        }

        _logger.LogInformation("{Action} RSS订阅: {Name}",
            subscription.IsEnabled ? "启用" : "禁用", subscription.Name);
    }

    /// <summary>
    /// 将 RssSubscription 实体映射为 RssSubscriptionDto
    /// </summary>
    /// <param name="entity">订阅实体</param>
    /// <returns>订阅DTO</returns>
    private static RssSubscriptionDto MapToDto(RssSubscription entity)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return new RssSubscriptionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Keywords = entity.Keywords,
            IsEnabled = entity.IsEnabled,
            MinSeeders = entity.MinSeeders,
            MaxSeeders = entity.MaxSeeders,
            MinLeechers = entity.MinLeechers,
            MaxLeechers = entity.MaxLeechers,
            MinDownloads = entity.MinDownloads,
            MaxDownloads = entity.MaxDownloads,
            QualityFilter = entity.QualityFilter,
            SubtitleGroupFilter = entity.SubtitleGroupFilter,
            AutoDownload = entity.AutoDownload,
            VideoOnly = entity.VideoOnly,
            EnableKeywordFilter = entity.EnableKeywordFilter,
            SavePath = entity.SavePath,
            RssSourceId = entity.RssSourceId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Remark = entity.Remark,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }

    /// <summary>
    /// 将 CreateUpdateRssSubscriptionDto 映射到现有 RssSubscription 实体
    /// </summary>
    /// <param name="input">输入DTO</param>
    /// <param name="entity">目标实体</param>
    private static void MapToEntity(CreateUpdateRssSubscriptionDto input, RssSubscription entity)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        entity.Name = input.Name;
        entity.Keywords = input.Keywords;
        entity.IsEnabled = input.IsEnabled;
        entity.MinSeeders = input.MinSeeders;
        entity.MaxSeeders = input.MaxSeeders;
        entity.MinLeechers = input.MinLeechers;
        entity.MaxLeechers = input.MaxLeechers;
        entity.MinDownloads = input.MinDownloads;
        entity.MaxDownloads = input.MaxDownloads;
        entity.QualityFilter = input.QualityFilter;
        entity.SubtitleGroupFilter = input.SubtitleGroupFilter;
        entity.AutoDownload = input.AutoDownload;
        entity.VideoOnly = input.VideoOnly;
        entity.EnableKeywordFilter = input.EnableKeywordFilter;
        entity.SavePath = input.SavePath;
        entity.RssSourceId = input.RssSourceId;
        entity.StartDate = input.StartDate;
        entity.EndDate = input.EndDate;
        entity.Remark = input.Remark;
    }
}
