using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Identity;

/// <summary>
/// 权限分组实体
/// </summary>
[SugarTable("AbpPermissionGroups")]
public class PermissionGroup : Entity
{
    /// <summary>
    /// 分组名称
    /// </summary>
    [SugarColumn(ColumnName = "Name", Length = 256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称
    /// </summary>
    [SugarColumn(ColumnName = "DisplayName", Length = 256)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 扩展属性（JSON 格式）
    /// </summary>
    [SugarColumn(ColumnName = "ExtraProperties", IsNullable = true)]
    public string? ExtraProperties { get; set; }

    /// <summary>
    /// 并发标记（不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public new string ConcurrencyStamp { get; set; } = string.Empty;

    /// <summary>
    /// 构造函数
    /// </summary>
    public PermissionGroup(Guid id, string name, string displayName)
    {
        Id = id;
        Name = name;
        DisplayName = displayName;
    }

    public PermissionGroup()
    {
    }
}
