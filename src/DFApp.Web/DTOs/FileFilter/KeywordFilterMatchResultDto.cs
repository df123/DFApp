using DFApp.FileFilter;

namespace DFApp.Web.DTOs.FileFilter
{
    /// <summary>
    /// 关键词过滤匹配结果 DTO
    /// </summary>
    public class KeywordFilterMatchResultDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public KeywordFilterMatchResultDto()
        {
        }

        /// <summary>
        /// 规则 ID
        /// </summary>
        public long RuleId { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 匹配模式
        /// </summary>
        public MatchMode MatchMode { get; set; }

        /// <summary>
        /// 过滤类型（黑名单/白名单）
        /// </summary>
        public FilterType FilterType { get; set; }

        /// <summary>
        /// 规则优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 是否区分大小写
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// 匹配到的文本内容
        /// </summary>
        public string? MatchedText { get; set; }
    }
}
