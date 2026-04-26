using System;
using System.Collections.Generic;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS镜像条目DTO
    /// </summary>
    public class RssMirrorItemDto : EntityDto<long>
    {
        /// <summary>
        /// RSS源ID
        /// </summary>
        public long RssSourceId { get; set; }

        /// <summary>
        /// RSS源名称
        /// </summary>
        public string? RssSourceName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTimeOffset? PublishDate { get; set; }

        /// <summary>
        /// 做种者数量
        /// </summary>
        public int? Seeders { get; set; }

        /// <summary>
        /// 下载者数量
        /// </summary>
        public int? Leechers { get; set; }

        /// <summary>
        /// 完成下载数量
        /// </summary>
        public int? Downloads { get; set; }

        /// <summary>
        /// 是否已下载
        /// </summary>
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// 下载时间
        /// </summary>
        public DateTime? DownloadTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 分词列表
        /// </summary>
        public List<RssWordSegmentDto>? WordSegments { get; set; }
    }
}
