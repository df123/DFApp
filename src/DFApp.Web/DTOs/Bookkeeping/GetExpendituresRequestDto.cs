using DFApp.Web.DTOs.Common;

namespace DFApp.Web.DTOs.Bookkeeping;

/// <summary>
/// 获取支出列表请求 DTO
/// </summary>
public class GetExpendituresRequestDto : FilterAndPagedAndSortedResultRequestDto
{
    public long? CategoryId { get; set; }

    public bool? IsBelongToSelf { get; set; }
}
