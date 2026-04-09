using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS分词统计
    /// </summary>
    [SugarTable("AppRssWordSegment")]
    public class RssWordSegment : CreationAuditedEntity<long>
    {
        /// <summary>
        /// RSS镜像条目ID
        /// </summary>
        [SugarColumn(ColumnName = "RssMirrorItemId")]
        public long RssMirrorItemId { get; set; }

        /// <summary>
        /// 分词文本
        /// </summary>
        [SugarColumn(ColumnName = "Word")]
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// 语言类型（0=中文，1=英文，2=日文）
        /// </summary>
        [SugarColumn(ColumnName = "LanguageType")]
        public int LanguageType { get; set; }

        /// <summary>
        /// 出现次数
        /// </summary>
        [SugarColumn(ColumnName = "Count")]
        public int Count { get; set; }

        /// <summary>
        /// 词性
        /// </summary>
        [SugarColumn(ColumnName = "PartOfSpeech")]
        public string? PartOfSpeech { get; set; }
    }
}
