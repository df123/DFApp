using System;
using System.Collections.Generic;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS条目DTO
    /// </summary>
    public class RssItemDto
    {
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
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 发布日期
        /// </summary>
        public DateTimeOffset? PublishDate { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// 分类
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 做种人数（Seeders）
        /// </summary>
        public int? Seeders { get; set; }

        /// <summary>
        /// 下载人数（Leechers）
        /// </summary>
        public int? Leechers { get; set; }

        /// <summary>
        /// 完成下载次数（Downloads）
        /// </summary>
        public int? Downloads { get; set; }

        /// <summary>
        /// 其他扩展字段（如种子信息）
        /// </summary>
        public Dictionary<string, string> Extensions { get; set; } = new Dictionary<string, string>();
    }
}
