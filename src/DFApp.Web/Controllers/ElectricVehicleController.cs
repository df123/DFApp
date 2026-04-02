using System;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.ElectricVehicle;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElectricVehicleService = DFApp.Web.Services.ElectricVehicle.ElectricVehicleService;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 电动车管理控制器
/// </summary>
[ApiController]
[Route("api/app/electric-vehicle")]
[Authorize]
public class ElectricVehicleController : DFAppControllerBase
{
    private readonly ElectricVehicleService _electricVehicleService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="electricVehicleService">电动车服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public ElectricVehicleController(
        ElectricVehicleService electricVehicleService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _electricVehicleService = electricVehicleService;
    }

    /// <summary>
    /// 获取电动车列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.ElectricVehicle.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _electricVehicleService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取电动车详情
    /// </summary>
    [HttpGet("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicle.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id)
    {
        var result = await _electricVehicleService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取电动车列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.ElectricVehicle.Default)]
    public async Task<IActionResult> GetPagedListAsync(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _electricVehicleService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建电动车
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.ElectricVehicle.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateElectricVehicleDto input)
    {
        var result = await _electricVehicleService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新电动车
    /// </summary>
    [HttpPut("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicle.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] CreateUpdateElectricVehicleDto input)
    {
        var result = await _electricVehicleService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除电动车
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicle.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _electricVehicleService.DeleteAsync(id);
        return Success();
    }
}
