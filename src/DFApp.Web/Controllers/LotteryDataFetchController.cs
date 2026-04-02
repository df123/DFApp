using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LotteryDataFetchService = DFApp.Web.Services.Lottery.LotteryDataFetchService;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 彩票数据获取控制器，提供从代理服务器获取彩票开奖数据的功能
/// </summary>
[ApiController]
[Route("api/app/lottery-data-fetch")]
[Authorize]
public class LotteryDataFetchController : DFAppControllerBase
{
    private readonly LotteryDataFetchService _lotteryDataFetchService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="lotteryDataFetchService">彩票数据获取服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public LotteryDataFetchController(
        LotteryDataFetchService lotteryDataFetchService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _lotteryDataFetchService = lotteryDataFetchService;
    }

    /// <summary>
    /// 获取彩票数据
    /// </summary>
    [HttpPost("fetch")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> FetchLotteryData([FromBody] LotteryDataFetchRequestDto input)
    {
        var result = await _lotteryDataFetchService.FetchLotteryData(input);
        return Success(result);
    }

    /// <summary>
    /// 获取双色球最新数据
    /// </summary>
    [HttpPost("fetch-ssq")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> FetchSSQLatestData()
    {
        var result = await _lotteryDataFetchService.FetchSSQLatestData();
        return Success(result);
    }

    /// <summary>
    /// 获取快乐8最新数据
    /// </summary>
    [HttpPost("fetch-kl8")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> FetchKL8LatestData()
    {
        var result = await _lotteryDataFetchService.FetchKL8LatestData();
        return Success(result);
    }

    /// <summary>
    /// 测试彩票API连接
    /// </summary>
    [HttpPost("test-connection")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> TestLotteryApiConnection([FromQuery] string lotteryType = "ssq")
    {
        var result = await _lotteryDataFetchService.TestLotteryApiConnection(lotteryType);
        return Success(result);
    }
}
