using DFApp.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DFApp.FileFilter
{
    public class EfCoreKeywordFilterRuleRepository : EfCoreRepository<DFAppDbContext, KeywordFilterRule, long>, IKeywordFilterRuleRepository
    {
        public EfCoreKeywordFilterRuleRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<KeywordFilterRule>> GetAllEnabledRulesAsync()
        {
            var dbSet = await GetDbSetAsync();
            return dbSet
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.Id)
                .ToList();
        }

        public async Task<List<KeywordFilterRule>> GetEnabledRulesByTypeAsync(FilterType filterType)
        {
            var dbSet = await GetDbSetAsync();
            return dbSet
                .Where(x => x.IsEnabled && x.FilterType == filterType)
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.Id)
                .ToList();
        }

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