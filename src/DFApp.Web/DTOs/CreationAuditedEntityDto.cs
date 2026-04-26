using System;

namespace DFApp.Web.DTOs;

/// <summary>
/// 创建审计实体 DTO 基类，替代 ABP 的 CreationAuditedEntityDto{TKey}
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public class CreationAuditedEntityDto<TKey> : EntityDto<TKey>
{
    public DateTime CreationTime { get; set; }
}
