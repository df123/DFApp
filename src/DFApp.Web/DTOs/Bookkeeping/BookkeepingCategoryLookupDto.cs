namespace DFApp.Web.DTOs.Bookkeeping;

/// <summary>
/// 记账分类查找 DTO
/// </summary>
public class BookkeepingCategoryLookupDto
{
    /// <summary>
    /// 分类 ID
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// 分类名称
    /// </summary>
    public string Category { get; set; } = null!;
}
