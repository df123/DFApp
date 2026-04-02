using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// Aria2 下载任务管理控制器，提供下载记录的查询、外链获取、添加下载及清理功能
/// </summary>
[ApiController]
[Route("api/app/aria2")]
[Authorize]
public class Aria2Controller : DFAppControllerBase
{
    private readonly Services.Aria2.Aria2Service _aria2Service;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="aria2Service">Aria2 下载管理服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public Aria2Controller(
        Services.Aria2.Aria2Service aria2Service,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _aria2Service = aria2Service;
    }

    /// <summary>
    /// 根据过滤条件分页查询下载记录
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    [HttpGet("filtered-list")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetFilteredListAsync(
        [FromQuery] string? filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _aria2Service.GetFilteredListAsync(filter, pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 根据 ID 获取下载记录
    /// </summary>
    /// <param name="id">下载记录 ID</param>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _aria2Service.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 获取所有下载记录列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _aria2Service.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 获取指定下载记录的外链
    /// </summary>
    /// <param name="id">下载记录 ID</param>
    [HttpGet("{id:long}/external-link")]
    [Permission(DFAppPermissions.Aria2.Link)]
    public async Task<IActionResult> GetExternalLinkAsync([FromRoute] long id)
    {
        var result = await _aria2Service.GetExternalLinkAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 获取所有下载记录的外链列表
    /// </summary>
    /// <param name="videoOnly">是否只获取视频文件</param>
    [HttpGet("all-external-links")]
    [Permission(DFAppPermissions.Aria2.Link)]
    public async Task<IActionResult> GetAllExternalLinksAsync([FromQuery] bool videoOnly = true)
    {
        var result = await _aria2Service.GetAllExternalLinksAsync(videoOnly);
        return Success(result);
    }

    /// <summary>
    /// 添加下载任务
    /// </summary>
    /// <param name="input">下载请求</param>
    [HttpPost("add-download")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> AddDownloadAsync([FromBody] DFApp.Aria2.AddDownloadRequestDto input)
    {
        var result = await _aria2Service.AddDownloadAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 删除指定下载记录及关联文件
    /// </summary>
    /// <param name="id">下载记录 ID</param>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.Aria2.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _aria2Service.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 删除所有下载记录及关联文件
    /// </summary>
    [HttpDelete("all")]
    [Permission(DFAppPermissions.Aria2.Delete)]
    public async Task<IActionResult> DeleteAllAsync()
    {
        await _aria2Service.DeleteAllAsync();
        return Success();
    }

    /// <summary>
    /// 清空下载目录
    /// </summary>
    [HttpDelete("clear-directory")]
    [Permission(DFAppPermissions.Aria2.Delete)]
    public async Task<IActionResult> ClearDownloadDirectoryAsync()
    {
        await _aria2Service.ClearDownloadDirectoryAsync();
        return Success();
    }
}
