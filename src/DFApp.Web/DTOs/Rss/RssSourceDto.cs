using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Rss
{
    /// <summary>
    /// RSS源DTO
    /// </summary>
    public class RssSourceDto : EntityDto<long>
    {
        /// <summary>
        /// RSS源名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// RSS源URL
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 代理URL
        /// </summary>
        public string? ProxyUrl { get; set; }

        /// <summary>
        /// 代理用户名
        /// </summary>
        public string? ProxyUsername { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 抓取间隔（分钟）
        /// </summary>
        public int FetchIntervalMinutes { get; set; }

        /// <summary>
        /// 最大条目数
        /// </summary>
        public int MaxItems { get; set; }

        /// <summary>
        /// 查询关键词
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 最后抓取时间
        /// </summary>
        public DateTime? LastFetchTime { get; set; }

        /// <summary>
        /// 抓取状态（0=未开始，1=成功，2=失败）
        /// </summary>
        public int FetchStatus { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
