using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Rss
{
    /// <summary>
    /// RSS获取请求DTO
    /// </summary>
    public class RssFetchRequestDto
    {
        /// <summary>
        /// RSS Feed URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 最大条目数（0表示无限制）
        /// </summary>
        public int MaxItems { get; set; } = 50;
    }

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
        /// 其他扩展字段（如种子信息）
        /// </summary>
        public Dictionary<string, string> Extensions { get; set; } = new Dictionary<string, string>();
    }

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