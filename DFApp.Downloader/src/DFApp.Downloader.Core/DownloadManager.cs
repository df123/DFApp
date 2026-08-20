using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    string? LastError,
    // 已完成任务的累计下载大小（字节）与视频数量
    long TotalDownloadedBytes,
    int VideoCount);

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
    private Task? _speedSamplerTask;
    // 进度持久化节流：记录每个下载项上次写回 DB 的时间，避免高频写库
    private readonly ConcurrentDictionary<int, DateTime> _lastProgressSave = new();
    // 每个下载项最后一次收到进度事件的时间，供卡死看门狗判定（每次事件都更新，不节流）
    private readonly ConcurrentDictionary<int, DateTime> _lastProgressAt = new();
    // 当前各活跃下载项的瞬时速度（字节/秒），由下载库直接给出，GetStatus 时求和得到总速度
    private readonly ConcurrentDictionary<int, double> _activeSpeeds = new();
    // 上次清理过期速度样本的时间（清理低频执行，避免每次采样都删）
    private DateTime _lastSpeedCleanupUtc = DateTime.MinValue;
    // 卡死看门狗：下载中任务超过该时长无任何进度即判定卡死，自动暂停并重新入队
    private static readonly TimeSpan StallTimeout = TimeSpan.FromMinutes(5);

    // 速度采样：全局总速度的记录间隔与样本保留时长（供仪表盘"速度记录"图表查询）
    private static readonly TimeSpan SpeedSampleInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan SpeedSampleRetention = TimeSpan.FromDays(30);

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
        // 启动速度采样：周期记录全局总速度，供仪表盘查看不同时间段的速度情况
        _speedSamplerTask = SpeedSamplerAsync(_processCts.Token);

        // 尝试连接 DFApp 后端（失败不阻止启动）
        await TryConnectAsync();

        // 恢复未完成的任务
        await ResumePendingDownloadsAsync();

        // 补齐历史视频的缩略图（后台执行）
        _ = BackfillThumbnailsAsync();

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
        if (_speedSamplerTask != null)
        {
            await _speedSamplerTask;
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
                item.Message = media.Message;
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

                // 视频下载完成后异步生成缩略图（媒体库展示用）
                if (IsVideo(item.MimeType, item.FileName))
                {
                    _ = GenerateThumbnailAsync(item);
                }

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
    /// 回写后端：标记指定媒体外链已生成（下载器已取回本地）。401（token 过期）时重新登录后重试一次。
    /// </summary>
    private async Task MarkExternalLinkGeneratedAsync(long mediaInfoId)
    {
        try
        {
            for (var attempt = 0; attempt < 2; attempt++)
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
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 0)
                {
                    _logger.LogWarning("回写外链标记 401，重新登录后重试（mediaInfoId={Id}）", mediaInfoId);
                    await _notificationClient.LoginAsync(_settings, _httpClient);
                    continue;
                }

                _logger.LogWarning("回写外链标记失败：HTTP {Status}（mediaInfoId={Id}）", (int)response.StatusCode, mediaInfoId);
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "回写外链标记异常（mediaInfoId={Id}）", mediaInfoId);
        }
    }

    /// <summary>
    /// 删除远程服务器上的物理文件（仅删文件，不删 DB 记录）。401（token 过期）时重新登录后重试一次。
    /// </summary>
    private async Task DeleteRemoteFileAsync(long mediaInfoId)
    {
        try
        {
            for (var attempt = 0; attempt < 2; attempt++)
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
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("已删除远程服务器文件（mediaInfoId={Id}）", mediaInfoId);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized && attempt == 0)
                {
                    _logger.LogWarning("删除远程文件 401，重新登录后重试（mediaInfoId={Id}）", mediaInfoId);
                    await _notificationClient.LoginAsync(_settings, _httpClient);
                    continue;
                }

                _logger.LogWarning("删除远程文件失败：HTTP {Status}（mediaInfoId={Id}）", (int)response.StatusCode, mediaInfoId);
                return;
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
    /// 速度采样循环：周期把全局总速度写入 DownloadSpeedSamples（仅在有活跃下载时写入，
    /// 空闲期无样本即视为 0 速度），并定期清理超过保留时长的旧样本。
    /// </summary>
    private async Task SpeedSamplerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(SpeedSampleInterval, cancellationToken);

                if (_activeSpeeds.IsEmpty)
                {
                    continue;
                }

                using var db = _dbContext.CreateClient();
                db.Insertable(new DownloadSpeedSample
                {
                    RecordedAt = DateTime.UtcNow,
                    SpeedBytesPerSecond = _activeSpeeds.Values.Sum()
                }).ExecuteCommand();

                if ((DateTime.UtcNow - _lastSpeedCleanupUtc).TotalHours >= 1)
                {
                    _lastSpeedCleanupUtc = DateTime.UtcNow;
                    db.Deleteable<DownloadSpeedSample>()
                        .Where(x => x.RecordedAt < DateTime.UtcNow - SpeedSampleRetention)
                        .ExecuteCommand();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "速度采样写入失败");
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
    /// 补漏同步：从 DFApp 后端拉取已下载完成的媒体，与本地比对，把缺失的补入下载队列。
    /// 后端返回全部"已下载未取回"媒体（无增量游标），本地按 SourceType+SourceId 去重——
    /// 用最大 SourceId 作游标会漏掉失败/删除产生的中间空洞。
    /// 对"本地已下载完成但服务器仍标记未取回"的记录（回写 401 失败导致）补一次回写并删除远程文件。
    /// </summary>
    public async Task<(int Scanned, int Added, int Reconciled)> SyncMissedDownloadsAsync()
    {
        var token = _notificationClient.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("未登录 DFApp，无法同步遗漏下载（请先重连）");
        }

        var scanned = 0;
        var added = 0;
        var reconciled = 0;
        var pageIndex = 1;
        const int pageSize = 100;

        // 一次性加载本地已有的 Telegram 记录（用于去重；已完成但回写失败的用于补回写）
        List<DownloadItem> localItems;
        using (var db = _dbContext.CreateClient())
        {
            localItems = db.Queryable<DownloadItem>()
                .Where(x => x.SourceType == "Telegram")
                .ToList();
        }
        var existingIds = localItems.Select(x => x.SourceId).ToHashSet();
        var localBySourceId = localItems.GroupBy(x => x.SourceId).ToDictionary(g => g.Key, g => g.First());

        _logger.LogInformation("补漏同步开始：本地已记录 {Exist} 项", existingIds.Count);

        while (true)
        {
            var url = $"{_settings.DfAppUrl}/api/app/media-info/completed?pageIndex={pageIndex}&pageSize={pageSize}";
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

                if (existingIds.Contains(sourceId))
                {
                    // 本地已有记录：若已下载完成且文件存在但服务器仍标记未取回（回写失败），补一次回写并删除远程文件
                    if (localBySourceId.TryGetValue(sourceId, out var local)
                        && local.Status == DownloadStatus.Completed
                        && File.Exists(local.LocalPath))
                    {
                        reconciled++;
                        await MarkExternalLinkGeneratedAsync(sourceId);
                        await DeleteRemoteFileAsync(sourceId);
                    }
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
                    ChatTitle = item.TryGetProperty("chatTitle", out var ctEl) ? ctEl.GetString() ?? string.Empty : string.Empty,
                    Message = item.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null
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

        _logger.LogInformation("补漏同步完成：扫描 {Scanned} 项，新增 {Added} 项，修复回写 {Reconciled} 项", scanned, added, reconciled);
        return (scanned, added, reconciled);
    }

    /// <summary>
    /// 回填历史记录的聊天标题与消息：按文件名（即 mediaId）从后端查询缺失的 ChatTitle/Message。
    /// 补漏同步只下发未取回的新媒体，历史已完成记录的聊天消息在此补上。
    /// </summary>
    public async Task<int> BackfillGalleryMessagesAsync()
    {
        var token = _notificationClient.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("未登录 DFApp，无法回填聊天消息（请先重连）");
        }

        var updated = 0;
        List<DownloadItem> pendingItems;
        using (var db = _dbContext.CreateClient())
        {
            // 已完成且聊天标题或消息缺失的记录
            pendingItems = db.Queryable<DownloadItem>()
                .Where(x => x.Status == DownloadStatus.Completed
                    && (x.ChatTitle == null || x.ChatTitle == "" || x.Message == null || x.Message == ""))
                .ToList();
        }

        foreach (var item in pendingItems)
        {
            // 文件名（不含扩展名）即远程 mediaId，按此过滤查询
            var mediaId = Path.GetFileNameWithoutExtension(item.FileName);
            if (string.IsNullOrEmpty(mediaId))
            {
                continue;
            }

            var url = $"{_settings.DfAppUrl}/api/app/media-info/paged?filter={Uri.EscapeDataString(mediaId)}&pageIndex=1&pageSize=1";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("data", out var data)
                    || !data.TryGetProperty("items", out var itemsEl)
                    || itemsEl.GetArrayLength() == 0)
                {
                    continue;
                }

                var first = itemsEl[0];
                var chatTitle = first.TryGetProperty("chatTitle", out var ctEl) ? ctEl.GetString() ?? string.Empty : string.Empty;
                var message = first.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null;

                item.ChatTitle = string.IsNullOrEmpty(item.ChatTitle) ? chatTitle : item.ChatTitle;
                item.Message = string.IsNullOrEmpty(item.Message) ? message : item.Message;
                item.UpdatedAt = DateTime.UtcNow;
                using var db = _dbContext.CreateClient();
                db.Updateable(item).ExecuteCommand();
                updated++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "回填聊天消息失败: {FileName}", item.FileName);
            }
        }

        _logger.LogInformation("回填聊天消息完成：更新 {Count} 条", updated);
        return updated;
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
            // 删除本地文件（含已完成）及下载中的 .download 临时文件，避免磁盘残留
            DeleteLocalFiles(item.LocalPath);

            db.Deleteable<DownloadItem>().In(itemId).ExecuteCommand();
            db.Deleteable<DownloadSegment>().Where(x => x.DownloadItemId == itemId).ExecuteCommand();
        }
    }

    /// <summary>
    /// 批量删除失败下载
    /// </summary>
    public int DeleteFailedDownloads()
    {
        using var db = _dbContext.CreateClient();
        var itemIds = db.Queryable<DownloadItem>()
            .Where(x => x.Status == DownloadStatus.Failed)
            .Select(x => x.Id)
            .ToList();

        foreach (var itemId in itemIds)
        {
            CancelDownload(itemId);
        }

        return itemIds.Count;
    }

    /// <summary>
    /// 删除本地目标文件及其 .download 临时文件（文件不存在时静默跳过）
    /// </summary>
    private void DeleteLocalFiles(string localPath)
    {
        foreach (var path in new[] { localPath, localPath + ".download" })
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                // 文件被占用等情况不影响删除任务本身，仅记录日志
                _logger.LogWarning(ex, "删除本地文件失败: {Path}", path);
            }
        }
    }

    /// <summary>缩略图存放目录名（下载目录下）</summary>
    private const string ThumbnailDirName = "thumbs";

    /// <summary>ffmpeg 可执行文件路径（静态构建，媒体库缩略图抽帧用）</summary>
    private readonly string _ffmpegPath = Environment.GetEnvironmentVariable("FFMPEG_PATH")
        ?? "/home/df/ffmpeg/ffmpeg";

    /// <summary>
    /// 判断是否视频：按 MIME 前缀或扩展名
    /// </summary>
    private static bool IsVideo(string? mimeType, string fileName)
    {
        if (!string.IsNullOrEmpty(mimeType) && mimeType.StartsWith("video", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" or ".ts" or ".m4v";
    }

    /// <summary>缩略图完整路径：{下载目录}/.thumbs/{文件名}.jpg</summary>
    private string GetThumbnailPath(string fileName)
    {
        var dir = Path.Combine(_settings.DownloadPath, ThumbnailDirName);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, Path.GetFileNameWithoutExtension(fileName) + ".jpg");
    }

    /// <summary>
    /// 用 ffmpeg 抽取视频某一帧生成缩略图。已存在则跳过；失败仅记日志，不影响下载流程。
    /// </summary>
    public async Task GenerateThumbnailAsync(DownloadItem item)
    {
        try
        {
            if (!IsVideo(item.MimeType, item.FileName) || !File.Exists(item.LocalPath))
            {
                return;
            }

            var thumbPath = GetThumbnailPath(item.FileName);
            if (File.Exists(thumbPath))
            {
                return;
            }

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _ffmpegPath,
                // 抽取距片头 5 秒处一帧，缩放到宽 960（保持比例，比旧版 480 更清晰），避免首帧黑屏
                ArgumentList = {
                    "-y", "-ss", "5", "-i", item.LocalPath,
                    "-frames:v", "1", "-vf", "scale=960:-2", "-q:v", "4",
                    thumbPath
                },
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null)
            {
                _logger.LogWarning("缩略图生成失败：无法启动 ffmpeg: {FfmpegPath}", _ffmpegPath);
                return;
            }
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var err = await process.StandardError.ReadToEndAsync();
                _logger.LogWarning("缩略图生成失败（exit={Exit}）: {File} {Error}",
                    process.ExitCode, item.FileName, err.Trim());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "生成缩略图异常: {File}", item.FileName);
        }
    }

    /// <summary>
    /// 启动时批量补齐已完成视频的缩略图（后台执行，不阻塞启动）
    /// </summary>
    private async Task BackfillThumbnailsAsync()
    {
        try
        {
            using var db = _dbContext.CreateClient();
            var videos = db.Queryable<DownloadItem>()
                .Where(x => x.Status == DownloadStatus.Completed)
                .ToList()
                .Where(x => IsVideo(x.MimeType, x.FileName))
                .ToList();

            foreach (var item in videos)
            {
                await GenerateThumbnailAsync(item);
            }
            _logger.LogInformation("缩略图补齐完成：共检查 {Count} 个视频", videos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "缩略图补齐异常");
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

        // 已完成任务的统计：累计大小按 FileSize 计（进度写回可能滞后），视频按 MimeType 前缀判定
        var completedItems = db.Queryable<DownloadItem>()
            .Where(x => x.Status == DownloadStatus.Completed)
            .Select(x => new { x.FileSize, x.MimeType })
            .ToList();
        var totalDownloadedBytes = completedItems.Sum(x => x.FileSize);
        var videoCount = completedItems.Count(x => x.MimeType?.StartsWith("video", StringComparison.OrdinalIgnoreCase) == true);

        return new DownloaderStatus(
            IsConnected: _notificationClient.IsConnected,
            ActiveDownloads: _downloadEngine.ActiveDownloadCount,
            Pending: pending,
            Downloading: downloading,
            Completed: completed,
            Failed: failed,
            TotalSpeedBytesPerSecond: _activeSpeeds.Values.Sum(),
            LastError: _notificationClient.LastConnectionError,
            TotalDownloadedBytes: totalDownloadedBytes,
            VideoCount: videoCount);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await _notificationClient.DisposeAsync();
        _httpClient.Dispose();
    }
}
