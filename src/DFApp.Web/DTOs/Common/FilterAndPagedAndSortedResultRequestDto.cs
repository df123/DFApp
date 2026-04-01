namespace DFApp.Web.DTOs.Common;

/// <summary>
/// 带过滤条件的分页排序请求 DTO
/// </summary>
public class FilterAndPagedAndSortedResultRequestDto : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 过滤关键词
    /// </summary>
    public string? Filter { get; set; }
}
