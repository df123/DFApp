using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.FileFilter
{
    /// <summary>
    /// 关键词过滤规则实体
    /// </summary>
    [SugarTable("AppKeywordFilterRule")]
    public class KeywordFilterRule : CreationAuditedEntity<long>
    {
        /// <summary>
        /// 关键词文本
        /// </summary>
        [SugarColumn(ColumnName = "Keyword")]
        public string Keyword { get; set; } = string.Empty;

        /// <summary>
        /// 匹配模式
        /// </summary>
        [SugarColumn(ColumnName = "MatchMode")]
        public MatchMode MatchMode { get; set; } = MatchMode.Contains;

        /// <summary>
        /// 过滤类型（黑名单/白名单）
        /// </summary>
        [SugarColumn(ColumnName = "FilterType")]
        public FilterType FilterType { get; set; } = FilterType.Blacklist;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 优先级（数字越小优先级越高）
        /// </summary>
        [SugarColumn(ColumnName = "Priority")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "Remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// 是否区分大小写
        /// </summary>
        [SugarColumn(ColumnName = "IsCaseSensitive")]
        public bool IsCaseSensitive { get; set; } = false;
    }
}