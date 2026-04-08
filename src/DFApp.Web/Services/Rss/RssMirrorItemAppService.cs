using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.DTOs;
using DFApp.Web.DTOs.Rss;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DFApp.Web.Services.Rss;

/// <summary>
/// RSS镜像条目管理服务
/// </summary>
public class RssMirrorItemAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<RssMirrorItem, long> _rssMirrorItemRepository;
    private readonly ISqlSugarRepository<RssWordSegment, long> _rssWordSegmentRepository;
    private readonly ISqlSugarRepository<RssSource, long> _rssSourceRepository;
    private readonly ILogger<RssMirrorItemAppService> _logger;

    // TODO: IAria2Service 未迁移，暂时使用 object? 替代
    private readonly object? _aria2Service;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="rssMirrorItemRepository">RSS镜像条目仓储</param>
    /// <param name="rssWordSegmentRepository">RSS分词仓储</param>
    /// <param name="rssSourceRepository">RSS源仓储</param>
    /// <param name="logger">日志记录器</param>
    public RssMirrorItemAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<RssMirrorItem, long> rssMirrorItemRepository,
        ISqlSugarRepository<RssWordSegment, long> rssWordSegmentRepository,
        ISqlSugarRepository<RssSource, long> rssSourceRepository,
        ILogger<RssMirrorItemAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _rssMirrorItemRepository = rssMirrorItemRepository;
        _rssWordSegmentRepository = rssWordSegmentRepository;
        _rssSourceRepository = rssSourceRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取镜像条目分页列表
    /// </summary>
    /// <param name="input">查询请求DTO</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<RssMirrorItemDto>> GetListAsync(GetRssMirrorItemsRequestDto input)
    {
        var queryable = _rssMirrorItemRepository.GetQueryable();

        // 应用过滤条件
        if (input.RssSourceId.HasValue)
        {
            queryable = queryable.Where(x => x.RssSourceId == input.RssSourceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Title.Contains(input.Filter) ||
                                              (x.Description != null && x.Description.Contains(input.Filter)));
        }

        if (input.StartTime.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime >= input.StartTime.Value);
        }

        if (input.EndTime.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime <= input.EndTime.Value);
        }

        if (input.IsDownloaded.HasValue)
        {
            queryable = queryable.Where(x => x.IsDownloaded == input.IsDownloaded.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.WordToken))
        {
            // 根据分词过滤：先获取匹配的镜像条目ID列表
            var filterItemIds = await _rssWordSegmentRepository.GetQueryable()
                .Where(x => x.Word.ToLower() == input.WordToken.ToLower())
                .Select(x => x.RssMirrorItemId)
                .Distinct()
                .ToListAsync();

            queryable = queryable.Where(x => filterItemIds.Contains(x.Id));
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
        var itemIds = items.Select(i => i.Id).ToList();
        var wordSegments = await _rssWordSegmentRepository.GetListAsync(x => itemIds.Contains(x.RssMirrorItemId));
        var sources = await _rssSourceRepository.GetListAsync();

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var dtos = items.Select(MapToDto).ToList();

        // 填充分词和RSS源名称
        foreach (var dto in dtos)
        {
            dto.RssSourceName = sources.FirstOrDefault(s => s.Id == dto.RssSourceId)?.Name;
            dto.WordSegments = wordSegments
                .Where(ws => ws.RssMirrorItemId == dto.Id)
                .GroupBy(ws => ws.Word.ToLower())
                .Select(g => new RssWordSegmentDto
                {
                    Word = g.First().Word,
                    LanguageType = g.First().LanguageType,
                    Count = g.Sum(x => x.Count),
                    CreationTime = g.First().CreationTime
                })
                .ToList();
        }

        return new PagedResultDto<RssMirrorItemDto>(totalCount, dtos);
    }

    /// <summary>
    /// 根据ID获取镜像条目
    /// </summary>
    /// <param name="id">镜像条目ID</param>
    /// <returns>镜像条目DTO</returns>
    public async Task<RssMirrorItemDto> GetAsync(long id)
    {
        var item = await _rssMirrorItemRepository.GetByIdAsync(id);
        EnsureEntityExists(item, id);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var dto = MapToDto(item!);

        // 加载分词
        var wordSegments = await _rssWordSegmentRepository.GetListAsync(x => x.RssMirrorItemId == id);
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        dto.WordSegments = wordSegments.Select(MapWordSegmentToDto).ToList();

        // 加载RSS源名称
        var source = await _rssSourceRepository.GetFirstOrDefaultAsync(s => s.Id == dto.RssSourceId);
        dto.RssSourceName = source?.Name;

        return dto;
    }

    /// <summary>
    /// 删除镜像条目
    /// </summary>
    /// <param name="id">镜像条目ID</param>
    public async Task DeleteAsync(long id)
    {
        await CheckPermissionAsync(DFAppPermissions.Rss.Delete);

        // 先删除关联的分词
        await _rssWordSegmentRepository.DeleteAsync(x => x.RssMirrorItemId == id);

        // 再删除镜像条目
        await _rssMirrorItemRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 批量删除镜像条目
    /// </summary>
    /// <param name="ids">镜像条目ID列表</param>
    public async Task DeleteManyAsync(List<long> ids)
    {
        await CheckPermissionAsync(DFAppPermissions.Rss.Delete);

        foreach (var id in ids)
        {
            await DeleteAsync(id);
        }
    }

    /// <summary>
    /// 获取分词统计
    /// </summary>
    /// <param name="rssSourceId">RSS源ID（可选）</param>
    /// <param name="languageType">语言类型（可选）</param>
    /// <param name="top">返回前N条</param>
    /// <returns>分词统计列表</returns>
    public async Task<List<WordSegmentStatisticsDto>> GetWordSegmentStatisticsAsync(
        long? rssSourceId = null,
        int? languageType = null,
        int top = 100)
    {
        var wordSegmentQueryable = _rssWordSegmentRepository.GetQueryable();

        // 应用过滤条件
        if (rssSourceId.HasValue)
        {
            var itemIds = await _rssMirrorItemRepository.GetQueryable()
                .Where(x => x.RssSourceId == rssSourceId.Value)
                .Select(x => x.Id)
                .ToListAsync();

            wordSegmentQueryable = wordSegmentQueryable.Where(x => itemIds.Contains(x.RssMirrorItemId));
        }

        if (languageType.HasValue)
        {
            wordSegmentQueryable = wordSegmentQueryable.Where(x => x.LanguageType == languageType.Value);
        }

        // 在内存中执行分组统计
        var allSegments = await wordSegmentQueryable.ToListAsync();

        var statistics = allSegments
            .GroupBy(x => x.Word.ToLower())
            .Select(g => new WordSegmentStatisticsDto
            {
                Word = g.First().Word,
                TotalCount = g.Sum(x => x.Count),
                ItemCount = g.Select(x => x.RssMirrorItemId).Distinct().Count(),
                LanguageType = g.First().LanguageType
            })
            .OrderByDescending(x => x.TotalCount)
            .Take(top)
            .ToList();

        return statistics;
    }

    /// <summary>
    /// 根据分词获取镜像条目
    /// </summary>
    /// <param name="wordToken">分词文本</param>
    /// <param name="input">分页排序请求</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<RssMirrorItemDto>> GetByWordTokenAsync(
        string wordToken,
        PagedAndSortedResultRequestDto input)
    {
        var request = new GetRssMirrorItemsRequestDto
        {
            WordToken = wordToken,
            SkipCount = input.SkipCount,
            MaxResultCount = input.MaxResultCount,
            Sorting = input.Sorting
        };

        return await GetListAsync(request);
    }

    /// <summary>
    /// 清空所有镜像数据
    /// </summary>
    public async Task ClearAllAsync()
    {
        await CheckPermissionAsync(DFAppPermissions.Rss.Delete);

        // 删除所有分词
        await _rssWordSegmentRepository.DeleteAsync(x => true);

        // 删除所有镜像条目
        await _rssMirrorItemRepository.DeleteAsync(x => true);

        _logger.LogInformation("已清空所有RSS镜像数据");
    }

    /// <summary>
    /// 下载到Aria2
    /// </summary>
    /// <param name="id">镜像条目ID</param>
    /// <param name="videoOnly">仅下载视频</param>
    /// <param name="enableKeywordFilter">启用关键词过滤</param>
    /// <returns>下载任务ID</returns>
    public async Task<string> DownloadToAria2Async(long id, bool videoOnly = false, bool enableKeywordFilter = false)
    {
        await CheckPermissionAsync(DFAppPermissions.Rss.Default);

        var item = await _rssMirrorItemRepository.GetByIdAsync(id);
        EnsureEntityExists(item, id);

        if (item!.IsDownloaded)
        {
            _logger.LogWarning("RSS镜像条目 {Id} 已经下载过", id);
            throw new BusinessException("该条目已经下载过");
        }

        // TODO: IAria2Service 未迁移，以下为伪代码
        // var request = new AddDownloadRequestDto
        // {
        //     Urls = new List<string> { item.Link },
        //     VideoOnly = videoOnly,
        //     EnableKeywordFilter = enableKeywordFilter
        // };
        // var result = await _aria2Service.AddDownloadAsync(request);

        // 更新下载状态
        item.IsDownloaded = true;
        item.DownloadTime = DateTime.Now;
        await _rssMirrorItemRepository.UpdateAsync(item);

        _logger.LogInformation("RSS镜像条目 {Id} 已添加到Aria2下载队列", id);

        // TODO: IAria2Service 未迁移，返回占位值
        throw new BusinessException("IAria2Service 尚未迁移，下载功能暂不可用");
        // return result.Id;
    }

    /// <summary>
    /// 将 RssMirrorItem 实体映射为 RssMirrorItemDto
    /// </summary>
    /// <param name="entity">镜像条目实体</param>
    /// <returns>镜像条目DTO</returns>
    private static RssMirrorItemDto MapToDto(RssMirrorItem entity)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return new RssMirrorItemDto
        {
            Id = entity.Id,
            RssSourceId = entity.RssSourceId,
            Title = entity.Title,
            Link = entity.Link,
            Description = entity.Description,
            Author = entity.Author,
            Category = entity.Category,
            PublishDate = entity.PublishDate,
            Seeders = entity.Seeders,
            Leechers = entity.Leechers,
            Downloads = entity.Downloads,
            IsDownloaded = entity.IsDownloaded,
            DownloadTime = entity.DownloadTime,
            CreationTime = entity.CreationTime
        };
    }

    /// <summary>
    /// 将 RssWordSegment 实体映射为 RssWordSegmentDto
    /// </summary>
    /// <param name="entity">分词实体</param>
    /// <returns>分词DTO</returns>
    private static RssWordSegmentDto MapWordSegmentToDto(RssWordSegment entity)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return new RssWordSegmentDto
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
