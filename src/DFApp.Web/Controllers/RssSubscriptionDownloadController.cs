using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Rss;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RssSubscriptionDownloadAppService = DFApp.Web.Services.Rss.RssSubscriptionDownloadAppService;

namespace DFApp.Web.Controllers;

/// <summary>
/// RSS订阅下载记录管理控制器，提供下载记录的查询、删除、清空及重试功能
/// </summary>
[ApiController]
[Route("api/app/rss-subscription-download")]
[Authorize]
public class RssSubscriptionDownloadController : DFAppControllerBase
{
    private readonly RssSubscriptionDownloadAppService _rssSubscriptionDownloadAppService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="rssSubscriptionDownloadAppService">RSS订阅下载记录管理服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public RssSubscriptionDownloadController(
        RssSubscriptionDownloadAppService rssSubscriptionDownloadAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _rssSubscriptionDownloadAppService = rssSubscriptionDownloadAppService;
    }

    /// <summary>
    /// 获取下载记录分页列表
    /// </summary>
    /// <param name="input">查询请求</param>
    [HttpGet]
    [Permission(DFAppPermissions.RssSubscription.Download)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetRssSubscriptionDownloadsRequestDto input)
    {
        var result = await _rssSubscriptionDownloadAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取下载记录详情
    /// </summary>
    /// <param name="id">下载记录ID</param>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.RssSubscription.Download)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _rssSubscriptionDownloadAppService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 删除下载记录
    /// </summary>
    /// <param name="id">下载记录ID</param>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.RssSubscription.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _rssSubscriptionDownloadAppService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 批量删除下载记录
    /// </summary>
    /// <param name="ids">下载记录ID列表</param>
    [HttpDelete("many")]
    [Permission(DFAppPermissions.RssSubscription.Delete)]
    public async Task<IActionResult> DeleteManyAsync([FromBody] List<long> ids)
    {
        await _rssSubscriptionDownloadAppService.DeleteManyAsync(ids);
        return Success();
    }

    /// <summary>
    /// 清空所有下载记录
    /// </summary>
    [HttpDelete("clear-all")]
    [Permission(DFAppPermissions.RssSubscription.Delete)]
    public async Task<IActionResult> ClearAllAsync()
    {
        await _rssSubscriptionDownloadAppService.ClearAllAsync();
        return Success();
    }

    /// <summary>
    /// 重试下载
    /// </summary>
    /// <param name="id">下载记录ID</param>
    [HttpPost("{id:long}/retry")]
    [Permission(DFAppPermissions.RssSubscription.Download)]
    public async Task<IActionResult> RetryAsync([FromRoute] long id)
    {
        await _rssSubscriptionDownloadAppService.RetryAsync(id);
        return Success();
    }
}
