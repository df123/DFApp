using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DFApp.Media;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services.Media;

/// <summary>
/// 媒体信息服务
/// </summary>
public class MediaInfoService : CrudServiceBase<MediaInfo, long, MediaInfoDto, CreateUpdateMediaInfoDto, CreateUpdateMediaInfoDto>
{
    private readonly MediaMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    public MediaInfoService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<MediaInfo, long> repository)
        : base(currentUser, permissionChecker, repository)
    {
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
