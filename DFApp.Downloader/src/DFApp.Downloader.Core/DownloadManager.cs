using System.Collections.Concurrent;
using DFApp.Downloader.Core.Configuration;
using DFApp.Downloader.Core.Data;
using DFApp.Downloader.Core.Engine;
using DFApp.Downloader.Core.Entities;
using DFApp.Downloader.Core.Models;
using DFApp.Downloader.Core.SignalR;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DFApp.Downloader.Core;

/// <summary>
/// 下载管理器，协调 SignalR 通知、下载队列和下载引擎
/// </summary>
public class DownloadManager : IAsyncDisposable
{
    private readonly DownloadNotificationClient _notificationClient;
    private readonly DownloadEngine _downloadEngine;
    private readonly DownloaderDbContext _dbContext;
    private readonly DownloaderSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DownloadManager> _logger;
    private readonly ConcurrentQueue<int> _pendingQueue = new();
    private readonly SemaphoreSlim _queueSignal = new(0);
    private CancellationTokenSource? _processCts;
    private Task? _processTask;

    /// <summary>全局状态变化事件</summary>
    public event Action? OnStateChanged;

    public DownloadManager(
        DownloadNotificationClient notificationClient,
        DownloadEngine downloadEngine,
        DownloaderDbContext dbContext,
        DownloaderSettings settings,
        HttpClient httpClient,
        ILogger<DownloadManager> logger)
    {
        _notificationClient = notificationClient;
        _downloadEngine = downloadEngine;
        _dbContext = dbContext;
        _settings = settings;
        _httpClient = httpClient;
        _logger = logger;

        // 订阅事件
        _notificationClient.OnDownloadCompleted += OnNotificationReceived;
        _downloadEngine.OnDownloadCompleted += OnDownloadCompleted;
        _downloadEngine.OnDownloadFailed += OnDownloadFailed;
    }

    /// <summary>
    /// 启动下载管理器
    /// </summary>
    public async Task StartAsync()
    {
        // 确保数据库表存在
        _dbContext.EnsureTablesCreated();

        // 启动队列处理
        _processCts = new CancellationTokenSource();
        _processTask = ProcessQueueAsync(_processCts.Token);

        // 尝试连接 DFApp 后端（失败不阻止启动）
        await TryConnectAsync();

        // 恢复未完成的任务
        await ResumePendingDownloadsAsync();

        _logger.LogInformation("下载管理器已启动");
    }

