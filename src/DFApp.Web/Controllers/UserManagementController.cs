using System;
using System.Threading.Tasks;
using DFApp.Web.DTOs.Account;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 用户管理控制器，提供用户的增删改查及密码管理功能
/// </summary>
[ApiController]
[Route("api/app/user-management")]
[Authorize]
public class UserManagementController : DFAppControllerBase
{
    private readonly UserManagementAppService _userManagementAppService;

    public UserManagementController(
        UserManagementAppService userManagementAppService,
        DFApp.Web.Data.ICurrentUser currentUser,
        DFApp.Web.Permissions.IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _userManagementAppService = userManagementAppService;
    }

    /// <summary>
    /// 获取用户列表（分页）
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.UserManagement.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetUserListDto input)
    {
        var result = await _userManagementAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取用户详情
    /// </summary>
    [HttpGet("{id:guid}")]
    [Permission(DFAppPermissions.UserManagement.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id)
    {
        var result = await _userManagementAppService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.UserManagement.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserDto input)
    {
        var result = await _userManagementAppService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    [HttpPut("{id:guid}")]
    [Permission(DFAppPermissions.UserManagement.Update)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateUserDto input)
    {
        var result = await _userManagementAppService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Permission(DFAppPermissions.UserManagement.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _userManagementAppService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    [HttpPost("change-password")]
    [Permission(DFAppPermissions.UserManagement.ChangePassword)]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto input)
    {
        await _userManagementAppService.ChangePasswordAsync(input);
        return Success();
    }
}
