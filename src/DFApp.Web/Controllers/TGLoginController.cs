using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using DFApp.Web.Services.TG;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Web.Controllers;

/// <summary>
/// Telegram 登录管理控制器
/// </summary>
[ApiController]
[Route("api/app/tg-login")]
[Authorize]
public class TGLoginController : DFAppControllerBase
{
    private readonly TGLoginService _tgLoginService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="tgLoginService">TG 登录服务</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    public TGLoginController(
        TGLoginService tgLoginService,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
        : base(currentUser, permissionChecker)
    {
        _tgLoginService = tgLoginService;
    }

    /// <summary>
    /// 获取 TG 登录状态
    /// </summary>
    [HttpGet("status")]
    public IActionResult Status()
    {
        var result = _tgLoginService.Status();
        return Success(result);
    }

    /// <summary>
    /// 设置 TG 登录配置
    /// </summary>
    /// <param name="value">配置值</param>
    [HttpPost("config")]
    public async Task<IActionResult> Config([FromBody] string value)
    {
        var result = await _tgLoginService.Config(value);
        return Success(result);
    }

    /// <summary>
    /// 获取 TG 聊天列表
    /// </summary>
    [HttpGet("chats")]
    public async Task<IActionResult> Chats()
    {
        var result = await _tgLoginService.Chats();
        return Success(result);
    }
}
