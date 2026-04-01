using System;

namespace DFApp.Web.DTOs;

/// <summary>
/// 审计实体 DTO 基类，替代 ABP 的 AuditedEntityDto{TKey}
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public class AuditedEntityDto<TKey> : EntityDto<TKey>
{
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
