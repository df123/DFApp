using System;

namespace DFApp.Web.DTOs.Media;

/// <summary>
/// 下载通知基类
/// </summary>
public class DownloadNotificationDto
{
    /// <summary>文件名</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long FileSize { get; set; }

    /// <summary>MIME 类型</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Apache 下载链接（由 ReturnDownloadUrlPrefix 生成）</summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>来源类型：Telegram | Aria2</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>原始记录 ID（MediaInfo.Id 或 TellStatusResult.Id）</summary>
    public long SourceId { get; set; }

    /// <summary>完成时间</summary>
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Telegram 媒体下载通知
/// </summary>
public class MediaDownloadNotificationDto : DownloadNotificationDto
{
    /// <summary>聊天 ID</summary>
    public long ChatId { get; set; }

    /// <summary>聊天标题</summary>
    public string ChatTitle { get; set; } = string.Empty;
}

/// <summary>
/// Aria2 下载通知
/// </summary>
public class Aria2DownloadNotificationDto : DownloadNotificationDto
{
    /// <summary>Aria2 GID</summary>
    public string Gid { get; set; } = string.Empty;

    /// <summary>种子名称</summary>
    public string TorrentName { get; set; } = string.Empty;
}
