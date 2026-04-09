using System;
using System.Threading.Tasks;
using DFApp.Web.DTOs.Identity;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 用户角色管理控制器，提供用户角色的查询、分配和移除功能
/// </summary>
public class UserRoleManagementController : DFAppControllerBase
{
    private readonly UserRoleManagementAppService _userRoleAppService;

    public UserRoleManagementController(
        UserRoleManagementAppService userRoleAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _userRoleAppService = userRoleAppService;
    }

    /// <summary>
    /// 获取用户的角色列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.UserRoleManagement.Default)]
    public async Task<IActionResult> GetUserRolesAsync([FromQuery] Guid userId)
    {
        var result = await _userRoleAppService.GetUserRolesAsync(userId);
        return Success(result);
    }

    /// <summary>
    /// 获取角色下的用户列表
    /// </summary>
    [HttpGet("users-by-role/{roleId:guid}")]
    [Permission(DFAppPermissions.UserRoleManagement.Default)]
    public async Task<IActionResult> GetUsersByRoleAsync(Guid roleId)
    {
        var result = await _userRoleAppService.GetUsersByRoleAsync(roleId);
        return Success(result);
    }

    /// <summary>
    /// 批量分配角色给用户
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.UserRoleManagement.Assign)]
    public async Task<IActionResult> AssignAsync([FromBody] AssignUserRolesDto input)
    {
        await _userRoleAppService.AssignAsync(input);
        return Success();
    }

    /// <summary>
    /// 批量移除用户的角色
    /// </summary>
    [HttpDelete]
    [Permission(DFAppPermissions.UserRoleManagement.Remove)]
    public async Task<IActionResult> RemoveAsync([FromBody] RemoveUserRolesDto input)
    {
        await _userRoleAppService.RemoveAsync(input);
        return Success();
    }
}
