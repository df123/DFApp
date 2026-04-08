using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// 查询RSS镜像请求DTO
    /// </summary>
    public class GetRssMirrorItemsRequestDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// RSS源ID
        /// </summary>
        public long? RssSourceId { get; set; }

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

        /// <summary>
        /// 是否已下载
        /// </summary>
        public bool? IsDownloaded { get; set; }

        /// <summary>
        /// 分词过滤
        /// </summary>
        public string? WordToken { get; set; }
    }
}
