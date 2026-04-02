using System;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.ElectricVehicle;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElectricVehicleCostService = DFApp.Web.Services.ElectricVehicle.ElectricVehicleCostService;

namespace DFApp.Web.Controllers;

/// <summary>
/// 电动车成本记录控制器
/// </summary>
[ApiController]
[Route("api/app/electric-vehicle-cost")]
[Authorize]
public class ElectricVehicleCostController : DFAppControllerBase
{
    private readonly ElectricVehicleCostService _electricVehicleCostService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="electricVehicleCostService">电动车成本服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public ElectricVehicleCostController(
        ElectricVehicleCostService electricVehicleCostService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _electricVehicleCostService = electricVehicleCostService;
    }

    /// <summary>
    /// 获取电动车成本记录列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.ElectricVehicleCost.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _electricVehicleCostService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取成本记录详情
    /// </summary>
    [HttpGet("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicleCost.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id)
    {
        var result = await _electricVehicleCostService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取成本记录列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.ElectricVehicleCost.Default)]
    public async Task<IActionResult> GetPagedListAsync(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _electricVehicleCostService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 根据过滤条件分页查询成本记录
    /// </summary>
    [HttpGet("filtered")]
    [Permission(DFAppPermissions.ElectricVehicleCost.Default)]
    public async Task<IActionResult> GetFilteredListAsync(
        [FromQuery] string? filter,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _electricVehicleCostService.GetFilteredListAsync(filter, pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 获取油电成本对比数据
    /// </summary>
    [HttpPost("oil-cost-comparison")]
    [Permission(DFAppPermissions.ElectricVehicleCost.Analysis)]
    public async Task<IActionResult> GetOilCostComparisonAsync([FromBody] OilCostComparisonRequestDto input)
    {
        var result = await _electricVehicleCostService.GetOilCostComparisonAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 创建成本记录
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.ElectricVehicleCost.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateElectricVehicleCostDto input)
    {
        var result = await _electricVehicleCostService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新成本记录
    /// </summary>
    [HttpPut("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicleCost.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] CreateUpdateElectricVehicleCostDto input)
    {
        var result = await _electricVehicleCostService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除成本记录
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Permission(DFAppPermissions.ElectricVehicleCost.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _electricVehicleCostService.DeleteAsync(id);
        return Success();
    }
}
