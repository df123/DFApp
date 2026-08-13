using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DFApp.Downloader.Core.Configuration;
using DFApp.Downloader.Core.Entities;
using DFApp.Downloader.Core.Models;
using Downloader;
using Microsoft.Extensions.Logging;

namespace DFApp.Downloader.Core.Engine;

/// <summary>
/// 下载引擎，管理并发下载任务。底层使用 Downloader（bezzad）库完成多分片并行下载，
/// 不再自行实现分片/Range 逻辑。
/// </summary>
public class DownloadEngine
{
    private readonly DownloaderSettings _settings;
    private readonly ILogger<DownloadEngine> _logger;
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeDownloads = new();
    private readonly SemaphoreSlim _concurrencySemaphore;

    /// <summary>下载进度事件</summary>
    public event Action<DownloadProgress>? OnProgress;

    /// <summary>下载开始事件（拿到并发信号量、真正开始下载时触发）</summary>
    public event Action<int>? OnDownloadStarted;

    /// <summary>下载完成事件</summary>
    public event Action<int>? OnDownloadCompleted;

    /// <summary>下载失败事件</summary>
    public event Action<int, string>? OnDownloadFailed;

    public DownloadEngine(DownloaderSettings settings, ILogger<DownloadEngine> logger)
    {
        _settings = settings;
        _logger = logger;
        _concurrencySemaphore = new SemaphoreSlim(settings.MaxConcurrentDownloads);
    }

    /// <summary>
    /// 提交下载任务。立即返回，下载在后台执行；
    /// 并发由 _concurrencySemaphore 控制，避免阻塞队列处理器导致只能串行下载。
    /// </summary>
    public Task SubmitDownloadAsync(DownloadItem item)
    {
        var cts = new CancellationTokenSource();
        _activeDownloads[item.Id] = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await _concurrencySemaphore.WaitAsync(cts.Token);
                await ExecuteDownloadAsync(item, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("下载已取消: {FileName}", item.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载失败: {FileName}", item.FileName);
                OnDownloadFailed?.Invoke(item.Id, ex.Message);
            }
            finally
            {
                _activeDownloads.TryRemove(item.Id, out _);
                _concurrencySemaphore.Release();
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// 暂停下载（取消正在进行的任务；恢复时会重新入队整文件重下）
    /// </summary>
    public void PauseDownload(int itemId)
    {
        if (_activeDownloads.TryGetValue(itemId, out var cts))
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// 执行单个下载任务：基于 Downloader 库，自动多分片并行、断点续传、失败重试。
    /// </summary>
    private async Task ExecuteDownloadAsync(DownloadItem item, CancellationToken cancellationToken)
    {
        // 通知外部：已获得并发槽位，真正开始下载（用于刷新 UpdatedAt，使其在列表中排到前面）
        OnDownloadStarted?.Invoke(item.Id);

        using var service = new DownloadService(BuildConfiguration());

        // 进度：库直接给出已下载字节与瞬时速度
        service.DownloadProgressChanged += (_, e) =>
        {
            OnProgress?.Invoke(new DownloadProgress
            {
                DownloadItemId = item.Id,
                DownloadedBytes = e.ReceivedBytesSize,
                TotalBytes = e.TotalBytesToReceive > 0 ? e.TotalBytesToReceive : item.FileSize,
                SpeedBytesPerSecond = e.BytesPerSecondSpeed
            });
        };

        // 完成结果：库以 DownloadFileCompleted 事件交付成败（任务本身不一定抛异常），用 TCS 桥接以获得确定结果
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler<AsyncCompletedEventArgs> onCompleted = (_, e) =>
        {
            if (e.Cancelled)
            {
                tcs.TrySetCanceled();
            }
            else if (e.Error != null)
            {
                tcs.TrySetException(e.Error);
            }
            else
            {
                tcs.TrySetResult(true);
            }
        };
        service.DownloadFileCompleted += onCompleted;

        try
        {
            await service.DownloadFileTaskAsync(item.DownloadUrl, item.LocalPath, cancellationToken);
            // 以完成事件为准，确认最终结果（成功时立即返回，失败/取消时抛出对应异常）
            await tcs.Task;
            OnDownloadCompleted?.Invoke(item.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("下载已取消: {FileName}", item.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载失败: {FileName}", item.FileName);
            OnDownloadFailed?.Invoke(item.Id, ex.Message);
        }
        finally
        {
            service.DownloadFileCompleted -= onCompleted;
        }
    }

    /// <summary>
    /// 构建下载库配置：多分片并行、Apache Basic Auth、禁用系统代理、失败自动重试。
    /// </summary>
    private DownloadConfiguration BuildConfiguration()
    {
        var chunkCount = Math.Max(1, _settings.MaxSegmentsPerFile);

        var config = new DownloadConfiguration
        {
            ChunkCount = chunkCount,
            ParallelDownload = true,
            ParallelCount = chunkCount,
            MaxTryAgainOnFailure = 3,
            // 禁用系统代理：运行环境存在 http(s)_proxy 指向本地 127.0.0.1:10079，
            // 下载 Cloudflare 直链需绕过，与原注入 HttpClient 的 UseProxy=false 行为一致
            CustomHttpMessageHandlerFactory = () => new SocketsHttpHandler { UseProxy = false }
        };

        // Apache Basic Auth（与原 SegmentDownloader.SetBasicAuth 等价）
        if (!string.IsNullOrEmpty(_settings.ApacheUsername))
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ApacheUsername}:{_settings.ApachePassword}"));
            config.RequestConfiguration.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        return config;
    }

    /// <summary>
    /// 活跃下载数
    /// </summary>
    public int ActiveDownloadCount => _activeDownloads.Count;
}
