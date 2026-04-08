using System;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Lottery.Simulation;
using DFApp.Web.DTOs.Lottery.Simulation.KL8;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Lottery.Simulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 快乐8模拟控制器，提供快乐8模拟投注的增删改查及模拟功能
/// </summary>
[ApiController]
[Route("api/app/lottery-kl8-simulation")]
[Authorize]
public class LotteryKL8SimulationController : DFAppControllerBase
{
    private readonly LotteryKL8SimulationService _kl8SimulationService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="kl8SimulationService">快乐8模拟服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public LotteryKL8SimulationController(
        LotteryKL8SimulationService kl8SimulationService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _kl8SimulationService = kl8SimulationService;
    }

    /// <summary>
    /// 获取快乐8模拟数据列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _kl8SimulationService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取快乐8模拟数据详情
    /// </summary>
    [HttpGet("{id:guid}")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id)
    {
        var result = await _kl8SimulationService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取快乐8模拟数据列表（按组聚合）
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var skipCount = (pageIndex - 1) * pageSize;
        var result = await _kl8SimulationService.GetPagedListAsync(skipCount, pageSize);
        return Success(result);
    }

    /// <summary>
    /// 创建快乐8模拟数据
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.Lottery.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateLotterySimulationDto input)
    {
        var result = await _kl8SimulationService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新快乐8模拟数据
    /// </summary>
    [HttpPut("{id:guid}")]
    [Permission(DFAppPermissions.Lottery.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] CreateUpdateLotterySimulationDto input)
    {
        var result = await _kl8SimulationService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除快乐8模拟数据
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Permission(DFAppPermissions.Lottery.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _kl8SimulationService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 生成随机号码
    /// </summary>
    [HttpPost("generate-random")]
    [Permission(DFAppPermissions.Lottery.Create)]
    public async Task<IActionResult> GenerateRandomNumbers([FromBody] GenerateRandomNumbersDto input)
    {
        var result = await _kl8SimulationService.GenerateRandomNumbersAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 计算中奖金额
    /// </summary>
    [HttpGet("calculate-winning/{termNumber:int}")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> CalculateWinningAmount([FromRoute] int termNumber)
    {
        var result = await _kl8SimulationService.CalculateWinningAmountAsync(termNumber);
        return Success(result);
    }

    /// <summary>
    /// 获取统计数据
    /// </summary>
    [HttpGet("statistics")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _kl8SimulationService.GetStatisticsAsync();
        return Success(result);
    }

    /// <summary>
    /// 删除指定期号的所有模拟数据
    /// </summary>
    [HttpDelete("by-term/{termNumber:int}")]
    [Permission(DFAppPermissions.Lottery.Delete)]
    public async Task<IActionResult> DeleteByTermNumber([FromRoute] int termNumber)
    {
        await _kl8SimulationService.DeleteByTermNumberAsync(termNumber);
        return Success();
    }
}
