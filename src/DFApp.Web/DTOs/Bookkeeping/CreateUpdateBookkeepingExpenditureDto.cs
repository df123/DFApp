using System;

namespace DFApp.Web.DTOs.Bookkeeping;

/// <summary>
/// 创建/更新记账支出 DTO
/// </summary>
public class CreateUpdateBookkeepingExpenditureDto
{
    public DateTime ExpenditureDate { get; set; }
    public decimal Expenditure { get; set; }
    public string? Remark { get; set; }
    public bool IsBelongToSelf { get; set; }
    public long CategoryId { get; set; }
}
