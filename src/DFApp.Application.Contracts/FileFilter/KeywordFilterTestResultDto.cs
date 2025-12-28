using System.Collections.Generic;

namespace DFApp.FileFilter
{
    /// <summary>
    /// 关键词过滤测试结果DTO
    /// </summary>
    public class KeywordFilterTestResultDto
    {
        /// <summary>
        /// 测试的文件名
        /// </summary>
        public required string FileName { get; set; }

        /// <summary>
        /// 是否应被过滤
        /// </summary>
        public bool ShouldFilter { get; set; }

        /// <summary>
        /// 匹配的规则列表
        /// </summary>
        public List<KeywordFilterMatchResultDto> MatchingRules { get; set; } = new();

        /// <summary>
        /// 最终决定的原因
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// 关键词过滤匹配结果DTO
    /// </summary>
    public class KeywordFilterMatchResultDto
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public long RuleId { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public required string Keyword { get; set; }

        /// <summary>
        /// 匹配模式
        /// </summary>
        public MatchMode MatchMode { get; set; }

        /// <summary>
        /// 过滤类型
        /// </summary>
        public FilterType FilterType { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 是否区分大小写
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// 匹配的文本片段（用于调试）
        /// </summary>
        public string? MatchedText { get; set; }
    }
}