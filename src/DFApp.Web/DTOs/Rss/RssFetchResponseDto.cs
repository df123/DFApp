using System.Collections.Generic;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS获取响应DTO
    /// </summary>
    public class RssFetchResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获取到的条目列表
        /// </summary>
        public List<RssItemDto> Items { get; set; } = new List<RssItemDto>();

        /// <summary>
        /// 条目总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 请求URL
        /// </summary>
        public string RequestUrl { get; set; } = string.Empty;

        /// <summary>
        /// HTTP状态码
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 响应时间（毫秒）
        /// </summary>
        public long ResponseTime { get; set; }

        /// <summary>
        /// 原始响应内容（用于调试）
        /// </summary>
        public string RawContent { get; set; } = string.Empty;
    }
}
