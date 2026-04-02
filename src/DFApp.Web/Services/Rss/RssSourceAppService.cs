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
/// RSS源管理服务
/// </summary>
public class RssSourceAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<RssSource, long> _rssSourceRepository;
    private readonly ILogger<RssSourceAppService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="rssSourceRepository">RSS源仓储</param>
    /// <param name="logger">日志记录器</param>
    public RssSourceAppService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<RssSource, long> rssSourceRepository,
        ILogger<RssSourceAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _rssSourceRepository = rssSourceRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取RSS源分页列表
    /// </summary>
    /// <param name="input">分页排序请求</param>
    /// <returns>分页结果</returns>
    public async Task<PagedResultDto<RssSourceDto>> GetListAsync(Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto input)
    {
        var queryable = _rssSourceRepository.GetQueryable();

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

        // TODO: 使用 Mapperly 映射（RssMapper 返回 DFApp.Web.DTOs.Rss.RssSourceDto，
        // 但此服务使用 DFApp.Rss.RssSourceDto，存在命名空间冲突，暂保留手动映射）
        var dtos = items.Select(MapToDto).ToList();

        return new PagedResultDto<RssSourceDto>(totalCount, dtos);
    }

    /// <summary>
    /// 根据ID获取RSS源
    /// </summary>
    /// <param name="id">RSS源ID</param>
    /// <returns>RSS源DTO</returns>
    public async Task<RssSourceDto> GetAsync(long id)
    {
        var source = await _rssSourceRepository.GetByIdAsync(id);
        EnsureEntityExists(source, id);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return MapToDto(source);
    }

    /// <summary>
    /// 创建RSS源
    /// </summary>
    /// <param name="input">创建/更新RSS源DTO</param>
    /// <returns>RSS源DTO</returns>
    public async Task<RssSourceDto> CreateAsync(CreateUpdateRssSourceDto input)
    {
        // 验证URL是否重复
        var existing = await _rssSourceRepository.GetFirstOrDefaultAsync(x => x.Url == input.Url);
        if (existing != null)
        {
            throw new BusinessException("该RSS源URL已存在");
        }

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        var source = new RssSource
        {
            Name = input.Name,
            Url = input.Url,
            ProxyUrl = input.ProxyUrl,
            ProxyUsername = input.ProxyUsername,
            ProxyPassword = input.ProxyPassword,
            IsEnabled = input.IsEnabled,
            FetchIntervalMinutes = input.FetchIntervalMinutes,
            MaxItems = input.MaxItems,
            Query = input.Query,
            Remark = input.Remark,
            CreationTime = DateTime.Now,
            FetchStatus = 0,
            ExtraProperties = string.Empty
        };

        await _rssSourceRepository.InsertAsync(source);

        _logger.LogInformation("创建RSS源: {Name} ({Url})", input.Name, input.Url);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return MapToDto(source);
    }

    /// <summary>
    /// 更新RSS源
    /// </summary>
    /// <param name="id">RSS源ID</param>
    /// <param name="input">创建/更新RSS源DTO</param>
    /// <returns>RSS源DTO</returns>
    public async Task<RssSourceDto> UpdateAsync(long id, CreateUpdateRssSourceDto input)
    {
        var source = await _rssSourceRepository.GetByIdAsync(id);
        EnsureEntityExists(source, id);

        // 检查URL是否与其他源重复
        var existing = await _rssSourceRepository.GetFirstOrDefaultAsync(x => x.Url == input.Url && x.Id != id);
        if (existing != null)
        {
            throw new BusinessException("该RSS源URL已被其他源使用");
        }

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        source.Name = input.Name;
        source.Url = input.Url;
        source.ProxyUrl = input.ProxyUrl;
        source.ProxyUsername = input.ProxyUsername;
        source.ProxyPassword = input.ProxyPassword;
        source.IsEnabled = input.IsEnabled;
        source.FetchIntervalMinutes = input.FetchIntervalMinutes;
        source.MaxItems = input.MaxItems;
        source.Query = input.Query;
        source.Remark = input.Remark;

        await _rssSourceRepository.UpdateAsync(source);

        _logger.LogInformation("更新RSS源: {Name} ({Url})", input.Name, input.Url);

        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return MapToDto(source);
    }

    /// <summary>
    /// 删除RSS源
    /// </summary>
    /// <param name="id">RSS源ID</param>
    public async Task DeleteAsync(long id)
    {
        await _rssSourceRepository.DeleteAsync(id);
        _logger.LogInformation("删除RSS源: {Id}", id);
    }

    /// <summary>
    /// 手动触发RSS源抓取
    /// </summary>
    /// <param name="id">RSS源ID</param>
    public async Task TriggerFetchAsync(long id)
    {
        var source = await _rssSourceRepository.GetByIdAsync(id);
        EnsureEntityExists(source, id);

        if (!source.IsEnabled)
        {
            throw new BusinessException("该RSS源未启用");
        }

        _logger.LogInformation("手动触发RSS源抓取: {Name} ({Url})", source.Name, source.Url);

        // 更新最后抓取时间
        source.LastFetchTime = DateTime.Now;
        await _rssSourceRepository.UpdateAsync(source);

        // TODO: 集成后台任务调度，触发立即抓取
        throw new BusinessException("手动触发功能将在Background Worker下次执行时生效，或等待自动调度");
    }

    /// <summary>
    /// 将 RssSource 实体映射为 RssSourceDto
    /// </summary>
    /// <param name="entity">RSS源实体</param>
    /// <returns>RSS源DTO</returns>
    private static RssSourceDto MapToDto(RssSource entity)
    {
        // TODO: 使用 Mapperly 映射（命名空间冲突，暂保留手动映射）
        return new RssSourceDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Url = entity.Url,
            ProxyUrl = entity.ProxyUrl,
            ProxyUsername = entity.ProxyUsername,
            IsEnabled = entity.IsEnabled,
            FetchIntervalMinutes = entity.FetchIntervalMinutes,
            MaxItems = entity.MaxItems,
            Query = entity.Query,
            LastFetchTime = entity.LastFetchTime,
            FetchStatus = entity.FetchStatus,
            ErrorMessage = entity.ErrorMessage,
            Remark = entity.Remark,
            CreationTime = entity.CreationTime
        };
    }
}
