using System;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.ElectricVehicle;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GasolinePriceService = DFApp.Web.Services.ElectricVehicle.GasolinePriceService;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 油价信息控制器
/// </summary>
[ApiController]
[Route("api/app/gasoline-price")]
[Authorize]
public class GasolinePriceController : DFAppControllerBase
{
    private readonly GasolinePriceService _gasolinePriceService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="gasolinePriceService">油价服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public GasolinePriceController(
        GasolinePriceService gasolinePriceService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _gasolinePriceService = gasolinePriceService;
    }

    /// <summary>
    /// 获取指定省份的最新汽油价格
    /// </summary>
    [HttpGet("latest")]
    [Permission(DFAppPermissions.GasolinePrice.Default)]
    public async Task<IActionResult> GetLatestPriceAsync([FromQuery] string province)
    {
        var result = await _gasolinePriceService.GetLatestPriceAsync(province);
        return Success(result);
    }

    /// <summary>
    /// 获取指定省份和日期的汽油价格
    /// </summary>
    [HttpGet("by-date")]
    [Permission(DFAppPermissions.GasolinePrice.Default)]
    public async Task<IActionResult> GetPriceByDateAsync([FromQuery] string province, [FromQuery] DateTime date)
    {
        var result = await _gasolinePriceService.GetPriceByDateAsync(province, date);
        return Success(result);
    }

    /// <summary>
    /// 获取汽油价格列表
    /// </summary>
    [HttpGet("list")]
    [Permission(DFAppPermissions.GasolinePrice.Default)]
    public async Task<IActionResult> GetListAsync([FromQuery] GetGasolinePricesDto input)
    {
        var result = await _gasolinePriceService.GetListAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 刷新汽油价格数据
    /// </summary>
    [HttpPost("refresh")]
    [Permission(DFAppPermissions.ElectricVehicle.Statistics)]
    public async Task<IActionResult> RefreshGasolinePricesAsync()
    {
        await _gasolinePriceService.RefreshGasolinePricesAsync();
        return Success();
    }
}
