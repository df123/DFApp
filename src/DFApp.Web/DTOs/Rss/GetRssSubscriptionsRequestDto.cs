using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// 查询RSS订阅请求DTO
    /// </summary>
    public class GetRssSubscriptionsRequestDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 关键词过滤
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// RSS源ID
        /// </summary>
        public long? RssSourceId { get; set; }
    }
}
