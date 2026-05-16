using System;
using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 创建审计实体类，只包含创建信息
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public abstract class CreationAuditedEntity<TKey> : EntityBase<TKey>, ICreationAuditedObject
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 创建者 ID
    /// </summary>
    public Guid? CreatorId { get; set; }
}
