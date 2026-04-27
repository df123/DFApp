using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Media;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 媒体外链控制器，提供外链的查询和删除功能（创建和更新操作已被禁用）
/// </summary>
[ApiController]
[Route("api/app/external-link")]
[Authorize]
public class ExternalLinkController : DFAppControllerBase
{
    private readonly ExternalLinkService _externalLinkService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="externalLinkService">外链服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public ExternalLinkController(
        ExternalLinkService externalLinkService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _externalLinkService = externalLinkService;
    }

    /// <summary>
    /// 获取外链列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.Medias.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _externalLinkService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取外链详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.Medias.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _externalLinkService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 生成外链（后台任务）
    /// </summary>
    [HttpGet("external-link")]
    [Permission(DFAppPermissions.Medias.Create)]
    public async Task<IActionResult> GetExternalLinkAsync()
    {
        var result = await _externalLinkService.GetExternalLink();
        return Success(result);
    }

    /// <summary>
    /// 分页获取外链列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.Medias.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _externalLinkService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 删除外链记录（同时移除关联的物理文件）
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.Medias.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _externalLinkService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 仅移除外链关联的物理文件
    /// </summary>
    [HttpDelete("{id:long}/file")]
    [Permission(DFAppPermissions.Medias.Delete)]
    public async Task<IActionResult> RemoveFileAsync([FromRoute] long id)
    {
        await _externalLinkService.RemoveFileAsync(id);
        return Success();
    }
}
