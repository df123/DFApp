using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Identity;

/// <summary>
/// 角色实体
/// </summary>
[SugarTable("AbpRoles")]
public class Role : AuditedEntity<Guid>
{
    /// <summary>
    /// 租户 ID
    /// </summary>
    [SugarColumn(ColumnName = "TenantId")]
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    [SugarColumn(ColumnName = "Name", Length = 256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 规范化角色名称
    /// </summary>
    [SugarColumn(ColumnName = "NormalizedName", Length = 256)]
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认角色
    /// </summary>
    [SugarColumn(ColumnName = "IsDefault")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否为静态角色（不可删除）
    /// </summary>
    [SugarColumn(ColumnName = "IsStatic")]
    public bool IsStatic { get; set; }

    /// <summary>
    /// 是否为公共角色
    /// </summary>
    [SugarColumn(ColumnName = "IsPublic")]
    public bool IsPublic { get; set; }

    /// <summary>
    /// 实体版本
    /// </summary>
    [SugarColumn(ColumnName = "EntityVersion")]
    public int EntityVersion { get; set; }

    /// <summary>
    /// 扩展属性（JSON 格式）
    /// </summary>
    [SugarColumn(ColumnName = "ExtraProperties")]
    public string ExtraProperties { get; set; } = string.Empty;

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
    /// 构造函数
    /// </summary>
    public Role(Guid id, string name, string normalizedName)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
    }

    public Role()
    {
    }
}
