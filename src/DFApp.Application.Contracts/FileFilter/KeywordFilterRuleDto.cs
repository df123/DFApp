using System;
using Volo.Abp.Application.Dtos;

namespace DFApp.FileFilter
{
    /// <summary>
    /// 关键词过滤规则DTO
    /// </summary>
    public class KeywordFilterRuleDto : CreationAuditedEntityDto<long>
    {
        /// <summary>
        /// 关键词文本
        /// </summary>
        public required string Keyword { get; set; }

        /// <summary>
        /// 匹配模式
        /// </summary>
        public MatchMode MatchMode { get; set; } = MatchMode.Contains;

        /// <summary>
        /// 过滤类型（黑名单/白名单）
        /// </summary>
        public FilterType FilterType { get; set; } = FilterType.Blacklist;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 优先级（数字越小优先级越高）
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 是否区分大小写
        /// </summary>
        public bool IsCaseSensitive { get; set; } = false;
    }
}