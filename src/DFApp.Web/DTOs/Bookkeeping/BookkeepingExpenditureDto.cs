using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Bookkeeping;

/// <summary>
/// 记账支出 DTO
/// </summary>
public class BookkeepingExpenditureDto : AuditedEntityDto<long>
{
    public DateTime ExpenditureDate { get; set; }
    public decimal Expenditure { get; set; }
    public string? Remark { get; set; }
    public bool IsBelongToSelf { get; set; }
    public BookkeepingCategoryDto Category { get; set; } = new BookkeepingCategoryDto();
    public long CategoryId { get; set; }
}
