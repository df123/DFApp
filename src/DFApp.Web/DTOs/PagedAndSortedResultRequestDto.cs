namespace DFApp.Web.DTOs;

/// <summary>
/// 分页排序请求 DTO 基类，替代 ABP 的 PagedAndSortedResultRequestDto
/// </summary>
public class PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 跳过数量（ABP 默认 (Page - 1) * MaxResultCount）
    /// </summary>
    public int SkipCount { get; set; } = 0;

    /// <summary>
    /// 每页数量
    /// </summary>
    public int MaxResultCount { get; set; } = 10;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? Sorting { get; set; }
}
