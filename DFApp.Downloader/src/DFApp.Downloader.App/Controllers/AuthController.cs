using DFApp.Downloader.Core.Security;
using Microsoft.AspNetCore.Mvc;

namespace DFApp.Downloader.App.Controllers;

/// <summary>
/// 登录认证接口：登录后通过 HttpOnly Cookie 携带会话
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    /// <summary>会话 Cookie 名</summary>
    public const string SessionCookieName = "downloader_session";

    private readonly AccountService _accountService;

    public AuthController(AccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>登录</summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = _accountService.Login(request.Username ?? string.Empty, request.Password ?? string.Empty);
        if (token is null)
        {
            return Unauthorized(new { message = "用户名或密码错误（连续失败会临时锁定）" });
        }

        Response.Cookies.Append(SessionCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/",
            MaxAge = TimeSpan.FromDays(7),
        });
        return Ok(new { username = request.Username });
    }

    /// <summary>注销</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _accountService.Logout(Request.Cookies[SessionCookieName]);
        Response.Cookies.Delete(SessionCookieName, new CookieOptions { Path = "/" });
        return Ok(new { message = "已注销" });
    }

    /// <summary>会话状态探测（前端路由守卫使用）</summary>
    [HttpGet("session")]
    public IActionResult GetSession()
    {
        if (_accountService.ValidateSession(Request.Cookies[SessionCookieName]))
        {
            return Ok(new { authenticated = true });
        }

        return Unauthorized(new { authenticated = false });
    }

    /// <summary>修改当前登录密码（改密后所有会话作废，需重新登录）</summary>
    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!_accountService.ValidateSession(Request.Cookies[SessionCookieName]))
        {
            return Unauthorized(new { message = "未登录" });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 4)
        {
            return BadRequest(new { message = "新密码长度至少 4 位" });
        }

        if (!_accountService.ChangePassword(AccountService.AdminUsername, request.OldPassword ?? string.Empty, request.NewPassword))
        {
            return BadRequest(new { message = "旧密码不正确" });
        }

        Response.Cookies.Delete(SessionCookieName, new CookieOptions { Path = "/" });
        return Ok(new { message = "密码已修改，请重新登录" });
    }
}

public record LoginRequest(string? Username, string? Password);

public record ChangePasswordRequest(string? OldPassword, string? NewPassword);
