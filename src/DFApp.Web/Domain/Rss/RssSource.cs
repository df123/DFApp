using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS源配置
    /// </summary>
    [SugarTable("AppRssSource")]
    public class RssSource : CreationAuditedEntity<long>
    {
        /// <summary>
        /// RSS源名称
        /// </summary>
        [SugarColumn(ColumnName = "Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// RSS源URL
        /// </summary>
        [SugarColumn(ColumnName = "Url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 代理URL
        /// </summary>
        [SugarColumn(ColumnName = "ProxyUrl")]
        public string? ProxyUrl { get; set; }

        /// <summary>
        /// 代理用户名
        /// </summary>
        [SugarColumn(ColumnName = "ProxyUsername")]
        public string? ProxyUsername { get; set; }

        /// <summary>
        /// 代理密码
        /// </summary>
        [SugarColumn(ColumnName = "ProxyPassword")]
        public string? ProxyPassword { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "IsEnabled")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 抓取间隔（分钟）
        /// </summary>
        [SugarColumn(ColumnName = "FetchIntervalMinutes")]
        public int FetchIntervalMinutes { get; set; }

        /// <summary>
        /// 最大条目数
        /// </summary>
        [SugarColumn(ColumnName = "MaxItems")]
        public int MaxItems { get; set; }

        /// <summary>
        /// 查询关键词
        /// </summary>
        [SugarColumn(ColumnName = "Query")]
        public string? Query { get; set; }

        /// <summary>
        /// 最后抓取时间
        /// </summary>
        [SugarColumn(ColumnName = "LastFetchTime")]
        public DateTime? LastFetchTime { get; set; }

        /// <summary>
        /// 抓取状态（0=未开始，1=成功，2=失败）
        /// </summary>
        [SugarColumn(ColumnName = "FetchStatus")]
        public int FetchStatus { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [SugarColumn(ColumnName = "ErrorMessage")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "Remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// 扩展属性（JSON格式）
        /// </summary>
        [SugarColumn(ColumnName = "ExtraProperties")]
        public string ExtraProperties { get; set; } = string.Empty;
    }
}
