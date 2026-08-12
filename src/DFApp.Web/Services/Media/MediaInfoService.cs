using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DFApp.Media;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using SqlSugar;
using CreateUpdateMediaInfoDto = DFApp.Web.DTOs.Media.CreateUpdateMediaInfoDto;
using ChartDataDto = DFApp.Web.DTOs.Media.ChartDataDto;
using MediaInfoDto = DFApp.Web.DTOs.Media.MediaInfoDto;
using MediaDownloadNotificationDto = DFApp.Web.DTOs.Media.MediaDownloadNotificationDto;

namespace DFApp.Web.Services.Media;

/// <summary>
/// 媒体信息服务
/// </summary>
public class MediaInfoService : CrudServiceBase<MediaInfo, long, MediaInfoDto, CreateUpdateMediaInfoDto, CreateUpdateMediaInfoDto>
{
    private readonly MediaMapper _mapper = new();
    private readonly IConfigurationInfoRepository _configRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    /// <param name="configRepository">配置仓储（读取下载 URL 前缀）</param>
    public MediaInfoService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<MediaInfo, long> repository,
        IConfigurationInfoRepository configRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _configRepository = configRepository;
    }

    /// <summary>
    /// 根据过滤条件分页查询
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public async Task<(List<MediaInfoDto> Items, int TotalCount)> GetFilteredPagedListAsync(string? filter, int pageIndex, int pageSize)
    {
        if (!string.IsNullOrWhiteSpace(filter))
        {
            Expression<Func<MediaInfo, bool>> filterExpression = x =>
                x.MediaId.ToString().Contains(filter)
                || x.ChatTitle.Contains(filter)
                || x.Message!.Contains(filter)
                || x.MimeType.Contains(filter);

            return await GetPagedListAsync(filterExpression, pageIndex, pageSize);
        }
        else
        {
            return await GetPagedListAsync(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// 获取已下载完成且尚未取回本地的媒体列表（供下载器补漏同步），返回下载通知格式的数据。
    /// 仅返回 IsDownloadCompleted=true && IsExternalLinkGenerated=false 的记录；
    /// sinceId 用于增量：只返回 Id 大于 sinceId 的记录，避免下载器每次全量拉取。
    /// </summary>
    public async Task<(List<MediaDownloadNotificationDto> Items, int TotalCount)> GetDownloadCompletedAsync(long sinceId, int pageIndex, int pageSize)
    {
        var (entities, totalCount) = await Repository.GetPagedListAsync(
            x => x.IsDownloadCompleted && !x.IsExternalLinkGenerated && x.Id > sinceId, pageIndex, pageSize, x => x.Id, OrderByType.Asc);

        if (entities.Count == 0)
        {
            return (new List<MediaDownloadNotificationDto>(), totalCount);
        }

        // 按配置名读取前缀（忽略 ModuleName，兼容历史遗留的模块名不一致）
        var returnPrefix = await GetConfigurationValueAsync("ReturnDownloadUrlPrefix");
        var replacePrefix = await GetConfigurationValueAsync("ReplaceUrlPrefix");

        var items = entities.Select(e => new MediaDownloadNotificationDto
        {
            FileName = Path.GetFileName(e.SavePath),
            FileSize = e.Size,
            MimeType = e.MimeType,
            DownloadUrl = BuildDownloadUrl(e.SavePath, returnPrefix, replacePrefix),
            SourceType = "Telegram",
            SourceId = e.Id,
            ChatId = e.ChatId,
            ChatTitle = e.ChatTitle,
            CompletedAt = e.LastModificationTime ?? DateTime.UtcNow
        }).ToList();

        return (items, totalCount);
    }

    /// <summary>
    /// 标记指定媒体的外链已生成（下载器取回本地后回写），把 IsExternalLinkGenerated 置 true
    /// </summary>
    /// <param name="id">MediaInfo 主键 Id</param>
    /// <returns>实体不存在返回 false</returns>
    public async Task<bool> MarkExternalLinkGeneratedAsync(long id)
    {
        var entity = await Repository.GetFirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return false;
        }

        entity.IsExternalLinkGenerated = true;
        await Repository.UpdateAsync(entity);
        return true;
    }

    /// <summary>
    /// 生成 Apache 外链下载 URL，逻辑与 ListenTelegramService 推送通知一致
    /// </summary>
    private static string BuildDownloadUrl(string savePath, string? returnPrefix, string? replacePrefix)
    {
        if (string.IsNullOrWhiteSpace(returnPrefix) || string.IsNullOrWhiteSpace(replacePrefix))
        {
            return string.Empty;
        }

        var relative = savePath.Replace(replacePrefix, string.Empty).Replace("\\", "/");
        return Path.Combine(returnPrefix, relative.TrimStart('/')).Replace("\\", "/");
    }

    /// <summary>
    /// 按配置名读取值（忽略模块名，兼容历史遗留的 ModuleName 不一致；读不到返回 null）
    /// </summary>
    private async Task<string?> GetConfigurationValueAsync(string configurationName)
    {
        try
        {
            var info = await _configRepository.GetFirstOrDefaultAsync(x => x.ConfigurationName == configurationName);
            return info?.ConfigurationValue;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取图表数据（按聊天标题分组统计）
    /// </summary>
    /// <returns>图表数据 DTO</returns>
    public async Task<ChartDataDto> GetChartDataAsync()
    {
        var list = await Repository.GetListAsync();
        var temp = list.GroupBy(item => item.ChatTitle)
            .Select(item => new
            {
                Title = item.Key,
                Count = item.Count()
            });

        var dto = new ChartDataDto
        {
            Labels = new List<string>(temp.Count()),
            Datas = new List<int>(temp.Count())
        };

        foreach (var item in temp)
        {
            dto.Labels.Add(item.Title!);
            dto.Datas.Add(item.Count);
        }

        return dto;
    }

    /// <summary>
    /// 删除无效的媒体项（未下载完成且创建时间超过 1 分钟）
    /// </summary>
    public async Task DeleteInvalidItemsAsync()
    {
        await Repository.DeleteAsync(x =>
            !x.IsDownloadCompleted
            && x.CreationTime <= DateTime.Now.AddMinutes(-1));
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">媒体信息实体</param>
    /// <returns>媒体信息 DTO</returns>
    protected override MediaInfoDto MapToGetOutputDto(MediaInfo entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <returns>媒体信息实体</returns>
    protected override MediaInfo MapToEntity(CreateUpdateMediaInfoDto input)
    {
        return new MediaInfo
        {
            MediaId = input.MediaId,
            ChatId = input.ChatId,
            ChatTitle = input.ChatTitle,
            Message = input.Message,
            Size = input.Size,
            SavePath = input.SavePath,
            MimeType = input.MimeType,
            IsExternalLinkGenerated = input.IsExternalLinkGenerated
        };
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <param name="entity">媒体信息实体</param>
    protected override void MapToEntity(CreateUpdateMediaInfoDto input, MediaInfo entity)
    {
        entity.MediaId = input.MediaId;
        entity.ChatId = input.ChatId;
        entity.ChatTitle = input.ChatTitle;
        entity.Message = input.Message;
        entity.Size = input.Size;
        entity.SavePath = input.SavePath;
        entity.MimeType = input.MimeType;
        entity.IsExternalLinkGenerated = input.IsExternalLinkGenerated;
    }
}
