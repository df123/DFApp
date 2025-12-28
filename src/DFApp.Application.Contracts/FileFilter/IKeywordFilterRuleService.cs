using DFApp.CommonDtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.FileFilter
{
    public interface IKeywordFilterRuleService : ICrudAppService<
        KeywordFilterRuleDto,
        long,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateKeywordFilterRuleDto,
        CreateUpdateKeywordFilterRuleDto>
    {
        /// <summary>
        /// 测试文件名是否会被过滤
        /// </summary>
        Task<KeywordFilterTestResultDto> TestFilterAsync(TestFilterRequestDto input);

        /// <summary>
        /// 批量测试文件名过滤
        /// </summary>
        Task<List<KeywordFilterTestResultDto>> TestFilterBatchAsync(List<string> fileNames);

        /// <summary>
        /// 获取所有匹配的规则（用于调试）
        /// </summary>
        Task<List<KeywordFilterMatchResultDto>> GetMatchingRulesAsync(string fileName);

        /// <summary>
        /// 启用/禁用规则
        /// </summary>
        Task ToggleRuleAsync(long id, bool isEnabled);
    }
}