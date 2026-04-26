using System.Collections.Generic;

namespace DFApp.Aria2;

/// <summary>
/// 添加下载请求 DTO
/// </summary>
public class AddDownloadRequestDto
{
    /// <summary>
    /// 下载 URL 列表
    /// </summary>
    public List<string> Urls { get; set; } = new();

    /// <summary>
    /// 保存路径
    /// </summary>
    public string? SavePath { get; set; }

    /// <summary>
    /// 自定义选项
    /// </summary>
    public Dictionary<string, object>? Options { get; set; }

    /// <summary>
    /// 是否只下载视频
    /// </summary>
    public bool VideoOnly { get; set; }

    /// <summary>
    /// 是否启用关键词过滤
    /// </summary>
    public bool EnableKeywordFilter { get; set; } = true;
}

/// <summary>
/// 添加下载响应 DTO
/// </summary>
public class AddDownloadResponseDto
{
    /// <summary>
    /// 请求 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// 批量添加 URI 下载请求（每条链接创建独立任务）
/// </summary>
public class BatchAddUriRequestDto
{
    /// <summary>
    /// URL 列表
    /// </summary>
    public List<string> Urls { get; set; } = new();

    /// <summary>
    /// 保存路径
    /// </summary>
    public string? SavePath { get; set; }

    /// <summary>
    /// 自定义选项
    /// </summary>
    public Dictionary<string, object>? Options { get; set; }

    /// <summary>
    /// 只下载视频
    /// </summary>
    public bool VideoOnly { get; set; }

    /// <summary>
    /// 启用关键词过滤
    /// </summary>
    public bool EnableKeywordFilter { get; set; } = true;
}

/// <summary>
/// 添加种子文件下载请求
/// </summary>
public class AddTorrentRequestDto
{
    /// <summary>
    /// 种子文件的 Base64 编码内容
    /// </summary>
    public string TorrentData { get; set; } = string.Empty;

    /// <summary>
    /// 保存路径
    /// </summary>
    public string? SavePath { get; set; }

    /// <summary>
    /// 自定义选项
    /// </summary>
    public Dictionary<string, object>? Options { get; set; }
}

/// <summary>
/// 批量添加种子文件下载请求
/// </summary>
public class BatchAddTorrentRequestDto
{
    /// <summary>
    /// 种子文件列表
    /// </summary>
    public List<TorrentFileItemDto> Torrents { get; set; } = new();

    /// <summary>
    /// 保存路径
    /// </summary>
    public string? SavePath { get; set; }
}

/// <summary>
/// 种子文件项
/// </summary>
public class TorrentFileItemDto
{
    /// <summary>
    /// 种子文件的 Base64 编码内容
    /// </summary>
    public string TorrentData { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// 暂停任务请求
/// </summary>
public class PauseTasksRequestDto
{
    /// <summary>
    /// GID 列表
    /// </summary>
    public List<string> Gids { get; set; } = new();
}

/// <summary>
/// 停止任务请求
/// </summary>
public class StopTasksRequestDto
{
    /// <summary>
    /// GID 列表
    /// </summary>
    public List<string> Gids { get; set; } = new();
}

/// <summary>
/// 移除任务请求
/// </summary>
public class RemoveTasksRequestDto
{
    /// <summary>
    /// GID 列表
    /// </summary>
    public List<string> Gids { get; set; } = new();
}
