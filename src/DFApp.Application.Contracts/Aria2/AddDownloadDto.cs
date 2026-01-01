using System;
using System.Collections.Generic;

namespace DFApp.Aria2
{
    public class AddDownloadRequestDto
    {
        public List<string> Urls { get; set; } = new List<string>();
        public string? SavePath { get; set; }
        public Dictionary<string, object>? Options { get; set; }
        public bool VideoOnly { get; set; }
        public bool EnableKeywordFilter { get; set; } = true;
    }

    public class AddDownloadResponseDto
    {
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// 批量添加 URI 下载请求（每条链接创建独立任务）
    /// </summary>
    public class BatchAddUriRequestDto
    {
        /// <summary>
        /// URL 列表，每条链接将创建一个独立的下载任务
        /// </summary>
        public List<string> Urls { get; set; } = new List<string>();

        /// <summary>
        /// 保存路径（可选，应用于所有任务）
        /// </summary>
        public string? SavePath { get; set; }

        /// <summary>
        /// 下载选项（可选，应用于所有任务）
        /// </summary>
        public Dictionary<string, object>? Options { get; set; }

        /// <summary>
        /// 只下载视频（可选，应用于所有任务）
        /// </summary>
        public bool VideoOnly { get; set; }

        /// <summary>
        /// 启用关键词过滤（可选，应用于所有任务）
        /// </summary>
        public bool EnableKeywordFilter { get; set; } = true;
    }
}