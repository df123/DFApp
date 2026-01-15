using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace DFApp.Rss
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

    /// <summary>
    /// RSS分词DTO
    /// </summary>
    public class RssWordSegmentDto : EntityDto<long>
    {
        /// <summary>
        /// RSS镜像条目ID
        /// </summary>
        public long RssMirrorItemId { get; set; }

        /// <summary>
        /// 分词文本
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// 语言类型（0=中文，1=英文，2=日文）
        /// </summary>
        public int LanguageType { get; set; }

        /// <summary>
        /// 出现次数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 词性
        /// </summary>
        public string? PartOfSpeech { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }

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

    /// <summary>
    /// 创建/更新RSS源DTO
    /// </summary>
    public class CreateUpdateRssSourceDto
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
        /// 代理密码
        /// </summary>
        public string? ProxyPassword { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 抓取间隔（分钟）
        /// </summary>
        public int FetchIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// 最大条目数
        /// </summary>
        public int MaxItems { get; set; } = 50;

        /// <summary>
        /// 查询关键词
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 分词统计DTO
    /// </summary>
    public class WordSegmentStatisticsDto
    {
        /// <summary>
        /// 分词文本
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// 总出现次数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 包含该分词的镜像条目数量
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// 语言类型
        /// </summary>
        public int LanguageType { get; set; }
    }

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
