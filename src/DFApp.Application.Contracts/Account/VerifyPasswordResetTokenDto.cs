using System.ComponentModel.DataAnnotations;

namespace DFApp.Account;

/// <summary>
/// 验证密码重置令牌请求 DTO
/// </summary>
public class VerifyPasswordResetTokenDto
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
}
