using System;
using SqlSugar;
using DFApp.Web.Domain;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS镜像条目
    /// </summary>
    [SugarTable("RssMirrorItems")]
    public class RssMirrorItem : AuditedEntity<long>
    {
        /// <summary>
        /// RSS源ID
        /// </summary>
        [SugarColumn(ColumnName = "RssSourceId")]
        public long RssSourceId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [SugarColumn(ColumnName = "Title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 链接
        /// </summary>
        [SugarColumn(ColumnName = "Link")]
        public string Link { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(ColumnName = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [SugarColumn(ColumnName = "Author")]
        public string? Author { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        [SugarColumn(ColumnName = "Category")]
        public string? Category { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        [SugarColumn(ColumnName = "PublishDate")]
        public DateTimeOffset? PublishDate { get; set; }

        /// <summary>
        /// 做种者数量
        /// </summary>
        [SugarColumn(ColumnName = "Seeders")]
        public int? Seeders { get; set; }

        /// <summary>
        /// 下载者数量
        /// </summary>
        [SugarColumn(ColumnName = "Leechers")]
        public int? Leechers { get; set; }

        /// <summary>
        /// 完成下载数量
        /// </summary>
        [SugarColumn(ColumnName = "Downloads")]
        public int? Downloads { get; set; }

        /// <summary>
        /// 扩展信息（JSON格式）
        /// </summary>
        [SugarColumn(ColumnName = "Extensions")]
        public string? Extensions { get; set; }

        /// <summary>
        /// 是否已下载
        /// </summary>
        [SugarColumn(ColumnName = "IsDownloaded")]
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// 下载时间
        /// </summary>
        [SugarColumn(ColumnName = "DownloadTime")]
        public DateTime? DownloadTime { get; set; }
    }
}
