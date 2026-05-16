using System;
using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 审计实体类，包含创建和修改信息
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public abstract class AuditedEntity<TKey> : EntityBase<TKey>, IAuditedObject
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 创建者 ID
    /// </summary>
    public Guid? CreatorId { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 最后修改者 ID
    /// </summary>
    public Guid? LastModifierId { get; set; }
}
