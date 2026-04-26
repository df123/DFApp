using System;
using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 使用 Guid 作为主键的创建审计实体
/// </summary>
[SugarTable]
public abstract class CreationAuditedEntity : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// Guid 类型主键不支持数据库自增，覆盖基类属性移除 IsIdentity
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    public new Guid Id { get; set; }
}
