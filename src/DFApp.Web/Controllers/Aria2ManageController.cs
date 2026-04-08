using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Aria2;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// Aria2 RPC 管理控制器，直接与 Aria2 交互，提供任务状态查询、任务操作等功能
/// </summary>
[ApiController]
[Route("api/app/aria2-manage")]
[Authorize]
public class Aria2ManageController : DFAppControllerBase
{
    private readonly Services.Aria2.Aria2ManageService _aria2ManageService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="aria2ManageService">Aria2 管理服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public Aria2ManageController(
        Services.Aria2.Aria2ManageService aria2ManageService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _aria2ManageService = aria2ManageService;
    }

    #region 状态查询

    /// <summary>
    /// 获取 Aria2 全局状态
    /// </summary>
    [HttpGet("global-stat")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetGlobalStatAsync()
    {
        var result = await _aria2ManageService.GetGlobalStatAsync();
        return Success(result);
    }

    /// <summary>
    /// 获取活跃任务列表
    /// </summary>
    [HttpGet("active-tasks")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetActiveTasksAsync()
    {
        var result = await _aria2ManageService.GetActiveTasksAsync();
        return Success(result);
    }

    /// <summary>
    /// 获取等待任务列表
    /// </summary>
    [HttpGet("waiting-tasks")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetWaitingTasksAsync()
    {
        var result = await _aria2ManageService.GetWaitingTasksAsync();
        return Success(result);
    }

    /// <summary>
    /// 获取停止任务列表
    /// </summary>
    /// <param name="offset">偏移量</param>
    /// <param name="num">数量上限</param>
    [HttpGet("stopped-tasks")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetStoppedTasksAsync([FromQuery] int offset = 0, [FromQuery] int num = 100)
    {
        var result = await _aria2ManageService.GetStoppedTasksAsync(offset, num);
        return Success(result);
    }

    /// <summary>
    /// 获取任务状态
    /// </summary>
    /// <param name="gid">任务 GID</param>
    [HttpGet("task-status/{gid}")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetTaskStatusAsync([FromRoute] string gid)
    {
        var result = await _aria2ManageService.GetTaskStatusAsync(gid);
        return Success(result);
    }

    /// <summary>
    /// 获取任务详情（包含 peers 和文件列表）
    /// </summary>
    /// <param name="gid">任务 GID</param>
    [HttpGet("task-detail/{gid}")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetTaskDetailAsync([FromRoute] string gid)
    {
        var result = await _aria2ManageService.GetTaskDetailAsync(gid);
        return Success(result);
    }

    /// <summary>
    /// 获取 Aria2 连接状态
    /// </summary>
    [HttpGet("connection-status")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetConnectionStatusAsync()
    {
        var result = await _aria2ManageService.GetConnectionStatusAsync();
        return Success(result);
    }

    /// <summary>
    /// 批量查询 IP 地理位置
    /// </summary>
    /// <param name="ips">IP 地址列表</param>
    [HttpPost("ip-geolocation")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> GetIpGeolocationAsync([FromBody] List<string> ips)
    {
        var result = await _aria2ManageService.GetIpGeolocationAsync(ips);
        return Success(result);
    }

    #endregion

    #region 任务操作

    /// <summary>
    /// 添加 URI 下载任务
    /// </summary>
    /// <param name="input">下载请求</param>
    [HttpPost("add-uri")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> AddUriAsync([FromBody] AddDownloadRequestDto input)
    {
        var result = await _aria2ManageService.AddUriAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 批量添加 URI 下载任务（每条链接创建独立任务）
    /// </summary>
    /// <param name="input">批量下载请求</param>
    [HttpPost("batch-add-uri")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> BatchAddUriAsync([FromBody] BatchAddUriRequestDto input)
    {
        var result = await _aria2ManageService.BatchAddUriAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 添加种子文件下载任务
    /// </summary>
    /// <param name="input">种子文件下载请求</param>
    [HttpPost("add-torrent")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> AddTorrentAsync([FromBody] AddTorrentRequestDto input)
    {
        var result = await _aria2ManageService.AddTorrentAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 批量添加种子文件下载任务
    /// </summary>
    /// <param name="input">批量种子文件下载请求</param>
    [HttpPost("batch-add-torrent")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> BatchAddTorrentAsync([FromBody] BatchAddTorrentRequestDto input)
    {
        var result = await _aria2ManageService.BatchAddTorrentAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="input">暂停任务请求</param>
    [HttpPost("pause")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> PauseTasksAsync([FromBody] PauseTasksRequestDto input)
    {
        var result = await _aria2ManageService.PauseTasksAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 暂停所有任务
    /// </summary>
    [HttpPost("pause-all")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> PauseAllTasksAsync()
    {
        var result = await _aria2ManageService.PauseAllTasksAsync();
        return Success(result);
    }

    /// <summary>
    /// 恢复任务
    /// </summary>
    /// <param name="input">恢复任务请求</param>
    [HttpPost("unpause")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> UnpauseTasksAsync([FromBody] PauseTasksRequestDto input)
    {
        var result = await _aria2ManageService.UnpauseTasksAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 恢复所有任务
    /// </summary>
    [HttpPost("unpause-all")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> UnpauseAllTasksAsync()
    {
        var result = await _aria2ManageService.UnpauseAllTasksAsync();
        return Success(result);
    }

    /// <summary>
    /// 停止任务（移除活跃或等待中的任务）
    /// </summary>
    /// <param name="input">停止任务请求</param>
    [HttpPost("stop")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> StopTasksAsync([FromBody] StopTasksRequestDto input)
    {
        var result = await _aria2ManageService.StopTasksAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 删除停止的任务
    /// </summary>
    /// <param name="input">删除任务请求</param>
    [HttpPost("remove")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> RemoveTasksAsync([FromBody] RemoveTasksRequestDto input)
    {
        var result = await _aria2ManageService.RemoveTasksAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 清空已停止的下载结果
    /// </summary>
    [HttpPost("purge")]
    [Permission(DFAppPermissions.Aria2.Default)]
    public async Task<IActionResult> PurgeDownloadResultAsync()
    {
        var result = await _aria2ManageService.PurgeDownloadResultAsync();
        return Success(result);
    }

    #endregion
}
