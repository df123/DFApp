using System.Threading.Tasks;
using DFApp.Web.DTOs.Identity;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 权限授予管理控制器，提供权限的查询、授予、撤销和全量更新功能
/// </summary>
public class PermissionGrantManagementController : DFAppControllerBase
{
    private readonly PermissionGrantManagementAppService _permissionGrantAppService;

    public PermissionGrantManagementController(
        PermissionGrantManagementAppService permissionGrantAppService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _permissionGrantAppService = permissionGrantAppService;
    }

    /// <summary>
    /// 查询指定 Provider 的已授予权限列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.PermissionGrantManagement.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetPermissionGrantListDto input)
    {
        var result = await _permissionGrantAppService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 获取所有权限定义（权限树结构）
    /// </summary>
    [HttpGet("all-permissions")]
    [Permission(DFAppPermissions.PermissionGrantManagement.Default)]
    public async Task<IActionResult> GetAllPermissionsAsync()
    {
        var result = await _permissionGrantAppService.GetAllPermissionsAsync();
        return Success(result);
    }

    /// <summary>
    /// 全量更新指定 Provider 的权限
    /// </summary>
    [HttpPut]
    [Permission(DFAppPermissions.PermissionGrantManagement.Grant)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdatePermissionsDto input)
    {
        await _permissionGrantAppService.UpdateAsync(input);
        return Success();
    }

    /// <summary>
    /// 增量授予权限
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.PermissionGrantManagement.Grant)]
    public async Task<IActionResult> GrantAsync([FromBody] GrantPermissionsDto input)
    {
        await _permissionGrantAppService.GrantAsync(input);
        return Success();
    }

    /// <summary>
    /// 撤销权限
    /// </summary>
    [HttpDelete]
    [Permission(DFAppPermissions.PermissionGrantManagement.Revoke)]
    public async Task<IActionResult> RevokeAsync([FromBody] RevokePermissionsDto input)
    {
        await _permissionGrantAppService.RevokeAsync(input);
        return Success();
    }
}
