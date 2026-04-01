using System.ComponentModel.DataAnnotations;

namespace DFApp.Web.DTOs.Account;

/// <summary>
/// 重置密码请求 DTO
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// 用户名或邮箱
    /// </summary>
    [Required(ErrorMessage = "用户名或邮箱不能为空")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// 重置令牌
    /// </summary>
    [Required(ErrorMessage = "重置令牌不能为空")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 新密码
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 确认新密码
    /// </summary>
    [Required(ErrorMessage = "确认新密码不能为空")]
    [Compare("NewPassword", ErrorMessage = "两次输入的密码不一致")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
