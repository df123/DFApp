using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DFApp.Media;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Media;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MediaInfoService = DFApp.Web.Services.Media.MediaInfoService;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 媒体信息控制器，提供媒体信息的增删改查、文件下载及自定义查询功能
/// </summary>
[ApiController]
[Route("api/app/media-info")]
[Authorize]
public class MediaInfoController : DFAppControllerBase
{
    private readonly MediaInfoService _mediaInfoService;
    private readonly FileExtensionContentTypeProvider _typeProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mediaInfoService">媒体信息服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public MediaInfoController(
        MediaInfoService mediaInfoService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _mediaInfoService = mediaInfoService;
        _typeProvider = new FileExtensionContentTypeProvider();
        _typeProvider.Mappings[".iso"] = "application/octet-stream";
    }

    /// <summary>
    /// 获取媒体信息列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.Medias.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _mediaInfoService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取媒体信息详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.Medias.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _mediaInfoService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 根据过滤条件分页获取媒体信息列表
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.Medias.Default)]
    public async Task<IActionResult> GetFilteredPagedListAsync(
        [FromQuery] string? filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _mediaInfoService.GetFilteredPagedListAsync(filter, pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建媒体信息
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.Medias.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateMediaInfoDto input)
    {
        var result = await _mediaInfoService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新媒体信息
    /// </summary>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.Medias.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateMediaInfoDto input)
    {
        var result = await _mediaInfoService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除媒体信息
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.Medias.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _mediaInfoService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 获取图表数据（按聊天标题分组统计）
    /// </summary>
    [HttpGet("chart-data")]
    [Permission(DFAppPermissions.Medias.Default)]
    public async Task<IActionResult> GetChartDataAsync()
    {
        var result = await _mediaInfoService.GetChartDataAsync();
        return Success(result);
    }

    /// <summary>
    /// 删除无效的媒体项（未下载完成且创建时间超过 1 分钟）
    /// </summary>
    [HttpDelete("invalid")]
    [Permission(DFAppPermissions.Medias.Delete)]
    public async Task<IActionResult> DeleteInvalidItemsAsync()
    {
        await _mediaInfoService.DeleteInvalidItemsAsync();
        return Success();
    }

    /// <summary>
    /// 下载媒体文件
    /// 根据媒体记录 ID 返回文件流，同时更新修改时间以记录下载行为
    /// </summary>
    /// <param name="id">媒体记录 ID</param>
    [HttpGet("download")]
    [Permission(DFAppPermissions.Medias.Download)]
    public async Task<IActionResult> DownloadAsync([FromQuery] long id)
    {
        var dto = await _mediaInfoService.GetAsync(id);
        if (dto == null)
        {
            ThrowBusinessException("媒体记录不存在");
            return Fail("媒体记录不存在");
        }

        if (string.IsNullOrWhiteSpace(dto.SavePath))
        {
            ThrowBusinessException("媒体文件路径为空");
            return Fail("媒体文件路径为空");
        }

        if (!_typeProvider.TryGetContentType(dto.SavePath, out var contentType) || string.IsNullOrWhiteSpace(contentType))
        {
            contentType = "application/octet-stream";
        }

        var fileDownloadName = Path.GetFileName(dto.SavePath);

        var fs = new FileStream(dto.SavePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var fileStreamResult = new FileStreamResult(fs, contentType)
        {
            FileDownloadName = fileDownloadName,
            EnableRangeProcessing = true
        };

        // 将 MediaInfoDto 映射为 CreateUpdateMediaInfoDto 并更新，以记录下载行为（更新 LastModificationTime）
        var updateDto = new CreateUpdateMediaInfoDto
        {
            MediaId = long.Parse(dto.MediaId),
            ChatId = dto.ChatId,
            ChatTitle = dto.ChatTitle,
            Message = dto.Message,
            Size = dto.Size,
            SavePath = dto.SavePath,
            MD5 = dto.MD5,
            MimeType = dto.MimeType,
            IsExternalLinkGenerated = dto.IsExternalLinkGenerated
        };
        await _mediaInfoService.UpdateAsync(dto.Id, updateDto);

        return fileStreamResult;
    }
}
