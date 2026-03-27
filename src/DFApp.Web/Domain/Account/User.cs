using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Account;

/// <summary>
/// 用户实体
/// </summary>
[SugarTable("AbpUsers")]
public class User : AuditedEntity<Guid>
{
    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnName = "UserName", Length = 256)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [SugarColumn(ColumnName = "Email", Length = 256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    [SugarColumn(ColumnName = "PasswordHash")]
    public string? PasswordHash { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    [SugarColumn(ColumnName = "IsActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 构造函数
    /// </summary>
    public User(Guid id, string userName, string email) : base(id)
    {
        UserName = userName;
        Email = email;
    }

    public User()
    {
    }
}
