using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Lottery;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Lottery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateUpdateLotteryResultDto = DFApp.Web.DTOs.Lottery.CreateUpdateLotteryResultDto;
using LotteryResultDto = DFApp.Web.DTOs.Lottery.LotteryResultDto;

namespace DFApp.Web.Controllers;

/// <summary>
/// 彩票结果控制器，提供彩票开奖结果的增删改查功能
/// </summary>
[ApiController]
[Route("api/app/lottery-result")]
[Authorize]
public class LotteryResultController : DFAppControllerBase
{
    private readonly LotteryResultService _lotteryResultService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="lotteryResultService">彩票结果服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public LotteryResultController(
        LotteryResultService lotteryResultService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _lotteryResultService = lotteryResultService;
    }

    /// <summary>
    /// 获取彩票结果列表
    /// </summary>
    [HttpGet]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetListAsync()
    {
        var result = await _lotteryResultService.GetListAsync();
        return Success(result);
    }

    /// <summary>
    /// 根据ID获取彩票结果详情
    /// </summary>
    [HttpGet("{id:long}")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetAsync([FromRoute] long id)
    {
        var result = await _lotteryResultService.GetAsync(id);
        return Success(result);
    }

    /// <summary>
    /// 分页获取彩票结果列表
    /// </summary>
    [HttpGet("paged")]
    [Permission(DFAppPermissions.Lottery.Default)]
    public async Task<IActionResult> GetPagedListAsync([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _lotteryResultService.GetPagedListAsync(pageIndex, pageSize);
        return Success(new { Items = items, TotalCount = totalCount });
    }

    /// <summary>
    /// 创建彩票结果
    /// </summary>
    [HttpPost]
    [Permission(DFAppPermissions.Lottery.Create)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUpdateLotteryResultDto input)
    {
        var result = await _lotteryResultService.CreateAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 更新彩票结果
    /// </summary>
    [HttpPut("{id:long}")]
    [Permission(DFAppPermissions.Lottery.Edit)]
    public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] CreateUpdateLotteryResultDto input)
    {
        var result = await _lotteryResultService.UpdateAsync(id, input);
        return Success(result);
    }

    /// <summary>
    /// 删除彩票结果
    /// </summary>
    [HttpDelete("{id:long}")]
    [Permission(DFAppPermissions.Lottery.Delete)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id)
    {
        await _lotteryResultService.DeleteAsync(id);
        return Success();
    }
}
