using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompoundLotteryService = DFApp.Web.Services.Lottery.CompoundLotteryService;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Controllers;

/// <summary>
/// 复式投注控制器，提供复式投注组合计算功能
/// </summary>
[ApiController]
[Route("api/app/compound-lottery")]
[Authorize]
public class CompoundLotteryController : DFAppControllerBase
{
    private readonly CompoundLotteryService _compoundLotteryService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="compoundLotteryService">复式投注服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public CompoundLotteryController(
        CompoundLotteryService compoundLotteryService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _compoundLotteryService = compoundLotteryService;
    }

    /// <summary>
    /// 计算复式投注组合
    /// </summary>
    [HttpPost("calculate")]
    [Permission(DFAppPermissions.Lottery.Create)]
    public async Task<IActionResult> CalculateCompoundCombination([FromBody] CompoundLotteryInputDto input)
    {
        var result = await _compoundLotteryService.CalculateCompoundCombination(input);
        return Success(result);
    }
}
