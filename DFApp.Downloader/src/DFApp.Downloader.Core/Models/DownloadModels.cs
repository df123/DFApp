namespace DFApp.Downloader.Core.Models;

/// <summary>
/// 下载通知基类（与 DFApp.Web.DTOs.Media.DownloadNotificationDto 对应）
/// </summary>
public class DownloadNotification
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public long SourceId { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Telegram 媒体下载通知
/// </summary>
public class MediaDownloadNotification : DownloadNotification
{
    public long ChatId { get; set; }
    public string ChatTitle { get; set; } = string.Empty;
}

/// <summary>
/// Aria2 下载通知
/// </summary>
public class Aria2DownloadNotification : DownloadNotification
{
    public string Gid { get; set; } = string.Empty;
    public string TorrentName { get; set; } = string.Empty;
}

/// <summary>
/// 下载进度信息
/// </summary>
public class DownloadProgress
{
    public long DownloadItemId { get; set; }
    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }
    public double SpeedBytesPerSecond { get; set; }
    public double Percentage => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes * 100 : 0;
    public TimeSpan? EstimatedTimeRemaining =>
        SpeedBytesPerSecond > 0
            ? TimeSpan.FromSeconds((TotalBytes - DownloadedBytes) / SpeedBytesPerSecond)
            : null;
}

/// <summary>
/// DFApp 登录请求
/// </summary>
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DFApp 登录响应
/// </summary>
public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
