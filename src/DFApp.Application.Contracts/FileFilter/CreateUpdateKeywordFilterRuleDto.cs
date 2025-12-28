using System.ComponentModel.DataAnnotations;

namespace DFApp.FileFilter
{
    /// <summary>
    /// 创建/更新关键词过滤规则DTO
    /// </summary>
    public class CreateUpdateKeywordFilterRuleDto
    {
        /// <summary>
        /// 关键词文本
        /// </summary>
        [Required(ErrorMessage = "关键词不能为空")]
        [StringLength(200, ErrorMessage = "关键词长度不能超过200个字符")]
        public required string Keyword { get; set; }

        /// <summary>
        /// 匹配模式
        /// </summary>
        [Required(ErrorMessage = "匹配模式不能为空")]
        public MatchMode MatchMode { get; set; } = MatchMode.Contains;

        /// <summary>
        /// 过滤类型（黑名单/白名单）
        /// </summary>
        [Required(ErrorMessage = "过滤类型不能为空")]
        public FilterType FilterType { get; set; } = FilterType.Blacklist;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 优先级（数字越小优先级越高）
        /// </summary>
        [Range(0, 999, ErrorMessage = "优先级必须在0-999之间")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string? Remark { get; set; }

        /// <summary>
        /// 是否区分大小写
        /// </summary>
        public bool IsCaseSensitive { get; set; } = false;
    }
}