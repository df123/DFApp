using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
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
/// 下载器全局状态
/// </summary>
public record DownloaderStatus(
    bool IsConnected,
    int ActiveDownloads,
    int Pending,
    int Downloading,
    int Completed,
    int Failed,
    double TotalSpeedBytesPerSecond,
    string? LastError);

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
    private Task? _watchdogTask;
    // 进度持久化节流：记录每个下载项上次写回 DB 的时间，避免高频写库
    private readonly ConcurrentDictionary<int, DateTime> _lastProgressSave = new();
    // 每个下载项最后一次收到进度事件的时间，供卡死看门狗判定（每次事件都更新，不节流）
    private readonly ConcurrentDictionary<int, DateTime> _lastProgressAt = new();
    // 当前各活跃下载项的瞬时速度（字节/秒），由下载库直接给出，GetStatus 时求和得到总速度
    private readonly ConcurrentDictionary<int, double> _activeSpeeds = new();
    // 卡死看门狗：下载中任务超过该时长无任何进度即判定卡死，自动暂停并重新入队
    private static readonly TimeSpan StallTimeout = TimeSpan.FromMinutes(5);

    // 下载失败最大自动重试次数：重试超过该次数后标记 Failed，需用户手动处理
    private const int MaxDownloadRetries = 3;

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
        _downloadEngine.OnDownloadStarted += OnDownloadStarted;
        _downloadEngine.OnProgress += OnProgressReceived;
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
        // 启动卡死看门狗：自动重启长时间无进度的下载
        _watchdogTask = StallWatchdogAsync(_processCts.Token);

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
        if (_watchdogTask != null)
        {
            await _watchdogTask;
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

                    // 保持 Pending 状态提交到引擎；真正拿到并发槽位开始下载时，
                    // 由 OnDownloadStarted 标记 Downloading——避免大量排队任务虚占"下载中"
                    if (item != null && item.Status == DownloadStatus.Pending)
                    {
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
        ClearSpeedSample(itemId);
        try
        {
            using var db = _dbContext.CreateClient();
            var item = db.Queryable<DownloadItem>().InSingle(itemId);
            if (item != null)
            {
                // 完整性校验：本地文件实际大小必须等于期望大小，否则判定为失败。
                // 防止分片计算错误、服务器截断、磁盘写满等被误判为"下载完成"，
                // 尤其避免向远程回写"已取回"并删除源文件导致数据丢失。
                var actualLength = File.Exists(item.LocalPath) ? new FileInfo(item.LocalPath).Length : -1L;
                if (item.FileSize > 0 && actualLength != item.FileSize)
                {
                    _logger.LogError("下载不完整（期望 {Expect}，实际 {Actual}）: {FileName}",
                        item.FileSize, actualLength, item.FileName);

                    // 校验失败按下载失败处理（重新入队自动重试前删除本地错误文件）
                    TryRequeueOrFail(db, item,
                        $"文件大小不匹配，下载不完整：期望 {item.FileSize} 字节，实际 {actualLength} 字节",
                        deleteLocalFile: true);
                    return;
                }

                item.Status = DownloadStatus.Completed;
                item.DownloadedBytes = item.FileSize;
                item.CompletedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                db.Updateable(item).ExecuteCommand();

                _logger.LogInformation("下载完成: {FileName}", item.FileName);
                OnStateChanged?.Invoke();

                // Telegram 来源：先回写已取回标记（让补漏同步不再返回），再删除远程物理文件释放服务器空间
                if (item.SourceType == "Telegram" && item.SourceId > 0)
                {
                    _ = NotifyRetrievedAndCleanupAsync(item.SourceId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新下载完成状态失败");
        }
    }

    /// <summary>
    /// 下载完成后的远程收尾：先标记已取回本地（让补漏同步不再返回），再删除远程物理文件释放服务器空间。
    /// 串行执行以保证顺序——即使删文件失败，标记已成功则不会重复下载。fire-and-forget，失败仅记日志。
    /// </summary>
    private async Task NotifyRetrievedAndCleanupAsync(long mediaInfoId)
    {
        await MarkExternalLinkGeneratedAsync(mediaInfoId);
        await DeleteRemoteFileAsync(mediaInfoId);
    }

    /// <summary>
    /// 回写后端：标记指定媒体外链已生成（下载器已取回本地）。失败仅记日志。
    /// </summary>
    private async Task MarkExternalLinkGeneratedAsync(long mediaInfoId)
    {
        try
        {
            var token = _notificationClient.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("回写外链标记失败：未登录 DFApp（mediaInfoId={Id}）", mediaInfoId);
                return;
            }

            var url = $"{_settings.DfAppUrl}/api/app/media-info/{mediaInfoId}/mark-external-link-generated";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("回写外链标记失败：HTTP {Status}（mediaInfoId={Id}）", (int)response.StatusCode, mediaInfoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "回写外链标记异常（mediaInfoId={Id}）", mediaInfoId);
        }
    }

    /// <summary>
    /// 删除远程服务器上的物理文件（仅删文件，不删 DB 记录）。失败仅记日志——下次同步命中本地去重不会重复下载。
    /// </summary>
    private async Task DeleteRemoteFileAsync(long mediaInfoId)
    {
        try
        {
            var token = _notificationClient.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("删除远程文件失败：未登录 DFApp（mediaInfoId={Id}）", mediaInfoId);
                return;
            }

            var url = $"{_settings.DfAppUrl}/api/app/media-info/{mediaInfoId}/file";
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("删除远程文件失败：HTTP {Status}（mediaInfoId={Id}）", (int)response.StatusCode, mediaInfoId);
            }
            else
            {
                _logger.LogInformation("已删除远程服务器文件（mediaInfoId={Id}）", mediaInfoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除远程文件异常（mediaInfoId={Id}）", mediaInfoId);
        }
    }

    /// <summary>
    /// 下载开始回调：任务已获得并发槽位真正开始下载，标记 Downloading 状态。
    /// 排队等待的任务仍为 Pending，从而列表中"下载中"只反映真正在下载的任务。
    /// </summary>
    private void OnDownloadStarted(int itemId)
    {
        // 记录开始时间，供卡死看门狗计算无进度时长
        _lastProgressAt[itemId] = DateTime.UtcNow;
        try
        {
            using var db = _dbContext.CreateClient();
            var item = db.Queryable<DownloadItem>().InSingle(itemId);
            if (item != null)
            {
                item.Status = DownloadStatus.Downloading;
                item.UpdatedAt = DateTime.UtcNow;
                db.Updateable(item).ExecuteCommand();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新下载开始状态失败");
        }
    }

    /// <summary>
    /// 下载失败回调：未超过最大重试次数则重新入队自动重试，超过则标记失败等待用户手动处理
    /// </summary>
    private void OnDownloadFailed(int itemId, string errorMessage)
    {
        ClearSpeedSample(itemId);
        try
        {
            using var db = _dbContext.CreateClient();
            var item = db.Queryable<DownloadItem>().InSingle(itemId);
            if (item != null)
            {
                TryRequeueOrFail(db, item, errorMessage, deleteLocalFile: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新下载失败状态失败");
        }
    }

    /// <summary>
    /// 失败重试判定：未超过 MaxDownloadRetries 时重置为 Pending 并重新入队自动重试，
    /// 超过则标记 Failed（需用户手动处理）。deleteLocalFile 为 true 时重新入队前删除本地文件
    /// （用于下载完成但完整性校验失败的场景，避免断点续传基于错误文件继续）。
    /// </summary>
    private void TryRequeueOrFail(ISqlSugarClient db, DownloadItem item, string errorMessage, bool deleteLocalFile)
    {
        if (item.RetryCount < MaxDownloadRetries)
        {
            if (deleteLocalFile)
            {
                try
                {
                    if (File.Exists(item.LocalPath))
                    {
                        File.Delete(item.LocalPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "删除校验失败文件失败: {Path}", item.LocalPath);
                }
            }

            item.RetryCount++;
            item.Status = DownloadStatus.Pending;
            item.ErrorMessage = "";
            item.UpdatedAt = DateTime.UtcNow;
            db.Updateable(item).ExecuteCommand();

            _pendingQueue.Enqueue(item.Id);
            _queueSignal.Release();

            _logger.LogWarning("下载失败，自动重试（第 {RetryCount}/{MaxRetries} 次）: {FileName}, 错误: {Error}",
                item.RetryCount, MaxDownloadRetries, item.FileName, errorMessage);
            OnStateChanged?.Invoke();
        }
        else
        {
            item.Status = DownloadStatus.Failed;
            item.ErrorMessage = errorMessage;
            item.UpdatedAt = DateTime.UtcNow;
            db.Updateable(item).ExecuteCommand();

            _logger.LogError("下载失败（已自动重试 {RetryCount} 次，需手动处理）: {FileName}, 错误: {Error}",
                item.RetryCount, item.FileName, errorMessage);
            OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// 下载进度回调：速度直接取自下载库；并节流写回 DownloadedBytes（每项至多 1s 一次），
    /// 让界面进度条实时更新（此前回调只更新内存速度，DB 的 DownloadedBytes 仅在完成时才写，故界面恒为 0）。
    /// </summary>
    private void OnProgressReceived(DownloadProgress progress)
    {
        var itemId = (int)progress.DownloadItemId;
        var now = DateTime.UtcNow;

        // 速度：直接用下载库给出的瞬时速度
        _activeSpeeds[itemId] = Math.Max(0, progress.SpeedBytesPerSecond);

        // 记录最近进度时间（每次事件都更新，供卡死看门狗判定；有进度即说明连接存活）
        _lastProgressAt[itemId] = now;

        // 节流持久化已下载字节（仅更新这两列，绝不覆盖别处设置的 Status）
        if (_lastProgressSave.TryGetValue(itemId, out var last) && (now - last).TotalSeconds < 1)
        {
            return;
        }
        _lastProgressSave[itemId] = now;

        try
        {
            using var db = _dbContext.CreateClient();
            db.Updateable<DownloadItem>()
                .SetColumns(it => new DownloadItem { DownloadedBytes = progress.DownloadedBytes, UpdatedAt = now })
                .Where(it => it.Id == itemId)
                .ExecuteCommand();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "持久化下载进度失败（itemId={Id}）", itemId);
        }
    }

    /// <summary>
    /// 清除指定下载项的速度与节流记录（下载结束调用）
    /// </summary>
    private void ClearSpeedSample(int itemId)
    {
        _activeSpeeds.TryRemove(itemId, out _);
        _lastProgressSave.TryRemove(itemId, out _);
        _lastProgressAt.TryRemove(itemId, out _);
    }

    /// <summary>
    /// 获取指定下载项的实时速度（字节/秒），非活跃下载返回 0
    /// </summary>
    public double GetItemSpeed(int itemId)
    {
        return _activeSpeeds.GetValueOrDefault(itemId, 0);
    }

    /// <summary>
    /// 卡死看门狗：周期检查"下载中但超过 StallTimeout 无任何进度"的任务（分片连接被静默掐断时
    /// 下载库可能永远挂起），自动暂停、清掉临时文件并重新入队，避免任务永久卡在下载中。
    /// </summary>
    private async Task StallWatchdogAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);

                var now = DateTime.UtcNow;
                using var db = _dbContext.CreateClient();
                var downloading = db.Queryable<DownloadItem>()
                    .Where(x => x.Status == DownloadStatus.Downloading)
                    .ToList();

                foreach (var item in downloading)
                {
                    // 无进度时间戳（例如刚启动尚未收到事件）则跳过本轮
                    if (!_lastProgressAt.TryGetValue(item.Id, out var last))
                    {
                        continue;
                    }

                    var idleMinutes = (now - last).TotalMinutes;
                    if (idleMinutes < StallTimeout.TotalMinutes)
                    {
                        continue;
                    }

                    _logger.LogWarning("下载疑似卡死（已 {IdleMinutes:F1} 分钟无进度），自动重启: {FileName}",
                        idleMinutes, item.FileName);

                    // 停止当前下载并清掉临时文件，从头干净重下
                    _downloadEngine.PauseDownload(item.Id);
                    try
                    {
                        var tmp = item.LocalPath + ".download";
                        if (File.Exists(tmp))
                        {
                            File.Delete(tmp);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除卡死临时文件失败: {Path}", item.LocalPath);
                    }

                    item.Status = DownloadStatus.Pending;
                    item.DownloadedBytes = 0;
                    item.ErrorMessage = "";
                    item.UpdatedAt = DateTime.UtcNow;
                    db.Updateable(item).ExecuteCommand();

                    _pendingQueue.Enqueue(item.Id);
                    _queueSignal.Release();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "卡死看门狗检查出错");
            }
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
    /// 补漏同步：从 DFApp 后端拉取已下载完成的媒体，与本地比对，把缺失的补入下载队列
    /// </summary>
    public async Task<(int Scanned, int Added)> SyncMissedDownloadsAsync()
    {
        var token = _notificationClient.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("未登录 DFApp，无法同步遗漏下载（请先重连）");
        }

        var scanned = 0;
        var added = 0;
        var pageIndex = 1;
        const int pageSize = 100;

        // 一次性加载本地已有的 Telegram SourceId（用于去重）和最大 SourceId（增量游标）
        HashSet<long> existingIds;
        long sinceId;
        using (var db = _dbContext.CreateClient())
        {
            var localIds = db.Queryable<DownloadItem>()
                .Where(x => x.SourceType == "Telegram")
                .Select(x => x.SourceId)
                .ToList();
            existingIds = localIds.ToHashSet();
            sinceId = localIds.Count > 0 ? localIds.Max() : 0L;
        }

        _logger.LogInformation("补漏同步开始：本地已记录 {Exist} 项，增量游标 sinceId={SinceId}", existingIds.Count, sinceId);

        while (true)
        {
            var url = $"{_settings.DfAppUrl}/api/app/media-info/completed?sinceId={sinceId}&pageIndex={pageIndex}&pageSize={pageSize}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            // 后端响应结构：{ success, data: { items, totalCount } }
            if (!doc.RootElement.TryGetProperty("data", out var data))
            {
                break;
            }

            var total = data.TryGetProperty("totalCount", out var tcEl) ? tcEl.GetInt32() : 0;
            if (!data.TryGetProperty("items", out var itemsEl) || itemsEl.GetArrayLength() == 0)
            {
                break;
            }

            foreach (var item in itemsEl.EnumerateArray())
            {
                var sourceId = item.TryGetProperty("sourceId", out var sidEl) ? sidEl.GetInt64() : 0L;
                if (sourceId == 0)
                {
                    continue;
                }

                // 内存比对（与本地去重维度一致：SourceType + SourceId）
                if (existingIds.Contains(sourceId))
                {
                    continue;
                }

                var notification = new MediaDownloadNotification
                {
                    FileName = item.TryGetProperty("fileName", out var fnEl) ? fnEl.GetString() ?? string.Empty : string.Empty,
                    FileSize = item.TryGetProperty("fileSize", out var fsEl) ? fsEl.GetInt64() : 0L,
                    MimeType = item.TryGetProperty("mimeType", out var mtEl) ? mtEl.GetString() ?? string.Empty : string.Empty,
                    DownloadUrl = item.TryGetProperty("downloadUrl", out var duEl) ? duEl.GetString() ?? string.Empty : string.Empty,
                    SourceType = "Telegram",
                    SourceId = sourceId,
                    ChatId = item.TryGetProperty("chatId", out var ciEl) ? ciEl.GetInt64() : 0L,
                    ChatTitle = item.TryGetProperty("chatTitle", out var ctEl) ? ctEl.GetString() ?? string.Empty : string.Empty
                };

                OnNotificationReceived(notification);
                existingIds.Add(sourceId);
                added++;
            }

            scanned = total;
            if (pageIndex * pageSize >= total)
            {
                break;
            }
            pageIndex++;
        }

        _logger.LogInformation("补漏同步完成：扫描 {Scanned} 项，新增 {Added} 项", scanned, added);
        return (scanned, added);
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
    /// 恢复下载（手动恢复视为新一轮尝试，重置自动重试计数）
    /// </summary>
    public void ResumeDownload(int itemId)
    {
        using var db = _dbContext.CreateClient();
        var item = db.Queryable<DownloadItem>().InSingle(itemId);
        if (item != null)
        {
            item.Status = DownloadStatus.Pending;
            item.RetryCount = 0;
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
    public DownloaderStatus GetStatus()
    {
        using var db = _dbContext.CreateClient();
        var pending = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Pending).Count();
        var downloading = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Downloading).Count();
        var completed = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Completed).Count();
        var failed = db.Queryable<DownloadItem>().Where(x => x.Status == DownloadStatus.Failed).Count();

        return new DownloaderStatus(
            IsConnected: _notificationClient.IsConnected,
            ActiveDownloads: _downloadEngine.ActiveDownloadCount,
            Pending: pending,
            Downloading: downloading,
            Completed: completed,
            Failed: failed,
            TotalSpeedBytesPerSecond: _activeSpeeds.Values.Sum(),
            LastError: _notificationClient.LastConnectionError);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await _notificationClient.DisposeAsync();
        _httpClient.Dispose();
    }
}
