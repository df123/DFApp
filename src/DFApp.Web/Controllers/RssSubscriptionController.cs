using System.Threading.Tasks;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.DTOs;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RssSubscriptionAppService = DFApp.Web.Services.Rss.RssSubscriptionAppService;

namespace DFApp.Web.Controllers;

/// <summary>
/// RSS订阅管理控制器，提供RSS订阅的增删改查及启用/禁用切换功能
/// </summary>
[ApiController]
[Route("api/app/rss-subscription")]
[Authorize]
public class RssSubscriptionController : DFAppControllerBase
{
    private readonly RssSubscriptionAppService _rssSubscriptionAppService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="rssSubscriptionAppService">RSS订阅管理服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public RssSubscriptionController(
        RssSubscriptionAppService rssSubscriptionAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _rssSubscriptionAppService = rssSubscriptionAppService;
    }

    /// <summary>
    /// 获取RSS订阅分页列表
    /// </summary>
    /// <param name="input">查询请求</param>
    [HttpGet]
    [Permission(DFAppPermissions.RssSubscription.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetRssSubscriptionsRequestDto input)
    {
        var result = await _rssSubscriptionAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取RSS订阅详情
    /// </summary>
    /// <param name="id">订阅ID</param>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.RssSubscription.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _rssSubscriptionAppService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 创建RSS订阅
    /// </summary>
    /// <param name="input">创建订阅请求</param>
    [HttpPost]
    [Permission(DFAppPermissions.RssSubscription.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateRssSubscriptionDto input)
    {
        var result = await _rssSubscriptionAppService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新RSS订阅
    /// </summary>
    /// <param name="id">订阅ID</param>
    /// <param name="input">更新订阅请求</param>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.RssSubscription.Update)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateRssSubscriptionDto input)
    {
        var result = await _rssSubscriptionAppService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除RSS订阅
    /// </summary>
    /// <param name="id">订阅ID</param>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.RssSubscription.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _rssSubscriptionAppService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 切换订阅启用状态
    /// </summary>
    /// <param name="id">订阅ID</param>
    [HttpPost("{id:long}/toggle-enable")]
    [Permission(DFAppPermissions.RssSubscription.Update)]
    public async Task<IActionResult> ToggleEnableAsync([FromRoute] long id)
    {
        await _rssSubscriptionAppService.ToggleEnableAsync(id);
        return Success();
    }
}
