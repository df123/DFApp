using System;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.ElectricVehicle;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElectricVehicleChargingRecordService = DFApp.Web.Services.ElectricVehicle.ElectricVehicleChargingRecordService;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 电动车充电记录控制器
/// </summary>
[ApiController]
[Route("api/app/electric-vehicle-charging-record")]
[Authorize]
public class ElectricVehicleChargingRecordController : DFAppControllerBase
{
    private readonly ElectricVehicleChargingRecordService _chargingRecordService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="chargingRecordService">充电记录服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public ElectricVehicleChargingRecordController(
        ElectricVehicleChargingRecordService chargingRecordService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _chargingRecordService = chargingRecordService;
    }

    /// <summary>
    /// 获取充电记录列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.ElectricVehicleChargingRecord.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _chargingRecordService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取充电记录详情
    /// </summary>
    [HttpGet("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicleChargingRecord.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id)
    {
        var result = await _chargingRecordService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取充电记录列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.ElectricVehicleChargingRecord.Default)]
    public async Task<IActionResult> GetPagedListAsync(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _chargingRecordService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 根据过滤条件分页查询充电记录
    /// </summary>
    [HttpGet("filtered")]
    [Permission(DFAppPermissions.ElectricVehicleChargingRecord.Default)]
    public async Task<IActionResult> GetFilteredListAsync(
        [FromQuery] string? filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _chargingRecordService.GetFilteredListAsync(filter, pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建充电记录
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.ElectricVehicleChargingRecord.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateElectricVehicleChargingRecordDto input)
    {
        var result = await _chargingRecordService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新充电记录
    /// </summary>
    [HttpPut("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicleChargingRecord.Edit)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] CreateUpdateElectricVehicleChargingRecordDto input)
    {
        var result = await _chargingRecordService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除充电记录
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicleChargingRecord.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _chargingRecordService.DeleteAsync(id);
        return Success();
    }
}
