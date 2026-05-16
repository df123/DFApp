using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DFApp.Media;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.DTOs.Media;
using DFApp.Web.Hubs;
using DFApp.Web.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TL;

namespace DFApp.Web.Background;

/// <summary>
/// Telegram 监听后台服务，管理 WTelegram 客户端连接、消息监听和媒体下载
/// </summary>
public class ListenTelegramService : BackgroundService
{
    /// <summary>
    /// 模块名称，用于查询配置
    /// </summary>
    private const string ModuleName = "DFApp.Web.Background.ListenTelegramService";

    private WTelegram.Client? _client;

    /// <summary>
    /// 媒体下载队列
    /// </summary>
    private readonly ConcurrentQueue<MediaQueueModel> _mediaQueue = new();
    private readonly SemaphoreSlim _mediaSignal = new(0);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ListenTelegramService> _logger;

    /// <summary>
    /// 需要用户配置的参数名称（null 表示已连接）
    /// </summary>
    public string? ConfigNeeded { get; private set; } = "start";

    /// <summary>
    /// 已连接的用户信息
    /// </summary>
    public TL.User? User => _client?.User;

    /// <summary>
    /// Telegram 客户端实例
    /// </summary>
    public WTelegram.Client? TGClinet => _client;

    public ListenTelegramService(
        IServiceProvider serviceProvider,
        ILogger<ListenTelegramService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram 监听服务启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await InitializeAndLoginAsync(stoppingToken);

                if (ConfigNeeded != null)
                {
                    _logger.LogInformation("Telegram 需要输入: {ConfigNeeded}，等待用户输入", ConfigNeeded);
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                // 登录成功后启动媒体下载任务
                var downloadTask = DownloadMediaAsync(stoppingToken);

                // 保持运行，等待取消
                while (!stoppingToken.IsCancellationRequested && _client != null)
                {
                    await Task.Delay(5000, stoppingToken);
                }

                await downloadTask;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telegram 监听服务出错，10秒后重试");
                await Task.Delay(10000, stoppingToken);
            }
        }

