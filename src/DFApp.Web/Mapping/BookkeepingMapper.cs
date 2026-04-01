using DFApp.Bookkeeping;
using DFApp.Web.DTOs.Bookkeeping;
using Riok.Mapperly.Abstractions;

namespace DFApp.Web.Mapping;

/// <summary>
/// 记账模块映射器
/// </summary>
[Mapper]
public partial class BookkeepingMapper
{
    /// <summary>
    /// BookkeepingCategory → BookkeepingCategoryDto
    /// </summary>
    public partial BookkeepingCategoryDto MapToDto(BookkeepingCategory entity);

    /// <summary>
    /// BookkeepingCategoryDto → BookkeepingCategory（双向映射的反向）
    /// </summary>
    public partial BookkeepingCategory MapToEntity(BookkeepingCategoryDto dto);

    /// <summary>
    /// CreateUpdateBookkeepingCategoryDto → BookkeepingCategory
    /// </summary>
    [MapperIgnoreTarget(nameof(BookkeepingCategory.ConcurrencyStamp))]
    public partial BookkeepingCategory MapToEntity(CreateUpdateBookkeepingCategoryDto dto);

    /// <summary>
    /// BookkeepingCategory → BookkeepingCategoryLookupDto（Id 映射为 CategoryId）
    /// </summary>
    [MapProperty(nameof(BookkeepingCategory.Id), nameof(BookkeepingCategoryLookupDto.CategoryId))]
    public partial BookkeepingCategoryLookupDto MapToLookupDto(BookkeepingCategory entity);

    /// <summary>
    /// BookkeepingExpenditure → BookkeepingExpenditureDto
    /// </summary>
    public partial BookkeepingExpenditureDto MapToExpenditureDto(BookkeepingExpenditure entity);

    /// <summary>
    /// BookkeepingExpenditureDto → BookkeepingExpenditure（双向映射的反向）
    /// </summary>
    public partial BookkeepingExpenditure MapToEntity(BookkeepingExpenditureDto dto);

    /// <summary>
    /// CreateUpdateBookkeepingExpenditureDto → BookkeepingExpenditure
    /// </summary>
    [MapperIgnoreTarget(nameof(BookkeepingExpenditure.ConcurrencyStamp))]
    public partial BookkeepingExpenditure MapToEntity(CreateUpdateBookkeepingExpenditureDto dto);
}
