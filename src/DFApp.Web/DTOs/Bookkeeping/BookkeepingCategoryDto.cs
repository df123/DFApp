using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Bookkeeping;

/// <summary>
/// 记账分类 DTO
/// </summary>
public class BookkeepingCategoryDto : AuditedEntityDto<long>
{
    public string Category { get; set; } = null!;
}
