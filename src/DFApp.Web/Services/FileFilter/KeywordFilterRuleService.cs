using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFApp.FileFilter;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.FileFilter;

/// <summary>
/// 关键词过滤规则服务
/// </summary>
public class KeywordFilterRuleService : CrudServiceBase<KeywordFilterRule, long, KeywordFilterRuleDto, CreateUpdateKeywordFilterRuleDto, CreateUpdateKeywordFilterRuleDto>
{
    private readonly IKeywordFilterRuleRepository _keywordFilterRuleRepository;
    private readonly ILogger<KeywordFilterRuleService> _logger;
    private readonly FileFilterMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    /// <param name="keywordFilterRuleRepository">关键词过滤规则仓储接口</param>
    /// <param name="logger">日志记录器</param>
    public KeywordFilterRuleService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<KeywordFilterRule, long> repository,
        IKeywordFilterRuleRepository keywordFilterRuleRepository,
        ILogger<KeywordFilterRuleService> logger)
        : base(currentUser, permissionChecker, repository)
    {
        _keywordFilterRuleRepository = keywordFilterRuleRepository;
        _logger = logger;
    }

    /// <summary>
    /// 测试文件名过滤
    /// </summary>
    /// <param name="input">测试过滤请求</param>
    /// <returns>测试结果</returns>
    public async Task<KeywordFilterTestResultDto> TestFilterAsync(TestFilterRequestDto input)
    {
        if (string.IsNullOrWhiteSpace(input.FileName))
        {
            throw new BusinessException("文件名不能为空");
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

    /// <summary>
    /// 批量测试文件名过滤
    /// </summary>
    /// <param name="fileNames">文件名列表</param>
    /// <returns>测试结果列表</returns>
    public async Task<List<KeywordFilterTestResultDto>> TestFilterBatchAsync(List<string> fileNames)
    {
        if (fileNames == null || fileNames.Count == 0)
        {
            throw new BusinessException("文件名列表不能为空");
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

    /// <summary>
    /// 获取匹配的规则列表
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>匹配的规则列表</returns>
    public async Task<List<KeywordFilterMatchResultDto>> GetMatchingRulesAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BusinessException("文件名不能为空");
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

    /// <summary>
    /// 切换规则启用状态
    /// </summary>
    /// <param name="id">规则 ID</param>
    /// <param name="isEnabled">是否启用</param>
    public async Task ToggleRuleAsync(long id, bool isEnabled)
    {
        var rule = await Repository.GetByIdAsync(id);
        EnsureEntityExists(rule, id);

        rule.IsEnabled = isEnabled;
        await Repository.UpdateAsync(rule);
    }

    /// <summary>
    /// 测试规则匹配
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="rule">规则</param>
    /// <returns>匹配结果</returns>
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

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">关键词过滤规则实体</param>
    /// <returns>关键词过滤规则 DTO</returns>
    protected override KeywordFilterRuleDto MapToGetOutputDto(KeywordFilterRule entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>关键词过滤规则实体</returns>
    protected override KeywordFilterRule MapToEntity(CreateUpdateKeywordFilterRuleDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">关键词过滤规则实体</param>
    protected override void MapToEntity(CreateUpdateKeywordFilterRuleDto input, KeywordFilterRule entity)
    {
        var mapped = _mapper.MapToEntity(input);
        entity.Keyword = mapped.Keyword;
        entity.MatchMode = mapped.MatchMode;
        entity.FilterType = mapped.FilterType;
        entity.IsEnabled = mapped.IsEnabled;
        entity.Priority = mapped.Priority;
        entity.Remark = mapped.Remark;
        entity.IsCaseSensitive = mapped.IsCaseSensitive;
    }
}
