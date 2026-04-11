using System.Collections.Generic;

namespace DFApp.Web.DTOs.Account;

/// <summary>
/// 登录结果 DTO
/// </summary>
public class LoginResultDto
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（Unix 时间戳）
    /// </summary>
    public long ExpiresAt { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 用户角色列表
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// 用户权限列表
    /// </summary>
    public List<string> Permissions { get; set; } = new();
}
