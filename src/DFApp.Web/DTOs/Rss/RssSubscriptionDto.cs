using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS订阅DTO
    /// </summary>
    public class RssSubscriptionDto : EntityDto<long>
    {
        /// <summary>
        /// 订阅名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 关键词
        /// </summary>
        public string Keywords { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 最小做种者数量
        /// </summary>
        public int? MinSeeders { get; set; }

        /// <summary>
        /// 最大做种者数量
        /// </summary>
        public int? MaxSeeders { get; set; }

        /// <summary>
        /// 最小下载者数量
        /// </summary>
        public int? MinLeechers { get; set; }

        /// <summary>
        /// 最大下载者数量
        /// </summary>
        public int? MaxLeechers { get; set; }

        /// <summary>
        /// 最小完成下载数量
        /// </summary>
        public int? MinDownloads { get; set; }

        /// <summary>
        /// 最大完成下载数量
        /// </summary>
        public int? MaxDownloads { get; set; }

        /// <summary>
        /// 质量过滤器
        /// </summary>
        public string? QualityFilter { get; set; }

        /// <summary>
        /// 字幕组过滤器
        /// </summary>
        public string? SubtitleGroupFilter { get; set; }

        /// <summary>
        /// 是否自动下载
        /// </summary>
        public bool AutoDownload { get; set; }

        /// <summary>
        /// 是否仅视频
        /// </summary>
        public bool VideoOnly { get; set; }

        /// <summary>
        /// 是否启用关键词过滤
        /// </summary>
        public bool EnableKeywordFilter { get; set; }

        /// <summary>
        /// 保存路径
        /// </summary>
        public string? SavePath { get; set; }

        /// <summary>
        /// RSS源ID
        /// </summary>
        public long? RssSourceId { get; set; }

        /// <summary>
        /// RSS源名称
        /// </summary>
        public string? RssSourceName { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }
    }
}
