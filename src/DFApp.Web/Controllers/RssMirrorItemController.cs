using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs;
using DFApp.Web.DTOs.Rss;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RssMirrorItemAppService = DFApp.Web.Services.Rss.RssMirrorItemAppService;

namespace DFApp.Web.Controllers;

/// <summary>
/// RSS镜像条目管理控制器，提供镜像条目的查询、删除及分词统计等功能
/// </summary>
[ApiController]
[Route("api/app/rss-mirror-item")]
[Authorize]
public class RssMirrorItemController : DFAppControllerBase
{
    private readonly RssMirrorItemAppService _rssMirrorItemAppService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="rssMirrorItemAppService">RSS镜像条目管理服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public RssMirrorItemController(
        RssMirrorItemAppService rssMirrorItemAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _rssMirrorItemAppService = rssMirrorItemAppService;
    }

    /// <summary>
    /// 获取镜像条目分页列表
    /// </summary>
    /// <param name="input">查询请求</param>
    [HttpGet]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetRssMirrorItemsRequestDto input)
    {
        var result = await _rssMirrorItemAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取镜像条目详情
    /// </summary>
    /// <param name="id">镜像条目ID</param>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _rssMirrorItemAppService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 删除镜像条目
    /// </summary>
    /// <param name="id">镜像条目ID</param>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.Rss.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _rssMirrorItemAppService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 批量删除镜像条目
    /// </summary>
    /// <param name="ids">镜像条目ID列表</param>
    [HttpDelete("many")]
    [Permission(DFAppPermissions.Rss.Delete)]
    public async Task<IActionResult> DeleteManyAsync([FromBody] List<long> ids)
    {
        await _rssMirrorItemAppService.DeleteManyAsync(ids);
        return Success();
    }

    /// <summary>
    /// 获取分词统计
    /// </summary>
    /// <param name="rssSourceId">RSS源ID（可选）</param>
    /// <param name="languageType">语言类型（可选）</param>
    /// <param name="top">返回前N条</param>
    [HttpGet("word-segment-statistics")]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetWordSegmentStatisticsAsync(
        [FromQuery] long? rssSourceId = null,
        [FromQuery] int? languageType = null,
        [FromQuery] int top = 100)
    {
        var result = await _rssMirrorItemAppService.GetWordSegmentStatisticsAsync(rssSourceId, languageType, top);
        return Success(result);
    }

    /// <summary>
    /// 根据分词获取镜像条目
    /// </summary>
    /// <param name="wordToken">分词文本</param>
    /// <param name="input">分页排序请求</param>
    [HttpGet("by-word-token/{wordToken}")]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetByWordTokenAsync(
        [FromRoute] string wordToken,
        [FromQuery] PagedAndSortedResultRequestDto input)
    {
        var result = await _rssMirrorItemAppService.GetByWordTokenAsync(wordToken, input);
        return Success(result);
    }

    /// <summary>
    /// 清空所有镜像数据
    /// </summary>
    [HttpDelete("clear-all")]
    [Permission(DFAppPermissions.Rss.Delete)]
    public async Task<IActionResult> ClearAllAsync()
    {
        await _rssMirrorItemAppService.ClearAllAsync();
        return Success();
    }

    /// <summary>
    /// 下载到Aria2
    /// </summary>
    /// <param name="id">镜像条目ID</param>
    /// <param name="videoOnly">仅下载视频</param>
    /// <param name="enableKeywordFilter">启用关键词过滤</param>
    [HttpPost("{id:long}/download-to-aria2")]
    [Permission(DFAppPermissions.Rss.Download)]
    public async Task<IActionResult> DownloadToAria2Async(
        [FromRoute] long id,
        [FromQuery] bool videoOnly = false,
        [FromQuery] bool enableKeywordFilter = false)
    {
        var result = await _rssMirrorItemAppService.DownloadToAria2Async(id, videoOnly, enableKeywordFilter);
        return Success(result);
    }
}
