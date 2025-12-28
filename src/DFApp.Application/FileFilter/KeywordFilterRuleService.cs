using DFApp.CommonDtos;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.FileFilter
{
    [Authorize(DFAppPermissions.FileFilter.Default)]
    public class KeywordFilterRuleService : CrudAppService<
        KeywordFilterRule,
        KeywordFilterRuleDto,
        long,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateKeywordFilterRuleDto,
        CreateUpdateKeywordFilterRuleDto>,
        IKeywordFilterRuleService
    {
        private readonly IKeywordFilterRuleRepository _keywordFilterRuleRepository;

        public KeywordFilterRuleService(
            IRepository<KeywordFilterRule, long> repository,
            IKeywordFilterRuleRepository keywordFilterRuleRepository)
            : base(repository)
        {
            _keywordFilterRuleRepository = keywordFilterRuleRepository;

            GetPolicyName = DFAppPermissions.FileFilter.Default;
            GetListPolicyName = DFAppPermissions.FileFilter.Default;
            CreatePolicyName = DFAppPermissions.FileFilter.Create;
            UpdatePolicyName = DFAppPermissions.FileFilter.Edit;
            DeletePolicyName = DFAppPermissions.FileFilter.Delete;
        }

        public async Task<KeywordFilterTestResultDto> TestFilterAsync(TestFilterRequestDto input)
        {
            if (string.IsNullOrWhiteSpace(input.FileName))
            {
                throw new UserFriendlyException("文件名不能为空");
            }

            var rules = await _keywordFilterRuleRepository.GetAllEnabledRulesAsync();
            var result = new KeywordFilterTestResultDto
            {
                FileName = input.FileName,
                MatchingRules = new List<KeywordFilterMatchResultDto>()
            };

            if (rules.Count == 0)
            {
                result.ShouldFilter = false;
                result.Reason = "没有启用任何过滤规则";
                return result;
            }

            var sortedRules = rules.OrderBy(x => x.Priority).ThenBy(x => x.Id).ToList();
            var hasWhitelist = rules.Any(x => x.FilterType == FilterType.Whitelist);

            bool matched = false;
            bool shouldFilter = false;

            foreach (var rule in sortedRules)
            {
                var matchResult = TestRuleMatch(input.FileName, rule);
                if (matchResult != null)
                {
                    result.MatchingRules.Add(matchResult);
                    matched = true;
                    shouldFilter = rule.FilterType == FilterType.Blacklist;
                    break; // 找到匹配规则，停止检查
                }
            }

            if (!matched)
            {
                // 没有匹配到任何规则
                shouldFilter = hasWhitelist;
                result.Reason = hasWhitelist
                    ? "白名单模式：没有匹配到任何白名单规则"
                    : "黑名单模式：没有匹配到任何黑名单规则";
            }
            else
            {
                var matchedRule = result.MatchingRules.First();
                result.Reason = matchedRule.FilterType == FilterType.Blacklist
                    ? $"匹配到黑名单规则（ID: {matchedRule.RuleId}）"
                    : $"匹配到白名单规则（ID: {matchedRule.RuleId}）";
            }

            result.ShouldFilter = shouldFilter;
            return result;
        }

        public async Task<List<KeywordFilterTestResultDto>> TestFilterBatchAsync(List<string> fileNames)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                throw new UserFriendlyException("文件名列表不能为空");
            }

            var rules = await _keywordFilterRuleRepository.GetAllEnabledRulesAsync();
            var results = new List<KeywordFilterTestResultDto>();

            if (rules.Count == 0)
            {
                // 没有规则，全部不过滤
                foreach (var fileName in fileNames)
                {
                    results.Add(new KeywordFilterTestResultDto
                    {
                        FileName = fileName,
                        ShouldFilter = false,
                        Reason = "没有启用任何过滤规则"
                    });
                }
                return results;
            }

            var sortedRules = rules.OrderBy(x => x.Priority).ThenBy(x => x.Id).ToList();
            var hasWhitelist = rules.Any(x => x.FilterType == FilterType.Whitelist);

            foreach (var fileName in fileNames)
            {
                var result = new KeywordFilterTestResultDto
                {
                    FileName = fileName,
                    MatchingRules = new List<KeywordFilterMatchResultDto>()
                };

                bool matched = false;
                bool shouldFilter = false;

                foreach (var rule in sortedRules)
                {
                    var matchResult = TestRuleMatch(fileName, rule);
                    if (matchResult != null)
                    {
                        result.MatchingRules.Add(matchResult);
                        matched = true;
                        shouldFilter = rule.FilterType == FilterType.Blacklist;
                        break;
                    }
                }

                if (!matched)
                {
                    shouldFilter = hasWhitelist;
                    result.Reason = hasWhitelist
                        ? "白名单模式：没有匹配到任何白名单规则"
                        : "黑名单模式：没有匹配到任何黑名单规则";
                }
                else
                {
                    var matchedRule = result.MatchingRules.First();
                    result.Reason = matchedRule.FilterType == FilterType.Blacklist
                        ? $"匹配到黑名单规则（ID: {matchedRule.RuleId}）"
                        : $"匹配到白名单规则（ID: {matchedRule.RuleId}）";
                }

                result.ShouldFilter = shouldFilter;
                results.Add(result);
            }

            return results;
        }

        public async Task<List<KeywordFilterMatchResultDto>> GetMatchingRulesAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new UserFriendlyException("文件名不能为空");
            }

            var rules = await _keywordFilterRuleRepository.GetAllEnabledRulesAsync();
            var matchingRules = new List<KeywordFilterMatchResultDto>();

            foreach (var rule in rules)
            {
                var matchResult = TestRuleMatch(fileName, rule);
                if (matchResult != null)
                {
                    matchingRules.Add(matchResult);
                }
            }

            return matchingRules.OrderBy(x => x.Priority).ToList();
        }

        public async Task ToggleRuleAsync(long id, bool isEnabled)
        {
            var rule = await Repository.GetAsync(id);
            rule.IsEnabled = isEnabled;
            await Repository.UpdateAsync(rule);
        }

        private KeywordFilterMatchResultDto? TestRuleMatch(string fileName, KeywordFilterRule rule)
        {
            var textToMatch = rule.IsCaseSensitive ? fileName : fileName.ToLowerInvariant();
            var keyword = rule.IsCaseSensitive ? rule.Keyword : rule.Keyword.ToLowerInvariant();
            string? matchedText = null;
            bool isMatch = false;

            switch (rule.MatchMode)
            {
                case MatchMode.Contains:
                    isMatch = textToMatch.Contains(keyword);
                    if (isMatch)
                    {
                        var index = textToMatch.IndexOf(keyword);
                        matchedText = fileName.Substring(index, Math.Min(keyword.Length, fileName.Length - index));
                    }
                    break;

                case MatchMode.StartsWith:
                    isMatch = textToMatch.StartsWith(keyword);
                    if (isMatch)
                    {
                        matchedText = fileName.Substring(0, Math.Min(keyword.Length, fileName.Length));
                    }
                    break;

                case MatchMode.EndsWith:
                    isMatch = textToMatch.EndsWith(keyword);
                    if (isMatch)
                    {
                        matchedText = fileName.Substring(Math.Max(0, fileName.Length - keyword.Length));
                    }
                    break;

                case MatchMode.Exact:
                    isMatch = textToMatch.Equals(keyword);
                    if (isMatch)
                    {
                        matchedText = fileName;
                    }
                    break;

                case MatchMode.Regex:
                    try
                    {
                        var regexOptions = rule.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                        var match = Regex.Match(fileName, rule.Keyword, regexOptions);
                        isMatch = match.Success;
                        if (isMatch)
                        {
                            matchedText = match.Value;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // 正则表达式无效，视为不匹配
                        isMatch = false;
                    }
                    break;

                default:
                    isMatch = false;
                    break;
            }

            if (!isMatch)
            {
                return null;
            }

            return new KeywordFilterMatchResultDto
            {
                RuleId = rule.Id,
                Keyword = rule.Keyword,
                MatchMode = rule.MatchMode,
                FilterType = rule.FilterType,
                Priority = rule.Priority,
                IsCaseSensitive = rule.IsCaseSensitive,
                MatchedText = matchedText
            };
        }
    }
}