using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 基础实体类，包含 Id 和 ConcurrencyStamp
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public abstract class EntityBase<TKey> : IEntity<TKey>
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    // 移除 IsIdentity：Guid 类型主键不支持数据库自增，SQLite TEXT 主键也无法自动生成值，
    // 保留 IsIdentity 会导致 INSERT 时 SqlSugar 不传递 Id，引发 NOT NULL 约束失败
    [SugarColumn(IsPrimaryKey = true)]
    public TKey Id { get; set; }

    /// <summary>
    /// 并发标记，用于乐观并发控制
    /// </summary>
    [SugarColumn(Length = 128)]
    public string ConcurrencyStamp { get; set; }
}
