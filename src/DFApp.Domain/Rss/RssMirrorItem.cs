using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS镜像条目
    /// </summary>
    public class RssMirrorItem : Entity<long>, IHasCreationTime, IHasModificationTime, IHasConcurrencyStamp
    {
        /// <summary>
        /// RSS源ID
        /// </summary>
        public long RssSourceId { get; set; }

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
        /// 扩展信息（JSON格式）
        /// </summary>
        public string? Extensions { get; set; }

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
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 并发标记
        /// </summary>
        public string ConcurrencyStamp { get; set; } = string.Empty;
    }
}
