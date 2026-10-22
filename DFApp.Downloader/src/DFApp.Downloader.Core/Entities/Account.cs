using SqlSugar;

namespace DFApp.Downloader.Core.Entities;

/// <summary>
/// 管理界面登录账号（当前仅使用单个 admin 账号）
/// </summary>
[SugarTable("Accounts")]
public class Account
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>用户名（唯一）</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>PBKDF2 口令哈希（Base64）</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>PBKDF2 盐（Base64）</summary>
    public string PasswordSalt { get; set; } = string.Empty;

    /// <summary>哈希迭代次数</summary>
    public int Iterations { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
