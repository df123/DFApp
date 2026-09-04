using DFApp.Downloader.Core;
using DFApp.Downloader.Core.Configuration;
using DFApp.Downloader.Core.Data;
using DFApp.Downloader.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace DFApp.Downloader.App.Controllers;

[ApiController]
[Route("api")]
public class DownloadsController : ControllerBase
{
    private readonly DownloadManager _manager;
    private readonly DownloaderDbContext _dbContext;
    private readonly DownloaderSettings _settings;

    public DownloadsController(DownloadManager manager, DownloaderDbContext dbContext, DownloaderSettings settings)
    {
        _manager = manager;
        _dbContext = dbContext;
        _settings = settings;
    }

    /// <summary>下载列表（分页）</summary>
    [HttpGet("downloads")]
    public IActionResult GetDownloads([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        using var db = _dbContext.CreateClient();
        var query = db.Queryable<DownloadItem>();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var total = query.Count();
        var items = query
            // 优先级：下载中 > 等待中 > 失败 > 已暂停 > 已完成；组内按 UpdatedAt 倒序
            // ——正在下载的任务会被 OnDownloadStarted 回调刷新 UpdatedAt，从而浮到组内顶部
            .OrderBy("CASE WHEN Status='Downloading' THEN 0 WHEN Status='Pending' THEN 1 WHEN Status='Failed' THEN 2 WHEN Status='Paused' THEN 3 ELSE 4 END")
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // 填充实时速度（仅活跃下载有值）
        foreach (var item in items)
        {
            item.SpeedBytesPerSecond = _manager.GetItemSpeed(item.Id);
            MarkUtc(item);
        }

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>下载详情</summary>
    [HttpGet("downloads/{id}")]
    public IActionResult GetDownload(int id)
    {
        using var db = _dbContext.CreateClient();
        var item = db.Queryable<DownloadItem>().InSingle(id);
        if (item == null) return NotFound();
        item.SpeedBytesPerSecond = _manager.GetItemSpeed(item.Id);
        MarkUtc(item);
        return Ok(item);
    }

    /// <summary>活跃下载</summary>
    [HttpGet("downloads/active")]
    public IActionResult GetActiveDownloads()
    {
        using var db = _dbContext.CreateClient();
        var items = db.Queryable<DownloadItem>()
            .Where(x => x.Status == DownloadStatus.Downloading)
            .ToList();
        foreach (var item in items)
        {
            item.SpeedBytesPerSecond = _manager.GetItemSpeed(item.Id);
            MarkUtc(item);
        }
        return Ok(items);
    }

    /// <summary>等待队列</summary>
    [HttpGet("downloads/queue")]
    public IActionResult GetQueue()
    {
        using var db = _dbContext.CreateClient();
        var items = db.Queryable<DownloadItem>()
            .Where(x => x.Status == DownloadStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToList();
        foreach (var item in items)
        {
            MarkUtc(item);
        }
        return Ok(items);
    }

    /// <summary>暂停</summary>
    [HttpPost("downloads/{id}/pause")]
    public IActionResult Pause(int id)
    {
        _manager.PauseDownload(id);
        return Ok();
    }

    /// <summary>恢复</summary>
    [HttpPost("downloads/{id}/resume")]
    public IActionResult Resume(int id)
    {
        _manager.ResumeDownload(id);
        return Ok();
    }

    /// <summary>取消/删除</summary>
    [HttpDelete("downloads/{id}")]
    public IActionResult Delete(int id)
    {
        _manager.CancelDownload(id);
        return Ok();
    }

    /// <summary>批量删除失败任务</summary>
    [HttpDelete("downloads/failed")]
    public IActionResult DeleteFailed()
    {
        var deletedCount = _manager.DeleteFailedDownloads();
        return Ok(new { deletedCount });
    }

    /// <summary>密码回显掩码；更新时提交该哨兵值表示保留原密码</summary>
    public const string PasswordMask = "********";

    /// <summary>获取设置（凭据脱敏回显，不再向任何调用方回传明文密码）</summary>
    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        var masked = System.Text.Json.JsonSerializer.Deserialize<DownloaderSettings>(
            System.Text.Json.JsonSerializer.Serialize(_settings))!;
        if (!string.IsNullOrEmpty(masked.DfAppPassword)) masked.DfAppPassword = PasswordMask;
        if (!string.IsNullOrEmpty(masked.ApachePassword)) masked.ApachePassword = PasswordMask;
        if (!string.IsNullOrEmpty(masked.ManagementToken)) masked.ManagementToken = PasswordMask;
        return Ok(masked);
    }

    /// <summary>更新设置（密码/令牌提交掩码哨兵值时保留原值）</summary>
    [HttpPut("settings")]
    public IActionResult UpdateSettings([FromBody] DownloaderSettings newSettings)
    {
        // 更新内存中的设置；掩码哨兵表示"未修改"，避免回显值原样写回覆盖真实密码
        _settings.DfAppUrl = newSettings.DfAppUrl;
        _settings.DfAppUsername = newSettings.DfAppUsername;
        _settings.DfAppPassword = newSettings.DfAppPassword == PasswordMask
            ? _settings.DfAppPassword : newSettings.DfAppPassword;
        _settings.ApacheUsername = newSettings.ApacheUsername;
        _settings.ApachePassword = newSettings.ApachePassword == PasswordMask
            ? _settings.ApachePassword : newSettings.ApachePassword;
        _settings.ManagementToken = newSettings.ManagementToken == PasswordMask
            ? _settings.ManagementToken : newSettings.ManagementToken;
        _settings.DownloadPath = newSettings.DownloadPath;
        _settings.MaxConcurrentDownloads = newSettings.MaxConcurrentDownloads;
        _settings.MaxSegmentsPerFile = newSettings.MaxSegmentsPerFile;
        _settings.SegmentSize = newSettings.SegmentSize;
        _settings.AutoStart = newSettings.AutoStart;
        _settings.SyncMissedOnStartup = newSettings.SyncMissedOnStartup;

        // 保存到文件
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        var json = System.Text.Json.JsonSerializer.Serialize(_settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(settingsPath, json);

        return GetSettings();
    }

    /// <summary>全局状态</summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(_manager.GetStatus());
    }

    /// <summary>速度记录支持的时间范围与对应聚合桶粒度</summary>
    private static readonly Dictionary<string, (TimeSpan Span, TimeSpan Bucket)> SpeedHistoryRanges = new()
    {
        ["1h"] = (TimeSpan.FromHours(1), TimeSpan.FromMinutes(1)),
        ["6h"] = (TimeSpan.FromHours(6), TimeSpan.FromMinutes(5)),
        ["24h"] = (TimeSpan.FromHours(24), TimeSpan.FromMinutes(15)),
        ["7d"] = (TimeSpan.FromDays(7), TimeSpan.FromHours(1)),
        ["30d"] = (TimeSpan.FromDays(30), TimeSpan.FromHours(3)),
    };

    /// <summary>
    /// 速度记录：按时间桶聚合的历史全局下载速度（range: 1h|6h|24h|7d|30d，默认 24h）。
    /// 空闲期不采样，对应桶的速度为 0。
    /// </summary>
    [HttpGet("speed-history")]
    public IActionResult GetSpeedHistory([FromQuery] string range = "24h")
    {
        if (!SpeedHistoryRanges.TryGetValue(range.ToLowerInvariant(), out var conf))
        {
            return BadRequest(new { message = "range 仅支持 1h/6h/24h/7d/30d" });
        }

        var now = DateTime.UtcNow;
        var from = now - conf.Span;
        var bucketSeconds = (long)conf.Bucket.TotalSeconds;

        using var db = _dbContext.CreateClient();
        var samples = db.Queryable<DownloadSpeedSample>()
            .Where(x => x.RecordedAt >= from && x.RecordedAt <= now)
            .OrderBy(x => x.RecordedAt)
            .Select(x => new { x.RecordedAt, x.SpeedBytesPerSecond })
            .ToList();

        // 按桶聚合：桶编号 = Unix 秒 / 桶秒数（各桶粒度均能整除对齐）
        // SQLite 读回的 DateTime.Kind 为 Unspecified，必须显式标记 UTC，否则会按本地时区解释导致桶键偏移 8 小时
        var buckets = new Dictionary<long, (double Sum, double Max, int Count)>();
        foreach (var s in samples)
        {
            var key = new DateTimeOffset(DateTime.SpecifyKind(s.RecordedAt, DateTimeKind.Utc)).ToUnixTimeSeconds() / bucketSeconds;
            if (buckets.TryGetValue(key, out var b))
            {
                buckets[key] = (b.Sum + s.SpeedBytesPerSecond, Math.Max(b.Max, s.SpeedBytesPerSecond), b.Count + 1);
            }
            else
            {
                buckets[key] = (s.SpeedBytesPerSecond, s.SpeedBytesPerSecond, 1);
            }
        }

        // 生成连续桶序列，无样本的桶（空闲期）速度为 0
        var startKey = new DateTimeOffset(from).ToUnixTimeSeconds() / bucketSeconds;
        var endKey = new DateTimeOffset(now).ToUnixTimeSeconds() / bucketSeconds;
        var items = new List<object>(capacity: (int)(endKey - startKey + 1));
        for (var key = startKey; key <= endKey; key++)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(key * bucketSeconds).UtcDateTime;
            if (buckets.TryGetValue(key, out var b))
            {
                items.Add(new { time, avgSpeed = b.Sum / b.Count, maxSpeed = b.Max });
            }
            else
            {
                items.Add(new { time, avgSpeed = 0.0, maxSpeed = 0.0 });
            }
        }

        return Ok(new { range = range.ToLowerInvariant(), bucketSeconds, items });
    }

    /// <summary>SignalR 连接状态</summary>
    [HttpGet("connection")]
    public IActionResult GetConnection()
    {
        var status = _manager.GetStatus();
        return Ok(new { isConnected = status.IsConnected, lastError = status.LastError });
    }

    /// <summary>重新连接 DFApp 后端</summary>
    [HttpPost("connection/reconnect")]
    public async Task<IActionResult> Reconnect()
    {
        await _manager.TryConnectAsync();
        var status = _manager.GetStatus();
        return Ok(new { isConnected = status.IsConnected, lastError = status.LastError });
    }

    /// <summary>补漏同步：拉取服务器已下载完成但本地缺失的媒体</summary>
    [HttpPost("downloads/sync-missed")]
    public async Task<IActionResult> SyncMissed()
    {
        try
        {
            var (scanned, added, reconciled) = await _manager.SyncMissedDownloadsAsync();
            return Ok(new { scanned, added, reconciled });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>媒体库（大图浏览）：已完成且本地文件存在的记录，含聊天标题与消息</summary>
    [HttpGet("gallery")]
    public IActionResult GetGallery([FromQuery] int page = 1, [FromQuery] int pageSize = 60)
    {
        using var db = _dbContext.CreateClient();
        var query = db.Queryable<DownloadItem>()
            .Where(x => x.Status == DownloadStatus.Completed)
            .OrderByDescending(x => x.CompletedAt);

        var total = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            .Where(x => System.IO.File.Exists(x.LocalPath)) // 本地文件被清理的记录不展示
            .Select(x => new
            {
                x.Id,
                x.FileName,
                x.FileSize,
                x.MimeType,
                x.ChatTitle,
                x.Message,
                CompletedAt = DateTime.SpecifyKind(x.CompletedAt, DateTimeKind.Utc),
                // 相对下载目录的访问路径，供 /media 静态映射加载
                MediaUrl = $"/media/{Path.GetFileName(x.LocalPath)}",
                // Windows 路径，供前端拼 vlc: 协议链接直接唤起 VLC
                WindowsPath = ConvertToWindowsPath(x.LocalPath),
                // 视频缩略图：下载目录/.thumbs/{文件名}.jpg，存在才返回
                ThumbUrl = System.IO.File.Exists(GetThumbnailPath(x.FileName))
                    ? $"/media/{ThumbnailDirName}/{Path.GetFileNameWithoutExtension(x.FileName)}.jpg"
                    : null
            })
            .ToList();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>回填历史记录的聊天标题与消息（从后端按 mediaId 查询）</summary>
    [HttpPost("gallery/backfill-messages")]
    public async Task<IActionResult> BackfillMessages()
    {
        try
        {
            var updated = await _manager.BackfillGalleryMessagesAsync();
            return Ok(new { updated });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>用 VLC 播放本地视频文件（通过 Windows 侧 vlc.exe 打开）</summary>
    [HttpPost("gallery/{id}/play")]
    public IActionResult PlayWithVlc(int id)
    {
        using var db = _dbContext.CreateClient();
        var item = db.Queryable<DownloadItem>().InSingle(id);
        if (item == null)
        {
            return NotFound(new { message = "记录不存在" });
        }

        if (!System.IO.File.Exists(item.LocalPath))
        {
            return NotFound(new { message = "本地文件不存在" });
        }

        // WSL 环境：/mnt/d/xxx 对应 Windows 的 D:\xxx，VLC 需以 Windows 路径打开
        var windowsPath = ConvertToWindowsPath(item.LocalPath);
        var vlcPath = "/mnt/c/Program Files/VideoLAN/VLC/vlc.exe";

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                // 直接经 WSL interop 启动 Windows 侧 vlc.exe（经 cmd.exe 会因 UNC 当前目录失败）
                FileName = vlcPath,
                UseShellExecute = false,
                // 避免继承 WSL UNC 工作目录
                WorkingDirectory = "/tmp"
            };
            psi.ArgumentList.Add(windowsPath);
            System.Diagnostics.Process.Start(psi);
            return Ok(new { message = "已调用 VLC 播放", path = windowsPath });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"调用 VLC 失败: {ex.Message}" });
        }
    }

    /// <summary>将 WSL 路径（/mnt/d/...）转换为 Windows 路径（D:\...）</summary>
    private static string ConvertToWindowsPath(string path)
    {
        var full = Path.GetFullPath(path);
        if (full.StartsWith("/mnt/", StringComparison.OrdinalIgnoreCase))
        {
            var drive = char.ToUpperInvariant(full[5]);
            var rest = full[6..].Replace('/', '\\');
            return $"{drive}:{rest}";
        }
        return full.Replace('/', '\\');
    }

    /// <summary>
    /// 给时间字段标记 UTC Kind。SQLite 读取的 DateTime.Kind 为 Unspecified，
    /// 序列化时不带 Z 后缀，前端会误按本地时区解析导致显示偏差 8 小时
    /// </summary>
    private static void MarkUtc(DownloadItem item)
    {
        item.CreatedAt = DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc);
        item.UpdatedAt = DateTime.SpecifyKind(item.UpdatedAt, DateTimeKind.Utc);
        if (item.CompletedAt != DateTime.MinValue)
        {
            item.CompletedAt = DateTime.SpecifyKind(item.CompletedAt, DateTimeKind.Utc);
        }
    }

    /// <summary>缩略图存放目录名（下载目录下，与 DownloadManager 保持一致）</summary>
    private const string ThumbnailDirName = "thumbs";

    /// <summary>缩略图完整路径：{下载目录}/.thumbs/{文件名}.jpg</summary>
    private string GetThumbnailPath(string fileName)
    {
        var dir = Path.Combine(_settings.DownloadPath, ThumbnailDirName);
        return Path.Combine(dir, Path.GetFileNameWithoutExtension(fileName) + ".jpg");
    }
}
