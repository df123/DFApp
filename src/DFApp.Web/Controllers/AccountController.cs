using System.Threading.Tasks;
using DFApp.Web.DTOs.Account;
using DFApp.Web.Services.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// 账户控制器，提供登录和密码重置等接口（登录和密码重置接口不需要授权）
/// </summary>
[ApiController]
[Route("api/app/account")]
public class AccountController : DFAppControllerBase
{
    private readonly AccountAppService _accountAppService;

    public AccountController(
        AccountAppService accountAppService,
        DFApp.Web.Infrastructure.ICurrentUser currentUser,
        DFApp.Web.Permissions.IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _accountAppService = accountAppService;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto input)
    {
        var result = await _accountAppService.LoginAsync(input);
        return Success(result);
    }

    /// <summary>
    /// 发送密码重置码
    /// </summary>
    [HttpPost("send-password-reset-code")]
    [AllowAnonymous]
    public async Task<IActionResult> SendPasswordResetCodeAsync([FromBody] SendPasswordResetCodeDto input)
    {
        await _accountAppService.SendPasswordResetCodeAsync(input);
        return Success();
    }

    /// <summary>
    /// 验证密码重置令牌
    /// </summary>
    [HttpPost("verify-password-reset-token")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyPasswordResetTokenAsync([FromBody] VerifyPasswordResetTokenDto input)
    {
        var result = await _accountAppService.VerifyPasswordResetTokenAsync(input);
        return Success(data: result);
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto input)
    {
        var result = await _accountAppService.ResetPasswordAsync(input);
        return Success(data: result);
    }
}
