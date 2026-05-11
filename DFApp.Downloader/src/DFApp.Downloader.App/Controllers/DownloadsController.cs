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
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>下载详情</summary>
    [HttpGet("downloads/{id}")]
    public IActionResult GetDownload(long id)
    {
        using var db = _dbContext.CreateClient();
        var item = db.Queryable<DownloadItem>().InSingle(id);
        if (item == null) return NotFound();
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
        return Ok(items);
    }

    /// <summary>暂停</summary>
    [HttpPost("downloads/{id}/pause")]
    public IActionResult Pause(long id)
    {
        _manager.PauseDownload(id);
        return Ok();
    }

    /// <summary>恢复</summary>
    [HttpPost("downloads/{id}/resume")]
    public IActionResult Resume(long id)
    {
        _manager.ResumeDownload(id);
        return Ok();
    }

    /// <summary>取消/删除</summary>
    [HttpDelete("downloads/{id}")]
    public IActionResult Delete(long id)
    {
        _manager.CancelDownload(id);
        return Ok();
    }

    /// <summary>获取设置</summary>
    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        return Ok(_settings);
    }

    /// <summary>更新设置</summary>
    [HttpPut("settings")]
    public IActionResult UpdateSettings([FromBody] DownloaderSettings newSettings)
    {
        // 更新内存中的设置
        _settings.DfAppUrl = newSettings.DfAppUrl;
        _settings.DfAppUsername = newSettings.DfAppUsername;
        _settings.DfAppPassword = newSettings.DfAppPassword;
        _settings.ApacheBaseUrl = newSettings.ApacheBaseUrl;
        _settings.ApacheUsername = newSettings.ApacheUsername;
        _settings.ApachePassword = newSettings.ApachePassword;
        _settings.DownloadPath = newSettings.DownloadPath;
        _settings.MaxConcurrentDownloads = newSettings.MaxConcurrentDownloads;
        _settings.MaxSegmentsPerFile = newSettings.MaxSegmentsPerFile;
        _settings.SegmentSize = newSettings.SegmentSize;
        _settings.AutoStart = newSettings.AutoStart;

        // 保存到文件
        var settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        var json = System.Text.Json.JsonSerializer.Serialize(_settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(settingsPath, json);

        return Ok(_settings);
    }

    /// <summary>全局状态</summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(_manager.GetStatus());
    }

    /// <summary>SignalR 连接状态</summary>
    [HttpGet("connection")]
    public IActionResult GetConnection()
    {
        return Ok(new { isConnected = _manager.GetStatus() });
    }
}
