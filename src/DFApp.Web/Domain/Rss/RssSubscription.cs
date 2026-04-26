using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS订阅配置
    /// </summary>
    [SugarTable("AppRssSubscriptions")]
    public class RssSubscription : AuditedEntity<long>
    {
        /// <summary>
        /// 订阅名称
        /// </summary>
        [SugarColumn(ColumnName = "Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 关键词
        /// </summary>
        [SugarColumn(ColumnName = "Keywords")]
        public string Keywords { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(ColumnName = "IsEnabled")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 最小做种者数量
        /// </summary>
        [SugarColumn(ColumnName = "MinSeeders")]
        public int? MinSeeders { get; set; }

        /// <summary>
        /// 最大做种者数量
        /// </summary>
        [SugarColumn(ColumnName = "MaxSeeders")]
        public int? MaxSeeders { get; set; }

        /// <summary>
        /// 最小下载者数量
        /// </summary>
        [SugarColumn(ColumnName = "MinLeechers")]
        public int? MinLeechers { get; set; }

        /// <summary>
        /// 最大下载者数量
        /// </summary>
        [SugarColumn(ColumnName = "MaxLeechers")]
        public int? MaxLeechers { get; set; }

        /// <summary>
        /// 最小完成下载数量
        /// </summary>
        [SugarColumn(ColumnName = "MinDownloads")]
        public int? MinDownloads { get; set; }

        /// <summary>
        /// 最大完成下载数量
        /// </summary>
        [SugarColumn(ColumnName = "MaxDownloads")]
        public int? MaxDownloads { get; set; }

        /// <summary>
        /// 质量过滤器
        /// </summary>
        [SugarColumn(ColumnName = "QualityFilter")]
        public string? QualityFilter { get; set; }

        /// <summary>
        /// 字幕组过滤器
        /// </summary>
        [SugarColumn(ColumnName = "SubtitleGroupFilter")]
        public string? SubtitleGroupFilter { get; set; }

        /// <summary>
        /// 是否自动下载
        /// </summary>
        [SugarColumn(ColumnName = "AutoDownload")]
        public bool AutoDownload { get; set; }

        /// <summary>
        /// 是否仅视频
        /// </summary>
        [SugarColumn(ColumnName = "VideoOnly")]
        public bool VideoOnly { get; set; }

        /// <summary>
        /// 是否启用关键词过滤
        /// </summary>
        [SugarColumn(ColumnName = "EnableKeywordFilter")]
        public bool EnableKeywordFilter { get; set; }

        /// <summary>
        /// 保存路径
        /// </summary>
        [SugarColumn(ColumnName = "SavePath")]
        public string? SavePath { get; set; }

        /// <summary>
        /// RSS源ID
        /// </summary>
        [SugarColumn(ColumnName = "RssSourceId")]
        public long? RssSourceId { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        [SugarColumn(ColumnName = "StartDate")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        [SugarColumn(ColumnName = "EndDate")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnName = "Remark")]
        public string? Remark { get; set; }
    }
}
