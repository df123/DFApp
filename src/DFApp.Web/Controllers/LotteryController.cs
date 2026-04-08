using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.DTOs;
using DFApp.Web.DTOs.Lottery;
using DFApp.Web.DTOs.Lottery.Consts;
using DFApp.Web.DTOs.Lottery.Statistics;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateUpdateLotteryDto = DFApp.Web.DTOs.Lottery.CreateUpdateLotteryDto;
using LotteryDto = DFApp.Web.DTOs.Lottery.LotteryDto;
using LotteryService = DFApp.Web.Services.Lottery.LotteryService;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 彩票信息控制器，提供彩票信息的增删改查及统计功能
/// </summary>
[ApiController]
[Route("api/app/lottery")]
[Authorize]
public class LotteryController : DFAppControllerBase
{
    private readonly LotteryService _lotteryService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="lotteryService">彩票信息服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public LotteryController(
        LotteryService lotteryService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _lotteryService = lotteryService;
    }

    /// <summary>
    /// 获取彩票信息列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _lotteryService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取彩票信息详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _lotteryService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取彩票信息列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _lotteryService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建彩票信息
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.Lottery.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateLotteryDto input)
    {
        var result = await _lotteryService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新彩票信息
    /// </summary>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.Lottery.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateLotteryDto input)
    {
        var result = await _lotteryService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除彩票信息
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.Lottery.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _lotteryService.DeleteAsync(id);
        return Success();
    }

    /// <summary>
    /// 批量创建彩票
    /// </summary>
    [HttpPost("batch")]
    [Permission(DFAppPermissions.Lottery.Create)]
    public async Task<IActionResult> CreateLotteryBatch([FromBody] List<CreateUpdateLotteryDto> input)
    {
        var result = await _lotteryService.CreateLotteryBatch(input);
        return Success(result);
    }

    /// <summary>
    /// 计算组合投注
    /// </summary>
    [HttpPost("combination")]
    [Permission(DFAppPermissions.Lottery.Create)]
    public async Task<IActionResult> CalculateCombination([FromBody] LotteryCombinationDto input)
    {
        var result = await _lotteryService.CalculateCombination(input);
        return Success(result);
    }

    /// <summary>
    /// 获取中奖统计项（分页）
    /// </summary>
    [HttpGet("statistics-win-item")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetStatisticsWinItem(
        [FromQuery] string? purchasedPeriod,
        [FromQuery] string? winningPeriod,
        [FromQuery] string? lotteryType,
        [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 10)
    {
        var input = new StatisticsWinItemRequestDto
        {
            PurchasedPeriod = purchasedPeriod,
            WinningPeriod = winningPeriod,
            LotteryType = lotteryType,
            SkipCount = skipCount,
            MaxResultCount = maxResultCount
        };
        var result = await _lotteryService.GetStatisticsWinItem(input);
        return Success(result);
    }

    /// <summary>
    /// 获取中奖统计
    /// </summary>
    [HttpGet("statistics-win")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetStatisticsWin(
        [FromQuery] string? purchasedPeriod,
        [FromQuery] string? winningPeriod,
        [FromQuery] string lotteryType)
    {
        var result = await _lotteryService.GetStatisticsWin(purchasedPeriod, winningPeriod, lotteryType);
        return Success(result);
    }

    /// <summary>
    /// 通过输入 DTO 获取中奖统计项
    /// </summary>
    [HttpPost("statistics-win-item-by-input")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetStatisticsWinItemByInput([FromBody] StatisticsInputDto input)
    {
        var result = await _lotteryService.GetStatisticsWinItemInputDto(input);
        return Success(result);
    }

    /// <summary>
    /// 获取彩票常量
    /// </summary>
    [HttpGet("lottery-const")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public IActionResult GetLotteryConst()
    {
        var result = _lotteryService.GetLotteryConst();
        return Success(result);
    }

    /// <summary>
    /// 获取分组列表（分页）
    /// </summary>
    [HttpGet("list-grouped")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetListGrouped(
        [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 10,
        [FromQuery] string? sorting = null)
    {
        var input = new PagedAndSortedResultRequestDto
        {
            SkipCount = skipCount,
            MaxResultCount = maxResultCount,
            Sorting = sorting
        };
        var result = await _lotteryService.GetListGrouped(input);
        return Success(result);
    }

    /// <summary>
    /// 获取指定类型的最新期号
    /// </summary>
    [HttpGet("latest-index-no")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetLatestIndexNoByType([FromQuery] string lotteryType)
    {
        var result = await _lotteryService.GetLatestIndexNoByType(lotteryType);
        return Success(result);
    }

    /// <summary>
    /// 根据组号删除彩票组
    /// </summary>
    [HttpDelete("group/{groupId:long}")]
    [Permission(DFAppPermissions.Lottery.Delete)]
    public async Task<IActionResult> DeleteLotteryGroup([FromRoute] long groupId)
    {
        await _lotteryService.DeleteLotteryGroup(groupId);
        return Success();
    }

    /// <summary>
    /// 根据期号和组号删除彩票组
    /// </summary>
    [HttpDelete("group/{indexNo:int}/{groupId:long}")]
    [Permission(DFAppPermissions.Lottery.Delete)]
    public async Task<IActionResult> DeleteLotteryGroupByIndexNoAndGroupId(
        [FromRoute] int indexNo,
        [FromRoute] long groupId)
    {
        await _lotteryService.DeleteLotteryGroupByIndexNoAndGroupId(indexNo, groupId);
        return Success();
    }
}
