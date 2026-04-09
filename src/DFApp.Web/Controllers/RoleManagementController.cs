using System;
using System.Threading.Tasks;
using DFApp.Web.DTOs.Identity;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 角色管理控制器，提供角色的增删改查功能
/// </summary>
public class RoleManagementController : DFAppControllerBase
{
    private readonly RoleManagementAppService _roleAppService;

    public RoleManagementController(
        RoleManagementAppService roleAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _roleAppService = roleAppService;
    }

    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.RoleManagement.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetRoleListDto input)
    {
        var result = await _roleAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 获取所有角色列表（不分页）
    /// </summary>
    [HttpGet("all")]
    [Permission(DFAppPermissions.RoleManagement.Default)]
    public async Task<IActionResult> GetAllListAsync()
    {
        var result = await _roleAppService.GetAllListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取角色详情
    /// </summary>
    [HttpGet("{id:guid}")]
    [Permission(DFAppPermissions.RoleManagement.Default)]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var result = await _roleAppService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.RoleManagement.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateRoleDto input)
    {
        var result = await _roleAppService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新角色信息
    /// </summary>
    [HttpPut("{id:guid}")]
    [Permission(DFAppPermissions.RoleManagement.Update)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateRoleDto input)
    {
        var result = await _roleAppService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Permission(DFAppPermissions.RoleManagement.Delete)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _roleAppService.DeleteAsync(id);
        return Success();
    }
}
