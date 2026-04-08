using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS订阅下载DTO
    /// </summary>
    public class RssSubscriptionDownloadDto : EntityDto<long>
    {
        /// <summary>
        /// 订阅ID
        /// </summary>
        public long SubscriptionId { get; set; }

        /// <summary>
        /// 订阅名称
        /// </summary>
        public string? SubscriptionName { get; set; }

        /// <summary>
        /// RSS镜像条目ID
        /// </summary>
        public long RssMirrorItemId { get; set; }

        /// <summary>
        /// RSS镜像条目标题
        /// </summary>
        public string? RssMirrorItemTitle { get; set; }

        /// <summary>
        /// RSS镜像条目链接
        /// </summary>
        public string? RssMirrorItemLink { get; set; }

        /// <summary>
        /// RSS源名称
        /// </summary>
        public string? RssSourceName { get; set; }

        /// <summary>
        /// Aria2任务ID
        /// </summary>
        public string Aria2Gid { get; set; } = string.Empty;

        /// <summary>
        /// 下载状态（0=未开始，1=下载中，2=已完成，3=失败）
        /// </summary>
        public int DownloadStatus { get; set; }

        /// <summary>
        /// 下载状态文本
        /// </summary>
        public string? DownloadStatusText { get; set; }

        /// <summary>
        /// 是否因磁盘空间不足而暂存
        /// </summary>
        public bool IsPendingDueToLowDiskSpace { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 下载开始时间
        /// </summary>
        public DateTime? DownloadStartTime { get; set; }

        /// <summary>
        /// 下载完成时间
        /// </summary>
        public DateTime? DownloadCompleteTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
