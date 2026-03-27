using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SqlSugar;
using DFApp.Web.Data;

namespace DFApp.FileFilter
{
    /// <summary>
    /// 关键词过滤规则仓储实现
    /// </summary>
    public class KeywordFilterRuleRepository : SqlSugarRepository<KeywordFilterRule, long>, IKeywordFilterRuleRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db">SqlSugar 客户端</param>
        public KeywordFilterRuleRepository(ISqlSugarClient db) : base(db)
        {
        }

        /// <summary>
        /// 获取所有启用的过滤规则（按优先级排序）
        /// </summary>
        public async Task<List<KeywordFilterRule>> GetAllEnabledRulesAsync()
        {
            return await GetQueryable()
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Priority)
                .OrderBy(x => x.Id, OrderByType.Asc)
                .ToListAsync();
        }

        /// <summary>
        /// 根据过滤类型获取启用的规则
        /// </summary>
        public async Task<List<KeywordFilterRule>> GetEnabledRulesByTypeAsync(FilterType filterType)
        {
            return await GetQueryable()
                .Where(x => x.IsEnabled && x.FilterType == filterType)
                .OrderBy(x => x.Priority)
                .OrderBy(x => x.Id, OrderByType.Asc)
                .ToListAsync();
        }

        /// <summary>
        /// 检查文件名是否匹配任何规则
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>true表示文件应被过滤（根据规则类型）</returns>
        public async Task<bool> ShouldFilterFileAsync(string fileName)
        {
            var rules = await GetAllEnabledRulesAsync();
            if (rules.Count == 0)
            {
                return false; // 没有规则，不过滤
            }

            // 按优先级排序
            var sortedRules = rules.OrderBy(x => x.Priority).ThenBy(x => x.Id).ToList();

            // 检查每个规则
            foreach (var rule in sortedRules)
            {
                if (IsMatch(fileName, rule))
                {
                    // 匹配到规则，根据规则类型决定是否过滤
                    return rule.FilterType == FilterType.Blacklist;
                }
            }

            // 没有匹配到任何规则
            // 如果有白名单规则但没有匹配，则过滤掉（白名单模式：只有匹配到的才保留）
            var hasWhitelist = rules.Any(x => x.FilterType == FilterType.Whitelist);
            return hasWhitelist; // 有白名单规则但没匹配到 => 过滤
        }

        /// <summary>
        /// 批量检查多个文件名
        /// </summary>
        public async Task<Dictionary<string, bool>> ShouldFilterFilesAsync(IEnumerable<string> fileNames)
        {
            var rules = await GetAllEnabledRulesAsync();
            if (rules.Count == 0)
            {
                // 没有规则，全部不过滤
                return fileNames.ToDictionary(x => x, x => false);
            }

            var sortedRules = rules.OrderBy(x => x.Priority).ThenBy(x => x.Id).ToList();
            var hasWhitelist = rules.Any(x => x.FilterType == FilterType.Whitelist);
            var result = new Dictionary<string, bool>();

            foreach (var fileName in fileNames)
            {
                bool matched = false;
                bool shouldFilter = false;

                foreach (var rule in sortedRules)
                {
                    if (IsMatch(fileName, rule))
                    {
                        matched = true;
                        shouldFilter = rule.FilterType == FilterType.Blacklist;
                        break; // 找到匹配规则，停止检查
                    }
                }

                if (!matched)
                {
                    // 没有匹配到任何规则
                    shouldFilter = hasWhitelist; // 白名单模式但没匹配 => 过滤
                }

                result[fileName] = shouldFilter;
            }

            return result;
        }

        /// <summary>
        /// 判断文件名是否匹配规则
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="rule">过滤规则</param>
        /// <returns>是否匹配</returns>
        private bool IsMatch(string fileName, KeywordFilterRule rule)
        {
            var textToMatch = rule.IsCaseSensitive ? fileName : fileName.ToLowerInvariant();
            var keyword = rule.IsCaseSensitive ? rule.Keyword : rule.Keyword.ToLowerInvariant();

            switch (rule.MatchMode)
            {
                case MatchMode.Contains:
                    return textToMatch.Contains(keyword);

                case MatchMode.StartsWith:
                    return textToMatch.StartsWith(keyword);

                case MatchMode.EndsWith:
                    return textToMatch.EndsWith(keyword);

                case MatchMode.Exact:
                    return textToMatch.Equals(keyword);

                case MatchMode.Regex:
                    try
                    {
                        var regexOptions = rule.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                        return Regex.IsMatch(fileName, rule.Keyword, regexOptions);
                    }
                    catch (ArgumentException)
                    {
                        // 正则表达式无效，视为不匹配
                        return false;
                    }

                default:
                    return false;
            }
        }
    }
}
