using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Identity;

/// <summary>
/// 权限定义实体
/// </summary>
[SugarTable("AbpPermissions")]
public class Permission : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// 分组名称
    /// </summary>
    [SugarColumn(ColumnName = "GroupName")]
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 权限名称
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父权限名称
    /// </summary>
    [SugarColumn(ColumnName = "ParentName", IsNullable = true)]
    public string? ParentName { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [SugarColumn(ColumnName = "DisplayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnName = "IsEnabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 多租户侧
    /// </summary>
    [SugarColumn(ColumnName = "MultiTenancySide")]
    public int MultiTenancySide { get; set; }

    /// <summary>
    /// 提供者
    /// </summary>
    [SugarColumn(ColumnName = "Providers", IsNullable = true)]
    public string? Providers { get; set; }

    /// <summary>
    /// 状态检查器
    /// </summary>
    [SugarColumn(ColumnName = "StateCheckers", IsNullable = true)]
    public string? StateCheckers { get; set; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    [SugarColumn(ColumnName = "ExtraProperties", IsNullable = true)]
    public string? ExtraProperties { get; set; }

    /// <summary>
    /// 租户 ID（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public Guid? TenantId { get; set; }

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
    /// 构造函数
    /// </summary>
    public Permission(Guid id, string groupName, string name, string displayName)
    {
        Id = id;
        GroupName = groupName;
        Name = name;
        DisplayName = displayName;
    }

    public Permission()
    {
    }
}
