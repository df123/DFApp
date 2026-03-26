using System;
using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 使用 Guid 作为主键的创建审计实体
/// </summary>
[SugarTable]
public abstract class CreationAuditedEntity : CreationAuditedEntity<Guid>
{
}
