using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// 查询RSS订阅下载请求DTO
    /// </summary>
    public class GetRssSubscriptionDownloadsRequestDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 订阅ID
        /// </summary>
        public long? SubscriptionId { get; set; }

        /// <summary>
        /// RSS镜像条目ID
        /// </summary>
        public long? RssMirrorItemId { get; set; }

        /// <summary>
        /// 下载状态
        /// </summary>
        public int? DownloadStatus { get; set; }

        /// <summary>
        /// 关键词过滤
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
