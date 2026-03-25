using System.ComponentModel.DataAnnotations;

namespace DFApp.Account;

/// <summary>
/// 发送密码重置码请求 DTO
/// </summary>
public class SendPasswordResetCodeDto
{
    /// <summary>
    /// 用户名或邮箱
    /// </summary>
    [Required(ErrorMessage = "用户名或邮箱不能为空")]
    public string UserNameOrEmail { get; set; } = string.Empty;
}
