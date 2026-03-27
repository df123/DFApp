using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Identity;

/// <summary>
/// 角色声明实体
/// </summary>
[SugarTable("AbpRoleClaims")]
public class RoleClaim : AuditedEntity<Guid>
{
    /// <summary>
    /// 角色 ID
    /// </summary>
    [SugarColumn(ColumnName = "RoleId")]
    public Guid RoleId { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    [SugarColumn(ColumnName = "TenantId")]
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 声明类型
    /// </summary>
    [SugarColumn(ColumnName = "ClaimType")]
    public string ClaimType { get; set; } = string.Empty;

    /// <summary>
    /// 声明值
    /// </summary>
    [SugarColumn(ColumnName = "ClaimValue")]
    public string? ClaimValue { get; set; }

    /// <summary>
    /// 并发标记（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public new string ConcurrencyStamp { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public new DateTime CreationTime { get; set; }

    /// <summary>
    /// 创建者 ID（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public new Guid? CreatorId { get; set; }

    /// <summary>
    /// 最后修改时间（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public new DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 最后修改者 ID（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public new Guid? LastModifierId { get; set; }

    /// <summary>
    /// 角色导航属性（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public Role? Role { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public RoleClaim(Guid id, Guid roleId, string claimType)
    {
        Id = id;
        RoleId = roleId;
        ClaimType = claimType;
    }

    public RoleClaim()
    {
    }
}
