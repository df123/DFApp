using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Permissions;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.Services.ElectricVehicle;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DFApp.Web.Services.Rss;

/// <summary>
/// RSS分词应用服务
/// </summary>
public class RssWordSegmentAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<RssWordSegment, long> _rssWordSegmentRepository;
    private readonly ISqlSugarRepository<RssMirrorItem, long> _rssMirrorItemRepository;
    private readonly ISqlSugarRepository<RssSource, long> _rssSourceRepository;
    private readonly ILogger<RssWordSegmentAppService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="rssWordSegmentRepository">RSS分词仓储</param>
    /// <param name="rssMirrorItemRepository">RSS镜像条目仓储</param>
    /// <param name="rssSourceRepository">RSS源仓储</param>
    /// <param name="logger">日志记录器</param>
    public RssWordSegmentAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<RssWordSegment, long> rssWordSegmentRepository,
        ISqlSugarRepository<RssMirrorItem, long> rssMirrorItemRepository,
        ISqlSugarRepository<RssSource, long> rssSourceRepository,
        ILogger<RssWordSegmentAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _rssWordSegmentRepository = rssWordSegmentRepository;
        _rssMirrorItemRepository = rssMirrorItemRepository;
        _rssSourceRepository = rssSourceRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取分词列表（分页）
    /// </summary>
    /// <param name="input">查询请求DTO</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<RssWordSegmentWithItemDto>> GetListAsync(GetRssWordSegmentsRequestDto input)
    {
        var queryable = _rssWordSegmentRepository.GetQueryable();

        // 应用过滤条件
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Word.Contains(input.Filter));
        }

        if (input.RssSourceId.HasValue)
        {
            // 先获取匹配源的镜像条目ID列表
            var filterItemIds = await _rssMirrorItemRepository.GetQueryable()
                .Where(x => x.RssSourceId == input.RssSourceId.Value)
                .Select(x => x.Id)
                .ToListAsync();

            queryable = queryable.Where(x => filterItemIds.Contains(x.RssMirrorItemId));
        }

        if (input.LanguageType.HasValue)
        {
            queryable = queryable.Where(x => x.LanguageType == input.LanguageType.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Word))
        {
            queryable = queryable.Where(x => x.Word.ToLower() == input.Word.ToLower());
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

        // 加载关联数据
        var itemIds = items.Select(i => i.RssMirrorItemId).Distinct().ToList();
        var mirrorItems = await _rssMirrorItemRepository.GetListAsync(x => itemIds.Contains(x.Id));
        var sources = await _rssSourceRepository.GetListAsync();

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var dtos = items.Select(MapToDto).ToList();

        // 填充关联信息
        foreach (var dto in dtos)
        {
            var mirrorItem = mirrorItems.FirstOrDefault(mi => mi.Id == dto.RssMirrorItemId);
            if (mirrorItem != null)
            {
                dto.RssMirrorItemTitle = mirrorItem.Title;
                dto.RssMirrorItemLink = mirrorItem.Link;
                dto.RssSourceId = mirrorItem.RssSourceId;
                dto.RssSourceName = sources.FirstOrDefault(s => s.Id == mirrorItem.RssSourceId)?.Name;
            }
        }

        return new PagedResultDto<RssWordSegmentWithItemDto>(totalCount, dtos);
    }

    /// <summary>
    /// 获取分词统计（带分页）
    /// </summary>
    /// <param name="input">查询请求DTO</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<WordSegmentStatisticsDto>> GetStatisticsAsync(
        GetRssWordSegmentsRequestDto input)
    {
        var wordSegmentQueryable = _rssWordSegmentRepository.GetQueryable();

        // 应用过滤条件
        if (input.RssSourceId.HasValue)
        {
            var filterItemIds = await _rssMirrorItemRepository.GetQueryable()
                .Where(x => x.RssSourceId == input.RssSourceId.Value)
                .Select(x => x.Id)
                .ToListAsync();

            wordSegmentQueryable = wordSegmentQueryable.Where(x => filterItemIds.Contains(x.RssMirrorItemId));
        }

        if (input.LanguageType.HasValue)
        {
            wordSegmentQueryable = wordSegmentQueryable.Where(x => x.LanguageType == input.LanguageType.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            wordSegmentQueryable = wordSegmentQueryable.Where(x => x.Word.Contains(input.Filter));
        }

        // 在内存中执行分组统计
        var allSegments = await wordSegmentQueryable.ToListAsync();

        var statisticsQuery = allSegments
            .GroupBy(x => x.Word.ToLower())
            .Select(g => new WordSegmentStatisticsDto
            {
                Word = g.First().Word,
                TotalCount = g.Sum(x => x.Count),
                ItemCount = g.Select(x => x.RssMirrorItemId).Distinct().Count(),
                LanguageType = g.First().LanguageType
            })
            .AsQueryable();

        // 排序
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            statisticsQuery = statisticsQuery.OrderBy(input.Sorting);
        }
        else
        {
            statisticsQuery = statisticsQuery.OrderByDescending(x => x.TotalCount);
        }

        // 获取总数
        var totalCount = statisticsQuery.Count();

        // 分页
        var items = statisticsQuery
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        _logger.LogInformation("获取分词统计，来源：{RssSourceId}，语言：{LanguageType}，总数：{TotalCount}",
            input.RssSourceId, input.LanguageType, totalCount);

        return new PagedResultDto<WordSegmentStatisticsDto>(totalCount, items);
    }

    /// <summary>
    /// 删除指定RSS镜像条目的所有分词
    /// </summary>
    /// <param name="rssMirrorItemId">RSS镜像条目ID</param>
    public async Task DeleteByItemAsync(long rssMirrorItemId)
    {
        await CheckPermissionAsync(DFAppPermissions.Rss.Delete);

        await _rssWordSegmentRepository.DeleteAsync(x => x.RssMirrorItemId == rssMirrorItemId);
        _logger.LogInformation("已删除RSS镜像条目 {ItemId} 的所有分词", rssMirrorItemId);
    }

    /// <summary>
    /// 删除指定RSS源的所有分词
    /// </summary>
    /// <param name="rssSourceId">RSS源ID</param>
    public async Task DeleteBySourceAsync(long rssSourceId)
    {
        await CheckPermissionAsync(DFAppPermissions.Rss.Delete);

        var itemIds = await _rssMirrorItemRepository.GetQueryable()
            .Where(x => x.RssSourceId == rssSourceId)
            .Select(x => x.Id)
            .ToListAsync();

        await _rssWordSegmentRepository.DeleteAsync(x => itemIds.Contains(x.RssMirrorItemId));
        _logger.LogInformation("已删除RSS源 {SourceId} 的所有分词", rssSourceId);
    }

    /// <summary>
    /// 将 RssWordSegment 实体映射为 RssWordSegmentWithItemDto
    /// </summary>
    /// <param name="entity">分词实体</param>
    /// <returns>带镜像条目信息的分词DTO</returns>
    private static RssWordSegmentWithItemDto MapToDto(RssWordSegment entity)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return new RssWordSegmentWithItemDto
        {
            Id = entity.Id,
            RssMirrorItemId = entity.RssMirrorItemId,
            Word = entity.Word,
            LanguageType = entity.LanguageType,
            Count = entity.Count,
            PartOfSpeech = entity.PartOfSpeech,
            CreationTime = entity.CreationTime
        };
    }
}
