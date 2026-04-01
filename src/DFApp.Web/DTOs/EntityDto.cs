namespace DFApp.Web.DTOs;

/// <summary>
/// 实体 DTO 基类，替代 ABP 的 EntityDto{TKey}
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public class EntityDto<TKey>
{
    public TKey Id { get; set; } = default!;
}
