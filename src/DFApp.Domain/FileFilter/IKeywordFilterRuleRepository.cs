using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DFApp.FileFilter
{
    public interface IKeywordFilterRuleRepository : IRepository<KeywordFilterRule, long>
    {
        /// <summary>
        /// 获取所有启用的过滤规则（按优先级排序）
        /// </summary>
        Task<List<KeywordFilterRule>> GetAllEnabledRulesAsync();

        /// <summary>
        /// 根据过滤类型获取启用的规则
        /// </summary>
        Task<List<KeywordFilterRule>> GetEnabledRulesByTypeAsync(FilterType filterType);

        /// <summary>
        /// 检查文件名是否匹配任何规则
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>true表示文件应被过滤（根据规则类型）</returns>
        Task<bool> ShouldFilterFileAsync(string fileName);

        /// <summary>
        /// 批量检查多个文件名
        /// </summary>
        Task<Dictionary<string, bool>> ShouldFilterFilesAsync(IEnumerable<string> fileNames);
    }
}