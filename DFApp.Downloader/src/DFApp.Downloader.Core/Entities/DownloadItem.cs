using SqlSugar;

namespace DFApp.Downloader.Core.Entities;

/// <summary>
/// 下载项状态枚举
/// </summary>
public static class DownloadStatus
{
    public const string Pending = "Pending";
    public const string Downloading = "Downloading";
    public const string Paused = "Paused";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}

/// <summary>
/// 下载项实体
/// </summary>
[SugarTable("DownloadItems")]
public class DownloadItem
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>来源类型：Telegram | Aria2</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>原始记录 ID</summary>
    public long SourceId { get; set; }

    /// <summary>文件名</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long FileSize { get; set; }

    /// <summary>Apache 下载链接</summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>本地保存路径</summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>状态</summary>
    public string Status { get; set; } = DownloadStatus.Pending;

    /// <summary>已下载字节数</summary>
    public long DownloadedBytes { get; set; }

    /// <summary>MIME 类型</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>聊天标题（Telegram 来源）</summary>
    public string? ChatTitle { get; set; }

    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// 下载分片实体
/// </summary>
[SugarTable("DownloadSegments")]
public class DownloadSegment
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>关联的下载项 ID</summary>
    public long DownloadItemId { get; set; }

    /// <summary>分片索引</summary>
    public int SegmentIndex { get; set; }

    /// <summary>起始偏移</summary>
    public long StartOffset { get; set; }

    /// <summary>结束偏移</summary>
    public long EndOffset { get; set; }

    /// <summary>已下载字节数</summary>
    public long DownloadedBytes { get; set; }

    /// <summary>状态</summary>
    public string Status { get; set; } = DownloadStatus.Pending;
}
