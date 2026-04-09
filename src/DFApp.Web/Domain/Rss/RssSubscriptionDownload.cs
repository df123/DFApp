using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS订阅下载记录
    /// </summary>
    [SugarTable("AppRssSubscriptionDownloads")]
    public class RssSubscriptionDownload : CreationAuditedEntity<long>
    {
        /// <summary>
        /// 订阅ID
        /// </summary>
        [SugarColumn(ColumnName = "SubscriptionId")]
        public long SubscriptionId { get; set; }

        /// <summary>
        /// RSS镜像条目ID
        /// </summary>
        [SugarColumn(ColumnName = "RssMirrorItemId")]
        public long RssMirrorItemId { get; set; }

        /// <summary>
        /// Aria2任务ID
        /// </summary>
        [SugarColumn(ColumnName = "Aria2Gid")]
        public string Aria2Gid { get; set; } = string.Empty;

        /// <summary>
        /// 下载状态（0=未开始，1=下载中，2=已完成，3=失败）
        /// </summary>
        [SugarColumn(ColumnName = "DownloadStatus")]
        public int DownloadStatus { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [SugarColumn(ColumnName = "ErrorMessage")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 下载开始时间
        /// </summary>
        [SugarColumn(ColumnName = "DownloadStartTime")]
        public DateTime? DownloadStartTime { get; set; }

        /// <summary>
        /// 下载完成时间
        /// </summary>
        [SugarColumn(ColumnName = "DownloadCompleteTime")]
        public DateTime? DownloadCompleteTime { get; set; }

        /// <summary>
        /// 是否因磁盘空间不足而等待
        /// </summary>
        [SugarColumn(ColumnName = "IsPendingDueToLowDiskSpace")]
        public bool IsPendingDueToLowDiskSpace { get; set; }
    }
}
