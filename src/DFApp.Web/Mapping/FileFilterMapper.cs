using DFApp.FileFilter;
using DFApp.Web.DTOs.FileFilter;
using Riok.Mapperly.Abstractions;

namespace DFApp.Web.Mapping;

/// <summary>
/// 文件过滤映射器
/// </summary>
[Mapper]
public partial class FileFilterMapper
{
    /// <summary>
    /// KeywordFilterRule → KeywordFilterRuleDto
    /// </summary>
    public partial KeywordFilterRuleDto MapToDto(KeywordFilterRule entity);

    /// <summary>
    /// CreateUpdateKeywordFilterRuleDto → KeywordFilterRule
    /// </summary>
    [MapperIgnoreTarget(nameof(KeywordFilterRule.ConcurrencyStamp))]
    public partial KeywordFilterRule MapToEntity(CreateUpdateKeywordFilterRuleDto dto);
}