    /// <summary>
    /// 尝试连接 DFApp 后端
    /// </summary>
    public async Task TryConnectAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.DfAppUrl) ||
                string.IsNullOrEmpty(_settings.DfAppUsername) ||
                string.IsNullOrEmpty(_settings.DfAppPassword))
            {
                _logger.LogWarning("DFApp 后端未配置，请在设置页面配置连接信息");
                return;
            }

            await _notificationClient.LoginAsync(_settings, _httpClient);
            await _notificationClient.StartAsync(_settings);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "连接 DFApp 后端失败，请检查配置或后端服务是否启动");
        }
    }

    /// <summary>
    /// 停止下载管理器
    /// </summary>
    public async Task StopAsync()
    {
        _processCts?.Cancel();
        if (_processTask != null)
        {
            await _processTask;
        }
        await _notificationClient.StopAsync();
        _logger.LogInformation("下载管理器已停止");
    }

    /// <summary>
    /// 处理下载通知
    /// </summary>
    private void OnNotificationReceived(DownloadNotification notification)
    {
        try
        {
            using var db = _dbContext.CreateClient();

            // 检查是否已存在
            var existing = db.Queryable<DownloadItem>()
                .Where(x => x.SourceType == notification.SourceType && x.SourceId == notification.SourceId)
                .First();

            if (existing != null)
            {
                _logger.LogInformation("下载项已存在，跳过: {FileName}", notification.FileName);
                return;
            }

            // 确保下载目录存在
            var downloadPath = Environment.ExpandEnvironmentVariables(_settings.DownloadPath);
            Directory.CreateDirectory(downloadPath);

            // 生成本地路径，处理同名文件
            var localPath = Path.Combine(downloadPath, notification.FileName);
            if (File.Exists(localPath))
            {
                var name = Path.GetFileNameWithoutExtension(notification.FileName);
                var ext = Path.GetExtension(notification.FileName);
                var counter = 1;
                while (File.Exists(localPath))
                {
                    localPath = Path.Combine(downloadPath, $"{name}({counter}){ext}");
                    counter++;
                }
            }

            // 创建下载项
            var item = new DownloadItem
            {
                SourceType = notification.SourceType,
                SourceId = notification.SourceId,
                FileName = notification.FileName,
                FileSize = notification.FileSize,
                DownloadUrl = notification.DownloadUrl,
                LocalPath = localPath,
                Status = DownloadStatus.Pending,
                MimeType = notification.MimeType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (notification is MediaDownloadNotification media)
            {
                item.ChatTitle = media.ChatTitle;
            }

            item.Id = db.Insertable(item).ExecuteReturnIdentity();

            // 加入队列
            _pendingQueue.Enqueue(item.Id);
            _queueSignal.Release();

            _logger.LogInformation("已加入下载队列: {FileName} (ID: {Id})", item.FileName, item.Id);
            OnStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理下载通知失败");
        }
    }

    /// <summary>
    /// 处理下载队列
    /// </summary>
    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _queueSignal.WaitAsync(cancellationToken);

                if (_pendingQueue.TryDequeue(out var itemId))
                {
                    using var db = _dbContext.CreateClient();
                    var item = db.Queryable<DownloadItem>().InSingle(itemId);

                    if (item != null && item.Status == DownloadStatus.Pending)
                    {
                        item.Status = DownloadStatus.Downloading;
                        item.UpdatedAt = DateTime.UtcNow;
                        db.Updateable(item).ExecuteCommand();

                        await _downloadEngine.SubmitDownloadAsync(item);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理下载队列出错");
            }
        }
    }

    /// <summary>
    /// 下载完成回调
    /// </summary>
    private void OnDownloadCompleted(int itemId)
    {
        try
        {
            using var db = _dbContext.CreateClient();
            var item = db.Queryable<DownloadItem>().InSingle(itemId);
            if (item != null)
            {
                item.Status = DownloadStatus.Completed;
                item.DownloadedBytes = item.FileSize;
                item.CompletedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                db.Updateable(item).ExecuteCommand();

                _logger.LogInformation("下载完成: {FileName}", item.FileName);
                OnStateChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新下载完成状态失败");
        }
    }

    /// <summary>
    /// 下载失败回调
    /// </summary>
    private void OnDownloadFailed(int itemId, string errorMessage)
    {
        try
        {
            using var db = _dbContext.CreateClient();
            var item = db.Queryable<DownloadItem>().InSingle(itemId);
            if (item != null)
            {
                item.Status = DownloadStatus.Failed;
                item.ErrorMessage = errorMessage;
                item.UpdatedAt = DateTime.UtcNow;
                db.Updateable(item).ExecuteCommand();

                _logger.LogError("下载失败: {FileName}, 错误: {Error}", item.FileName, errorMessage);
                OnStateChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新下载失败状态失败");
        }
    }

    /// <summary>
    /// 恢复未完成的下载
    /// </summary>
    private async Task ResumePendingDownloadsAsync()
    {
        using var db = _dbContext.CreateClient();
        var pendingItems = db.Queryable<DownloadItem>()
            .Where(x => x.Status == DownloadStatus.Pending || x.Status == DownloadStatus.Downloading)
            .ToList();

        foreach (var item in pendingItems)
        {
            item.Status = DownloadStatus.Pending;
            item.UpdatedAt = DateTime.UtcNow;
            db.Updateable(item).ExecuteCommand();

            _pendingQueue.Enqueue(item.Id);
            _queueSignal.Release();
        }

        if (pendingItems.Count > 0)
        {
            _logger.LogInformation("已恢复 {Count} 个未完成的下载任务", pendingItems.Count);
        }
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public void PauseDownload(int itemId)
    {
        _downloadEngine.PauseDownload(itemId);

        using var db = _dbContext.CreateClient();
        var item = db.Queryable<DownloadItem>().InSingle(itemId);
        if (item != null)
        {
            item.Status = DownloadStatus.Paused;
            item.UpdatedAt = DateTime.UtcNow;
            db.Updateable(item).ExecuteCommand();
        }
    }

    /// <summary>
    /// 恢复下载
    /// </summary>
    public void ResumeDownload(int itemId)
    {
        using var db = _dbContext.CreateClient();
        var item = db.Queryable<DownloadItem>().InSingle(itemId);
        if (item != null)
        {
            item.Status = DownloadStatus.Pending;
            item.UpdatedAt = DateTime.UtcNow;
            db.Updateable(item).ExecuteCommand();

            _pendingQueue.Enqueue(itemId);
            _queueSignal.Release();
        }
    }

    /// <summary>
    /// 取消下载
    /// </summary>
    public void CancelDownload(int itemId)
    {
        _downloadEngine.PauseDownload(itemId);

        using var db = _dbContext.CreateClient();
        var item = db.Queryable<DownloadItem>().InSingle(itemId);
        if (item != null)
        {
            // 删除本地文件（如果是未完成的）
            if (item.Status != DownloadStatus.Completed && File.Exists(item.LocalPath))
            {
                File.Delete(item.LocalPath);
            }

            db.Deleteable<DownloadItem>().In(itemId).ExecuteCommand();
            db.Deleteable<DownloadSegment>().Where(x => x.DownloadItemId == itemId).ExecuteCommand();
        }
    }

    /// <summary>
    /// 获取全局状态
    /// </summary>
    public object GetStatus()
    {
        using var db = _dbContext.CreateClient();
        var pending = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Pending).Count();
        var downloading = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Downloading).Count();
        var completed = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Completed).Count();
        var failed = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Failed).Count();

        return new
        {
            IsConnected = _notificationClient.IsConnected,
            ActiveDownloads = _downloadEngine.ActiveDownloadCount,
            Pending = pending,
            Downloading = downloading,
            Completed = completed,
            Failed = failed
        };
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await _notificationClient.DisposeAsync();
        _httpClient.Dispose();
    }
}
