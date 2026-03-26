using System;
using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 使用 Guid 作为主键的完整审计实体
/// </summary>
[SugarTable]
public abstract class FullAuditedEntity : FullAuditedEntity<Guid>
{
}
