using System.Threading.Tasks;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.DTOs;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RssSourceAppService = DFApp.Web.Services.Rss.RssSourceAppService;
using PagedAndSortedResultRequestDto = Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto;

namespace DFApp.Web.Controllers;

/// <summary>
/// RSS源管理控制器，提供RSS源的增删改查及手动触发抓取功能
/// </summary>
[ApiController]
[Route("api/app/rss-source")]
[Authorize]
public class RssSourceController : DFAppControllerBase
{
    private readonly RssSourceAppService _rssSourceAppService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="rssSourceAppService">RSS源管理服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public RssSourceController(
        RssSourceAppService rssSourceAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _rssSourceAppService = rssSourceAppService;
    }

    /// <summary>
    /// 获取RSS源分页列表
    /// </summary>
    /// <param name="input">分页排序请求</param>
    [HttpGet]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
    {
        var result = await _rssSourceAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取RSS源详情
    /// </summary>
    /// <param name="id">RSS源ID</param>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.Rss.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _rssSourceAppService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 创建RSS源
    /// </summary>
    /// <param name="input">创建RSS源请求</param>
    [HttpPost]
    [Permission(DFAppPermissions.Rss.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateRssSourceDto input)
    {
        var result = await _rssSourceAppService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新RSS源
    /// </summary>
    /// <param name="id">RSS源ID</param>
    /// <param name="input">更新RSS源请求</param>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.Rss.Update)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateRssSourceDto input)
    {
        var result = await _rssSourceAppService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除RSS源
    /// </summary>
    /// <param name="id">RSS源ID</param>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.Rss.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _rssSourceAppService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 手动触发RSS源抓取
    /// </summary>
    /// <param name="id">RSS源ID</param>
    [HttpPost("{id:long}/trigger-fetch")]
    [Permission(DFAppPermissions.Rss.Download)]
    public async Task<IActionResult> TriggerFetchAsync([FromRoute] long id)
    {
        await _rssSourceAppService.TriggerFetchAsync(id);
        return Success();
    }
}
