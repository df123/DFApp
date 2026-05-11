using System.Collections.Concurrent;
using System.Net.Http.Headers;
using DFApp.Downloader.Core.Configuration;
using DFApp.Downloader.Core.Entities;
using DFApp.Downloader.Core.Models;
using Microsoft.Extensions.Logging;

namespace DFApp.Downloader.Core.Engine;

/// <summary>
/// 分段下载器
/// </summary>
public class SegmentDownloader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public SegmentDownloader(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// 下载单个分片
    /// </summary>
    public async Task<long> DownloadSegmentAsync(
        string url,
        string filePath,
        long startOffset,
        long endOffset,
        long alreadyDownloaded,
        CancellationToken cancellationToken,
        IProgress<long>? progress = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var resumeStart = startOffset + alreadyDownloaded;
        if (resumeStart <= endOffset)
        {
            request.Headers.Range = new RangeHeaderValue(resumeStart, endOffset);
        }

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = alreadyDownloaded;
        var buffer = new byte[81920];

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
        fileStream.Seek(resumeStart, SeekOrigin.Begin);

        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytes += bytesRead;
            progress?.Report(bytesRead);
        }

        return totalBytes - alreadyDownloaded;
    }

    /// <summary>
    /// 检查服务器是否支持 Range 请求
    /// </summary>
    public async Task<bool> CheckRangeSupportAsync(string url)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request);
            return response.Headers.AcceptRanges.Contains("bytes");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    public async Task<long> GetFileSizeAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return response.Content.Headers.ContentLength ?? 0;
    }
}

/// <summary>
/// 下载引擎，管理并发下载任务
/// </summary>
public class DownloadEngine
{
    private readonly DownloaderSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DownloadEngine> _logger;
    private readonly ConcurrentDictionary<long, CancellationTokenSource> _activeDownloads = new();
    private readonly SemaphoreSlim _concurrencySemaphore;

    /// <summary>下载进度事件</summary>
    public event Action<DownloadProgress>? OnProgress;

    /// <summary>下载完成事件</summary>
    public event Action<long>? OnDownloadCompleted;

    /// <summary>下载失败事件</summary>
    public event Action<long, string>? OnDownloadFailed;

    public DownloadEngine(DownloaderSettings settings, HttpClient httpClient, ILogger<DownloadEngine> logger)
    {
        _settings = settings;
        _httpClient = httpClient;
        _logger = logger;
        _concurrencySemaphore = new SemaphoreSlim(settings.MaxConcurrentDownloads);
    }

    /// <summary>
    /// 提交下载任务
    /// </summary>
    public async Task SubmitDownloadAsync(DownloadItem item)
    {
        var cts = new CancellationTokenSource();
        _activeDownloads[item.Id] = cts;

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
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public void PauseDownload(long itemId)
    {
        if (_activeDownloads.TryGetValue(itemId, out var cts))
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// 执行下载
    /// </summary>
    private async Task ExecuteDownloadAsync(DownloadItem item, CancellationToken cancellationToken)
    {
        var downloader = new SegmentDownloader(_httpClient, _logger);

        // 检查是否支持 Range
        var supportsRange = await downloader.CheckRangeSupportAsync(item.DownloadUrl);

        if (!supportsRange || item.FileSize < _settings.SegmentSize)
        {
            // 单线程下载
            await DownloadSingleThreadAsync(item, downloader, cancellationToken);
        }
        else
        {
            // 多线程下载
            await DownloadMultiThreadAsync(item, downloader, cancellationToken);
        }
    }

    /// <summary>
    /// 单线程下载
    /// </summary>
    private async Task DownloadSingleThreadAsync(DownloadItem item, SegmentDownloader downloader, CancellationToken cancellationToken)
    {
        var progress = new Progress<long>(bytes =>
        {
            item.DownloadedBytes += bytes;
            OnProgress?.Invoke(new DownloadProgress
            {
                DownloadItemId = item.Id,
                DownloadedBytes = item.DownloadedBytes,
                TotalBytes = item.FileSize,
                SpeedBytesPerSecond = 0 // 速度由外部计算
            });
        });

        await downloader.DownloadSegmentAsync(
            item.DownloadUrl,
            item.LocalPath,
            0,
            item.FileSize - 1,
            item.DownloadedBytes,
            cancellationToken,
            progress);

        OnDownloadCompleted?.Invoke(item.Id);
    }

    /// <summary>
    /// 多线程下载
    /// </summary>
    private async Task DownloadMultiThreadAsync(DownloadItem item, SegmentDownloader downloader, CancellationToken cancellationToken)
    {
        var segmentSize = _settings.SegmentSize;
        var segmentCount = (int)Math.Ceiling((double)item.FileSize / segmentSize);
        segmentCount = Math.Min(segmentCount, _settings.MaxSegmentsPerFile);

        var tasks = new List<Task<long>>();
        var progressLock = new object();

        for (int i = 0; i < segmentCount; i++)
        {
            var startOffset = i * segmentSize;
            var endOffset = Math.Min((i + 1) * segmentSize - 1, item.FileSize - 1);
            var segmentIndex = i;

            var progress = new Progress<long>(bytes =>
            {
                lock (progressLock)
                {
                    item.DownloadedBytes += bytes;
                    OnProgress?.Invoke(new DownloadProgress
                    {
                        DownloadItemId = item.Id,
                        DownloadedBytes = item.DownloadedBytes,
                        TotalBytes = item.FileSize,
                        SpeedBytesPerSecond = 0
                    });
                }
            });

            tasks.Add(downloader.DownloadSegmentAsync(
                item.DownloadUrl,
                item.LocalPath,
                startOffset,
                endOffset,
                0,
                cancellationToken,
                progress));
        }

        await Task.WhenAll(tasks);
        OnDownloadCompleted?.Invoke(item.Id);
    }

    /// <summary>
    /// 活跃下载数
    /// </summary>
    public int ActiveDownloadCount => _activeDownloads.Count;
}
