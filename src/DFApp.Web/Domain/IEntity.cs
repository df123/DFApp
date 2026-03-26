namespace DFApp.Web.Domain;

/// <summary>
/// 实体接口，定义实体的基本标识
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IEntity<TKey>
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    TKey Id { get; set; }
}