        _logger.LogInformation("Telegram 监听服务停止");
    }

    /// <summary>
    /// 初始化客户端并执行登录
    /// </summary>
    private async Task InitializeAndLoginAsync(CancellationToken stoppingToken)
    {
        // 确保保存目录存在
        string saveVideoPath = await GetConfigurationInfoAsync("SaveVideoPathPrefix");
        string savePhotoPath = await GetConfigurationInfoAsync("SavePhotoPathPrefix");
        if (!string.IsNullOrEmpty(saveVideoPath)) Directory.CreateDirectory(saveVideoPath);
        if (!string.IsNullOrEmpty(savePhotoPath)) Directory.CreateDirectory(savePhotoPath);

        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }

        WTelegram.Helpers.Log = (lvl, str) => _logger.Log((LogLevel)lvl, "{Message}", str);
        _client = new WTelegram.Client(what =>
        {
            switch (what)
            {
                case "api_id":
                case "session_pathname":
                case "api_hash": return GetConfigurationInfoAsync(what).Result;
                default: return null;
            }
        });

        _client.PingInterval = 300;
        _client.MaxAutoReconnects = int.MaxValue;

        // 注册消息更新回调
        _client.OnUpdates += ClientUpdate;

        // 代理配置
        if (bool.TryParse(await GetConfigurationInfoAsync("EnableProxy"), out var enableProxy) && enableProxy)
        {
            string proxyHost = await GetConfigurationInfoAsync("ProxyHost") ?? "";
            if (int.TryParse(await GetConfigurationInfoAsync("ProxyPort"), out var proxyPort) && proxyPort > 0)
            {
                _logger.LogInformation("启用 SOCKS5 代理: {Host}:{Port}", proxyHost, proxyPort);
                _client.TcpHandler = (address, port) =>
                {
                    return ConnectViaSocks5ProxyAsync(proxyHost, proxyPort, address, port);
                };
            }
        }

        // 执行登录
        string? phoneNumber = await GetConfigurationInfoAsync("phone_number");
        ConfigNeeded = await DoLogin(phoneNumber ?? "");
        if (ConfigNeeded == null)
        {
            _logger.LogInformation("Telegram 登录成功: {User}", User);
        }
        else
        {
            _logger.LogInformation("Telegram 需要输入: {ConfigNeeded}", ConfigNeeded);
        }
    }

    /// <summary>
    /// 处理登录步骤
    /// </summary>
    /// <param name="value">用户输入（手机号、验证码、密码等）</param>
    /// <returns>null 表示登录完成，否则返回需要输入的参数名</returns>
    public async Task<string?> DoLogin(string value)
    {
        if (_client != null)
        {
            return ConfigNeeded = await _client.Login(value);
        }
        return "start";
    }

    /// <summary>
    /// 获取配置信息
    /// </summary>
    private async Task<string> GetConfigurationInfoAsync(string configurationName)
    {
        using var scope = _serviceProvider.CreateScope();
        var configRepo = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
        return await configRepo.GetConfigurationInfoValue(configurationName, ModuleName);
    }

    /// <summary>
    /// 通过 SOCKS5 代理建立 TCP 连接
    /// </summary>
    private static async Task<TcpClient> ConnectViaSocks5ProxyAsync(string proxyHost, int proxyPort, string targetHost, int targetPort)
    {
        var tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(proxyHost, proxyPort);
        var stream = tcpClient.GetStream();

        // SOCKS5 握手：发送无需认证方式
        stream.WriteByte(0x05); // SOCKS 版本
        stream.WriteByte(0x01); // 1 个认证方法
        stream.WriteByte(0x00); // 无需认证
        await stream.FlushAsync();

        // 读取服务器响应
        var response = new byte[2];
        await stream.ReadAsync(response, 0, 2);
        if (response[0] != 0x05 || response[1] != 0x00)
        {
            throw new IOException($"SOCKS5 代理认证失败，响应: {response[0]:X2} {response[1]:X2}");
        }

        // 发送连接请求（域名类型）
        var hostBytes = Encoding.ASCII.GetBytes(targetHost);
        stream.WriteByte(0x05); // SOCKS 版本
        stream.WriteByte(0x01); // CONNECT 命令
        stream.WriteByte(0x00); // 保留字段
        stream.WriteByte(0x03); // 地址类型：域名
        stream.WriteByte((byte)hostBytes.Length);
        await stream.WriteAsync(hostBytes, 0, hostBytes.Length);
        await stream.WriteAsync(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)targetPort)), 0, 2);
        await stream.FlushAsync();

        // 读取连接响应（至少10字节）
        var connectResponse = new byte[10];
        int bytesRead = 0;
        while (bytesRead < connectResponse.Length)
        {
            int read = await stream.ReadAsync(connectResponse, bytesRead, connectResponse.Length - bytesRead);
            if (read == 0) break;
            bytesRead += read;
        }

        if (connectResponse[1] != 0x00)
        {
            throw new IOException($"SOCKS5 代理连接目标失败，状态码: {connectResponse[1]:X2}");
        }

        return tcpClient;
    }

    #region 消息监听

    /// <summary>
    /// Telegram 客户端更新回调，处理新消息中的媒体文件
    /// </summary>
    private async Task ClientUpdate(IObject arg)
    {
        if (arg is not Updates updates) return;

        var updateArray = updates.UpdateList;
        var chats = updates.chats;
        long chatId = long.MaxValue;
        string chatTitle = "NoChatTitle";
        if (chats.Count > 0)
        {
            chatId = chats.First().Value.ID;
            chatTitle = chats.First().Value.Title;
        }

        // 检查是否为忽略的频道
        var ignoredChatIds = await GetConfigurationInfoAsync("IgnoredChatIds");
        if (ignoredChatIds.Contains(chatId.ToString()))
        {
            return;
        }

        foreach (Update update in updateArray)
        {
            if (update is not UpdateNewMessage { message: Message message })
            {
                continue;
            }

            // 检查是否为忽略的消息内容
            var ignoredMessages = await GetConfigurationInfoAsync("IgnoredMessages");
            var ignoredMessagesArrays = ignoredMessages.Split(";", StringSplitOptions.RemoveEmptyEntries);
            bool isIgnored = false;
            foreach (var ignoredMessage in ignoredMessagesArrays)
            {
                if (message.message.Contains(ignoredMessage))
                {
                    isIgnored = true;
                    break;
                }
            }
            if (isIgnored)
            {
                continue;
            }

            await ProcessVideoMessage(message, chatId, chatTitle);
            await ProcessPhotoMessage(message, chatId, chatTitle);
        }
    }

    /// <summary>
    /// 处理视频消息，检查条件后将视频加入下载队列
    /// </summary>
    private async Task ProcessVideoMessage(Message message, long chatId, string chatTitle)
    {
        if (message.media is not MessageMediaDocument { document: Document document })
        {
            return;
        }

        int slash = document.mime_type.IndexOf('/');
        if (slash < 0)
        {
            return;
        }

        // 只处理视频类型
        if (!document.mime_type.Contains("video"))
        {
            return;
        }

        // 从配置读取过滤参数
        double duration = double.Parse(await GetConfigurationInfoAsync("VideoDurationLimit"));
        double maxDuration = double.Parse(await GetConfigurationInfoAsync("VideoDurationMaxLimit"));
        int minWidth = int.Parse(await GetConfigurationInfoAsync("MinVideoWidth"));
        int minHeight = int.Parse(await GetConfigurationInfoAsync("MinVideoHeight"));
        bool isDurationLimit = false;
        bool isVideo = false;
        bool isQualityEnough = false;

        foreach (var attribute in document.attributes)
        {
            if (attribute is DocumentAttributeVideo video)
            {
                if (video.duration <= duration)
                {
                    isDurationLimit = true;
                }
                if (video.duration > maxDuration)
                {
                    isDurationLimit = true;
                }
                isVideo = true;
                if (video.w >= minWidth && video.h >= minHeight)
                {
                    isQualityEnough = true;
                }
            }
        }

        if (isDurationLimit || !isVideo || !isQualityEnough)
        {
            return;
        }

        // 检查是否已存在（去重）
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
            var isExist = await repository.GetFirstOrDefaultAsync(x => x.MediaId == document.id);
            if (isExist != null)
            {
                return;
            }
        }

        string titleDirectory = Path.Combine(await GetConfigurationInfoAsync("SaveVideoPathPrefix"), chatId.ToString());
        if (!Directory.Exists(titleDirectory))
        {
            Directory.CreateDirectory(titleDirectory);
        }
        string fileName = Path.Combine(titleDirectory, $"{document.id}.{document.mime_type[(slash + 1)..]}");

        // 写入数据库记录
        MediaInfo? canAdd = null;
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
            var mediaInfo = new MediaInfo
            {
                MediaId = document.id,
                ChatId = chatId,
                ChatTitle = chatTitle,
                Message = message.message,
                SavePath = fileName,
                Size = document.size,
                MimeType = document.mime_type,
                IsExternalLinkGenerated = false,
                IsDownloadCompleted = false,
                ConcurrencyStamp = Guid.NewGuid().ToString("N")
            };
            await repository.InsertAsync(mediaInfo);
            canAdd = await repository.GetFirstOrDefaultAsync(x => x.MediaId == document.id);
        }

        if (canAdd != null)
        {
            EnqueueMedia(new MediaQueueModel
            {
                MediaInfos = canAdd,
                TObject = document,
                IsPhoto = false
            });
        }
    }

    /// <summary>
    /// 处理图片消息，检查条件后将图片加入下载队列
    /// </summary>
    private async Task ProcessPhotoMessage(Message message, long chatId, string chatTitle)
    {
        if (message.media is not MessageMediaPhoto { photo: Photo photo })
        {
            return;
        }

        // 检查是否已存在（去重）
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
            var isExist = await repository.GetFirstOrDefaultAsync(x => x.MediaId == photo.id);
            if (isExist != null)
            {
                return;
            }
        }

        string titleDirectory = Path.Combine(await GetConfigurationInfoAsync("SavePhotoPathPrefix"), chatId.ToString());
        if (!Directory.Exists(titleDirectory))
        {
            Directory.CreateDirectory(titleDirectory);
        }
        string fileName = Path.Combine(titleDirectory, $"{photo.id}.jpg");

        // 写入数据库记录
        MediaInfo? canAdd = null;
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
            var mediaInfo = new MediaInfo
            {
                MediaId = photo.id,
                ChatId = chatId,
                ChatTitle = chatTitle,
                Message = message.message,
                SavePath = fileName,
                Size = photo.LargestPhotoSize.FileSize,
                MimeType = "JPG",
                IsExternalLinkGenerated = false,
                IsDownloadCompleted = false,
                ConcurrencyStamp = Guid.NewGuid().ToString("N")
            };
            await repository.InsertAsync(mediaInfo);
            canAdd = await repository.GetFirstOrDefaultAsync(x => x.MediaId == photo.id);
        }

        if (canAdd != null)
        {
            EnqueueMedia(new MediaQueueModel
            {
                MediaInfos = canAdd,
                TObject = photo,
                IsPhoto = true
            });
        }
    }

    /// <summary>
    /// 将媒体项加入下载队列
    /// </summary>
    private void EnqueueMedia(MediaQueueModel model)
    {
        if (model == null) return;
        _mediaQueue.Enqueue(model);
        _mediaSignal.Release();
    }

    /// <summary>
    /// 从下载队列中取出媒体项（阻塞等待）
    /// </summary>
    private async Task<MediaQueueModel?> DequeueMediaAsync(CancellationToken cancellationToken)
    {
        await _mediaSignal.WaitAsync(cancellationToken);
        if (_mediaQueue.TryDequeue(out var item))
        {
            return item;
        }
        return null;
    }

    #endregion

    #region 媒体下载

    /// <summary>
    /// 媒体下载主循环，从队列中取出任务并逐个下载
    /// </summary>
    private async Task DownloadMediaAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var model = await DequeueMediaAsync(stoppingToken);
                if (model?.MediaInfos == null || model.TObject == null)
                {
                    continue;
                }

                var mediaInfo = model.MediaInfos;
                long fileSize = model.IsPhoto
                    ? ((Photo)model.TObject).LargestPhotoSize.FileSize
                    : ((Document)model.TObject).size;

                // 检查磁盘空间是否充足
                if (await IsSpaceUpperLimitAsync(fileSize))
                {
                    var isLoopDownload = bool.Parse(await GetConfigurationInfoAsync("IsLoopDownload"));
                    if (!isLoopDownload)
                    {
                        // 非循环下载模式：空间不足时直接删除记录
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
                            await repository.DeleteAsync(mediaInfo.Id);
                        }
                        continue;
                    }
                    else
                    {
                        // 循环下载模式：删除旧文件腾出空间
                        await DeleteOldestMediaUntilSpaceAvailableAsync(fileSize);
                    }
                }

                // 检查日下载流量限制
                await CheckBandwidthLimitAsync();

                // 清理临时文件
                string basePath = model.IsPhoto
                    ? await GetConfigurationInfoAsync("SavePhotoPathPrefix")
                    : await GetConfigurationInfoAsync("SaveVideoPathPrefix");
                DeleteTempFiles(basePath);

                // 执行下载并计时
                var stopwatch = Stopwatch.StartNew();
                if (model.IsPhoto)
                {
                    using var fileStream = File.Create(mediaInfo.SavePath);
                    await _client!.DownloadFileAsync((Photo)model.TObject, fileStream);
                    fileStream.Close();
                }
                else
                {
                    // 视频使用临时文件下载，完成后重命名
                    string fileNameTemp = $"{mediaInfo.SavePath}.temp";
                    using var fileStream = File.Create(fileNameTemp);
                    await _client!.DownloadFileAsync((Document)model.TObject, fileStream);
                    fileStream.Close();
                    File.Move(fileNameTemp, mediaInfo.SavePath, true);
                }
                stopwatch.Stop();

                // 计算下载速度
                long downloadTimeMs = stopwatch.ElapsedMilliseconds;
                double downloadSpeedBps = downloadTimeMs > 0 ? (fileSize * 1000.0 / downloadTimeMs) : 0;

                // 更新下载统计信息
                await UpdateDownloadStatsAsync(mediaInfo.Id, downloadTimeMs, downloadSpeedBps);
                await UpdateIsDownloadCompletedAsync(mediaInfo.Id);

                // 向 Downloader 子程序推送下载完成通知
                await NotifyDownloaderAsync(mediaInfo);

                double speedMBps = StorageUnitConversionHelper.ByteToMB(fileSize) / (downloadTimeMs / 1000.0);
                _logger.LogInformation("{MediaType} 下载完成 {SavePath}, 耗时: {Time}ms, 速度: {Speed:F2} MB/s ({Bps:F0} Bps)",
                    model.IsPhoto ? "图片" : "视频", mediaInfo.SavePath, downloadTimeMs, speedMBps, downloadSpeedBps);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "媒体下载出错: {Message}", ex.Message);
            }
        }
    }

    /// <summary>
    /// 更新媒体下载完成状态
    /// </summary>
    private async Task UpdateIsDownloadCompletedAsync(long id)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
        var mediaInfo = await repository.GetByIdAsync(id);
        if (mediaInfo != null)
        {
            mediaInfo.IsDownloadCompleted = true;
            await repository.UpdateAsync(mediaInfo);
        }
    }

    /// <summary>
    /// 更新媒体下载统计信息（耗时和速度）
    /// </summary>
    private async Task UpdateDownloadStatsAsync(long id, long downloadTimeMs, double downloadSpeedBps)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
        var mediaInfo = await repository.GetByIdAsync(id);
        if (mediaInfo != null)
        {
            mediaInfo.DownloadTimeMs = downloadTimeMs;
            mediaInfo.DownloadSpeedBps = downloadSpeedBps;
            await repository.UpdateAsync(mediaInfo);
        }
    }

    /// <summary>
    /// 向 Downloader 子程序推送下载完成通知
    /// </summary>
    private async Task NotifyDownloaderAsync(MediaInfo mediaInfo)
    {
        try
        {
            var downloaderEnabled = await GetConfigurationInfoAsync("DownloaderEnabled");
            if (!bool.TryParse(downloaderEnabled, out var enabled) || !enabled)
            {
                return;
            }

            var returnDownloadUrlPrefix = await GetConfigurationInfoAsync("ReturnDownloadUrlPrefix");
            var replaceUrlPrefix = await GetConfigurationInfoAsync("ReplaceUrlPrefix");

            if (string.IsNullOrWhiteSpace(returnDownloadUrlPrefix) || string.IsNullOrWhiteSpace(replaceUrlPrefix))
            {
                return;
            }

            var downloadUrl = $"{Path.Combine(returnDownloadUrlPrefix,
                mediaInfo.SavePath.Replace(replaceUrlPrefix, string.Empty).Replace("\\", "/"))}";

            using var scope = _serviceProvider.CreateScope();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DownloadNotificationHub>>();

            var notification = new MediaDownloadNotificationDto
            {
                FileName = Path.GetFileName(mediaInfo.SavePath),
                FileSize = mediaInfo.Size,
                MimeType = mediaInfo.MimeType,
                DownloadUrl = downloadUrl,
                SourceType = "Telegram",
                SourceId = mediaInfo.Id,
                ChatId = mediaInfo.ChatId,
                ChatTitle = mediaInfo.ChatTitle,
                CompletedAt = DateTime.UtcNow
            };

            await hubContext.Clients.Group("DownloadNotify")
                .SendAsync("DownloadCompleted", notification);

            _logger.LogInformation("已推送下载完成通知到 Downloader: {FileName}", notification.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "推送下载完成通知失败，不影响主流程");
        }
    }

    #endregion

    #region 磁盘空间管理

    /// <summary>
    /// 检查磁盘剩余空间是否足够下载指定大小的文件
    /// </summary>
    /// <returns>true 表示空间不足</returns>
    private async Task<bool> IsSpaceUpperLimitAsync(long fileSize)
    {
        double availableFreeSpace = double.Parse(await GetConfigurationInfoAsync("AvailableFreeSpace"));
        string driveName = await GetConfigurationInfoAsync("SaveDrive");
        var driveAvailableMB = SpaceHelper.GetDriveAvailableMB(driveName);
        var fileSizeMB = StorageUnitConversionHelper.ByteToMB(fileSize);

        if ((driveAvailableMB - fileSizeMB) < availableFreeSpace)
        {
            _logger.LogDebug("{Time} 磁盘空间不足，暂停下载", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await Task.Delay(1000);
            return true;
        }

        _logger.LogDebug("{Time} 磁盘空间充足，开始下载", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        return false;
    }

    /// <summary>
    /// 删除指定目录下的所有 .temp 临时文件
    /// </summary>
    private void DeleteTempFiles(string path)
    {
        if (Directory.Exists(path))
        {
            SpaceHelper.DeleteTempFiles(path);
            _logger.LogInformation("已清理所有 .temp 临时文件");
        }
        else
        {
            _logger.LogError("目录 {Path} 不存在", path);
        }
    }

    /// <summary>
    /// 计算今日已下载的文件总大小（MB）
    /// </summary>
    private async Task<double> CalculationDownloadsSizeAsync()
    {
        DateTime todayAtZero = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        DateTime tomorrowAtZero = todayAtZero.AddDays(1);

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();
        var result = await repository.GetListAsync(x =>
            x.IsDownloadCompleted &&
            x.CreationTime >= todayAtZero &&
            x.CreationTime < tomorrowAtZero);
        long totalSize = result.Sum(x => x.Size);
        return StorageUnitConversionHelper.ByteToMB(totalSize);
    }

    /// <summary>
    /// 检查日下载流量限制，超过限制则等待到次日
    /// </summary>
    private async Task CheckBandwidthLimitAsync()
    {
        long bandwidth = long.Parse(await GetConfigurationInfoAsync("Bandwidth"));
        double sizes = await CalculationDownloadsSizeAsync();
        _logger.LogInformation("{Time} 今日已下载: {Size}MB", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sizes);

        if (sizes > bandwidth)
        {
            _logger.LogInformation("{Time} 下载流量已达上限，暂停下载等待次日", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var untilTomorrow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).AddDays(1) - DateTime.Now;
            await Task.Delay(untilTomorrow);
            _logger.LogInformation("{Time} 新的一天，重新开始下载", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    /// <summary>
    /// 循环删除旧媒体文件直到磁盘空间足够
    /// </summary>
    private async Task DeleteOldestMediaUntilSpaceAvailableAsync(long requiredSpace)
    {
        while (await IsSpaceUpperLimitAsync(requiredSpace))
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<MediaInfo, long>>();

            try
            {
                var queryable = await repository.GetListAsync(x => x.IsDownloadCompleted && !x.IsExternalLinkGenerated);

                if (queryable.Count == 0)
                {
                    _logger.LogWarning("没有可删除的媒体文件");
                    break;
                }

                // 检查是否启用循环下载
                var isLoopDownload = bool.Parse(await GetConfigurationInfoAsync("IsLoopDownload"));
                MediaInfo? mediaToDelete;

                if (isLoopDownload)
                {
                    _logger.LogInformation("循环下载已启用，使用时间密度算法删除视频");
                    mediaToDelete = GetHighDensityMediaToDelete(queryable);
                }
                else
                {
                    _logger.LogInformation("循环下载未启用，删除最旧的文件");
                    mediaToDelete = queryable
                        .OrderBy(x => x.CreationTime)
                        .FirstOrDefault();
                }

                if (mediaToDelete == null)
                {
                    _logger.LogWarning("未找到可删除的媒体文件");
                    break;
                }

                // 删除物理文件
                SpaceHelper.DeleteFile(mediaToDelete.SavePath);

                // 标记为已删除（通过 IsExternalLinkGenerated 标记）
                mediaToDelete.IsExternalLinkGenerated = true;
                await repository.UpdateAsync(mediaToDelete);
                _logger.LogInformation("已删除媒体文件: {Path}, 大小: {Size} bytes, 创建时间: {Time}",
                    mediaToDelete.SavePath, mediaToDelete.Size, mediaToDelete.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除旧媒体文件失败");
                break;
            }
        }
    }

    /// <summary>
    /// 时间密度删除算法：优先删除同一时间窗口内聚集的旧文件
    /// 时间密度高的定义：同一时间窗口内获取到的多个已下载完成的视频
    /// </summary>
    /// <param name="mediaList">已下载完成的媒体列表</param>
    /// <returns>需要删除的媒体文件</returns>
    private MediaInfo? GetHighDensityMediaToDelete(List<MediaInfo> mediaList)
    {
        if (mediaList == null || mediaList.Count == 0)
        {
            _logger.LogWarning("媒体列表为空，无法执行时间密度删除算法");
            return null;
        }

        // 从配置中获取时间窗口大小（分钟），默认为2分钟
        int timeWindowMinutes = 2;
        try
        {
            var configValue = GetConfigurationInfoAsync("TimeDensityWindowMinutes").GetAwaiter().GetResult();
            timeWindowMinutes = int.Parse(configValue);
            _logger.LogInformation("从配置中读取时间密度窗口大小: {Minutes} 分钟", timeWindowMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("无法从配置中读取时间密度窗口大小，使用默认值 2 分钟。错误: {Error}", ex.Message);
        }

        _logger.LogInformation("开始执行时间密度删除算法，共有 {Count} 个已下载完成的媒体文件，时间窗口: {Minutes} 分钟",
            mediaList.Count, timeWindowMinutes);

        // 按创建时间分组，计算每个时间窗口的媒体数量
        var mediaGroups = mediaList
            .GroupBy(x =>
            {
                var time = x.CreationTime;
                int timeBlock = time.Minute / timeWindowMinutes;
                return new
                {
                    Year = time.Year,
                    Month = time.Month,
                    Day = time.Day,
                    Hour = time.Hour,
                    TimeBlock = timeBlock
                };
            })
            .Select(g => new
            {
                TimeKey = g.Key,
                Count = g.Count(),
                Medias = g.ToList(),
                StartMinute = g.Key.TimeBlock * timeWindowMinutes,
                EndMinute = Math.Min((g.Key.TimeBlock * timeWindowMinutes) + timeWindowMinutes - 1, 59)
            })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.TimeKey.Year)
            .ThenBy(g => g.TimeKey.Month)
            .ThenBy(g => g.TimeKey.Day)
            .ThenBy(g => g.TimeKey.Hour)
            .ThenBy(g => g.TimeKey.TimeBlock)
            .ToList();

        _logger.LogInformation("时间分组完成，共分为 {Count} 个时间组（按每 {Minutes} 分钟分组）",
            mediaGroups.Count, timeWindowMinutes);

        // 记录每个时间组的详细信息（只记录前5个）
        foreach (var group in mediaGroups.Take(5))
        {
            _logger.LogInformation(
                "时间组 {Year}-{Month:D2}-{Day:D2} {Hour:D2}:{Start:D2}-{End:D2} 包含 {Count} 个已下载完成的文件",
                group.TimeKey.Year, group.TimeKey.Month, group.TimeKey.Day,
                group.TimeKey.Hour, group.StartMinute, group.EndMinute, group.Count);
        }

        // 没有高密度分组时，删除最旧的文件
        if (mediaGroups.Count == 0 || mediaGroups.All(g => g.Count == 1))
        {
            _logger.LogInformation("未找到高密度时间组，删除最旧的文件");
            var oldestFile = mediaList.OrderBy(x => x.CreationTime).FirstOrDefault();
            if (oldestFile != null)
            {
                _logger.LogInformation("选择删除最旧的文件: {Path}, 创建时间: {Time}",
                    oldestFile.SavePath, oldestFile.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            return oldestFile;
        }

        // 找到第一个数量大于1的分组（时间密度最高的分组）
        var highDensityGroup = mediaGroups.FirstOrDefault(g => g.Count > 1);
        if (highDensityGroup != null)
        {
            _logger.LogInformation(
                "找到高密度时间组: {Year}-{Month:D2}-{Day:D2} {Hour:D2}:{Start:D2}-{End:D2}, 包含 {Count} 个文件",
                highDensityGroup.TimeKey.Year, highDensityGroup.TimeKey.Month, highDensityGroup.TimeKey.Day,
                highDensityGroup.TimeKey.Hour, highDensityGroup.StartMinute, highDensityGroup.EndMinute,
                highDensityGroup.Count);

            // 在高密度分组中删除最旧的文件
            var oldestInHighDensity = highDensityGroup.Medias.OrderBy(x => x.CreationTime).FirstOrDefault();
            if (oldestInHighDensity != null)
            {
                _logger.LogInformation("在高密度时间组中选择删除最旧的文件: {Path}, 创建时间: {Time}",
                    oldestInHighDensity.SavePath, oldestInHighDensity.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            return oldestInHighDensity;
        }

        // 兜底：删除最旧的文件
        _logger.LogInformation("未找到高密度时间组，删除最旧的文件");
        var defaultOldestFile = mediaList.OrderBy(x => x.CreationTime).FirstOrDefault();
        if (defaultOldestFile != null)
        {
            _logger.LogInformation("选择删除最旧的文件: {Path}, 创建时间: {Time}",
                defaultOldestFile.SavePath, defaultOldestFile.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        return defaultOldestFile;
    }

    #endregion

    public override void Dispose()
    {
        _client?.Dispose();
        base.Dispose();
    }
}
