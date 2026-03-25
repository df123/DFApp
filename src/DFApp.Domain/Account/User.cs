using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Account;

/// <summary>
/// 用户实体
/// </summary>
public class User : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public User(Guid id, string userName, string email) : base(id)
    {
        UserName = userName;
        Email = email;
    }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    public User()
    {
    }
}
