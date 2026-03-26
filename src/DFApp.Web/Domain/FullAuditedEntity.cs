using System;
using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 完整审计实体类，包含创建、修改和删除信息
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>, IFullAuditedObject
{
    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }

    /// <summary>
    /// 删除者 ID
    /// </summary>
    public Guid? DeleterId { get; set; }
}
