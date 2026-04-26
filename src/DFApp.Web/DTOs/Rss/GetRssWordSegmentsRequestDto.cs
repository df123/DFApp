using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// 查询RSS分词请求DTO
    /// </summary>
    public class GetRssWordSegmentsRequestDto : PagedAndSortedResultRequestDto
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
        /// 语言类型
        /// </summary>
        public int? LanguageType { get; set; }

        /// <summary>
        /// 分词文本（精确匹配）
        /// </summary>
        public string? Word { get; set; }
    }
}
