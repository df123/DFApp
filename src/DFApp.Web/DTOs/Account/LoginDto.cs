using System.ComponentModel.DataAnnotations;

namespace DFApp.Web.DTOs.Account;

/// <summary>
/// 登录请求 DTO
/// </summary>
public class LoginDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
