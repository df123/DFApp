using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Identity;

/// <summary>
/// 权限授予实体
/// </summary>
[SugarTable("AbpPermissionGrants")]
public class PermissionGrant : AuditedEntity<Guid>
{
    /// <summary>
    /// 租户 ID
    /// </summary>
    [SugarColumn(ColumnName = "TenantId")]
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 提供者名称
    /// </summary>
    [SugarColumn(ColumnName = "ProviderName")]
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// 提供者键
    /// </summary>
    [SugarColumn(ColumnName = "ProviderKey")]
    public string ProviderKey { get; set; } = string.Empty;

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
    /// 构造函数
    /// </summary>
    public PermissionGrant(Guid id, string name, string providerName, string providerKey)
    {
        Id = id;
        Name = name;
        ProviderName = providerName;
        ProviderKey = providerKey;
    }

    public PermissionGrant()
    {
    }
}
