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
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public TKey Id { get; set; }

    /// <summary>
    /// 并发标记，用于乐观并发控制
    /// </summary>
    [SugarColumn(Length = 128)]
    public string ConcurrencyStamp { get; set; }
}
