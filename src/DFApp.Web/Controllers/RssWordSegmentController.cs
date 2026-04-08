using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Rss;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RssWordSegmentAppService = DFApp.Web.Services.Rss.RssWordSegmentAppService;

namespace DFApp.Web.Controllers;

/// <summary>
/// RSS分词管理控制器，提供分词列表查询、统计及删除功能
/// </summary>
[ApiController]
[Route("api/app/rss-word-segment")]
[Authorize]
public class RssWordSegmentController : DFAppControllerBase
{
    private readonly RssWordSegmentAppService _rssWordSegmentAppService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="rssWordSegmentAppService">RSS分词管理服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public RssWordSegmentController(
        RssWordSegmentAppService rssWordSegmentAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _rssWordSegmentAppService = rssWordSegmentAppService;
    }

    /// <summary>
    /// 获取分词列表（分页）
    /// </summary>
    /// <param name="input">查询请求</param>
    [HttpGet]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetRssWordSegmentsRequestDto input)
    {
        var result = await _rssWordSegmentAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 获取分词统计（带分页）
    /// </summary>
    /// <param name="input">查询请求</param>
    [HttpGet("statistics")]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetStatisticsAsync([FromQuery] GetRssWordSegmentsRequestDto input)
    {
        var result = await _rssWordSegmentAppService.GetStatisticsAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 删除指定RSS镜像条目的所有分词
    /// </summary>
    /// <param name="rssMirrorItemId">RSS镜像条目ID</param>
    [HttpDelete("by-item/{rssMirrorItemId:long}")]
    [Permission(DFAppPermissions.Rss.Delete)]
    public async Task<IActionResult> DeleteByItemAsync([FromRoute] long rssMirrorItemId)
    {
        await _rssWordSegmentAppService.DeleteByItemAsync(rssMirrorItemId);
        return Success();
    }

    /// <summary>
    /// 删除指定RSS源的所有分词
    /// </summary>
    /// <param name="rssSourceId">RSS源ID</param>
    [HttpDelete("by-source/{rssSourceId:long}")]
    [Permission(DFAppPermissions.Rss.Delete)]
    public async Task<IActionResult> DeleteBySourceAsync([FromRoute] long rssSourceId)
    {
        await _rssWordSegmentAppService.DeleteBySourceAsync(rssSourceId);
        return Success();
    }
}
