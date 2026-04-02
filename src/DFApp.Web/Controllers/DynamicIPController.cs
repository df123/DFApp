using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.IP;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 动态 IP 管理控制器，提供动态 IP 的增删改查功能
/// </summary>
[ApiController]
[Route("api/app/dynamic-ip")]
[Authorize]
public class DynamicIPController : DFAppControllerBase
{
    private readonly Services.IP.DynamicIPService _dynamicIPService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dynamicIPService">动态 IP 服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public DynamicIPController(
        Services.IP.DynamicIPService dynamicIPService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _dynamicIPService = dynamicIPService;
    }

    /// <summary>
    /// 获取动态 IP 列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.DynamicIP.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _dynamicIPService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取动态 IP 详情
    /// </summary>
    [HttpGet("{id:guid}")]
    [Permission(DFAppPermissions.DynamicIP.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id)
    {
        var result = await _dynamicIPService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取动态 IP 列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.DynamicIP.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _dynamicIPService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建动态 IP
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.DynamicIP.Default)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateDynamicIPDto input)
    {
        var result = await _dynamicIPService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新动态 IP
    /// </summary>
    [HttpPut("{id:guid}")]
    [Permission(DFAppPermissions.DynamicIP.Default)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] CreateUpdateDynamicIPDto input)
    {
        var result = await _dynamicIPService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除动态 IP
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Permission(DFAppPermissions.DynamicIP.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _dynamicIPService.DeleteAsync(id);
        return Success();
    }
}
