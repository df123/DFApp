using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Media;
using DFApp.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Starksoft.Net.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace DFApp.Background
{
    public class ListenTelegramService : BackgroundService
    {
        private WTelegram.Client? _client;
        public WTelegram.Client? TGClinet { get { return _client; } }
        public User? User => TGClinet?.User;
        public string ConfigNeeded { get; set; } = "start";


        private IQueueBase<MediaQueueModel> _mediaQueue;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScope _serviceScope;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly IRepository<MediaInfo, long> _mediaInfoRepository;


        private ILogger Logger => _loggerFactory.CreateLogger(GetType().FullName!) ?? NullLogger.Instance;


        public ListenTelegramService(IServiceScopeFactory serviceScopeFactory)
        {
            _mediaQueue = new QueueBase<MediaQueueModel>();

            _serviceScopeFactory = serviceScopeFactory;

            _serviceScope = _serviceScopeFactory.CreateScope();
            _loggerFactory = _serviceScope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _unitOfWorkManager = _serviceScope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            _configurationInfoRepository = _serviceScope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
            _mediaInfoRepository = _serviceScope.ServiceProvider.GetRequiredService<IRepository<MediaInfo, long>>();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartWork(stoppingToken).ConfigureAwait(false);
        }

        public async Task<string> DoLogin(string loginInfo)
        {
            if (TGClinet != null)
            {
                return ConfigNeeded = await TGClinet.Login(loginInfo);
            }
            else
            {
                return "start";
            }
        }

        public async Task<string> GetConfigurationInfo(string configurationName)
        {
            using (_configurationInfoRepository.DisableTracking())
            {
                string v = await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, ListenTelegramConst.ModuleName);
                return v;
            }
        }

        public async Task StartWork(CancellationToken stoppingToken)
        {
            try
            {
                Logger.LogInformation("Start listening for messages");

                Directory.CreateDirectory(await GetConfigurationInfo("SaveVideoPathPrefix"));
                Directory.CreateDirectory(await GetConfigurationInfo("SavePhotoPathPrefix"));

                if (_client != null)
                {
                    _client.Dispose();
                }
                else
                {
                    WTelegram.Helpers.Log = (lvl, str) => Logger.Log((LogLevel)lvl, str);
                    _client = new WTelegram.Client(what =>
                    {
                        switch (what)
                        {
                            case "api_id":
                            case "session_pathname":
                            case "api_hash": return GetConfigurationInfo(what).Result;
                            default: return null;
                        }
                    });

                    _client.PingInterval = 300;
                    _client.MaxAutoReconnects = int.MaxValue;
                    
                    _client.OnUpdates += ClientUpdate;
                }

                if (bool.Parse(await GetConfigurationInfo("EnableProxy")))
                {
                    _client.TcpHandler = async (address, port) =>
                    {
                        var proxy = new Socks5ProxyClient(
                            await GetConfigurationInfo("ProxyHost"),
                        int.Parse(await GetConfigurationInfo("ProxyPort")));
                        return proxy.CreateConnection(address, port);
                    };
                }

                ConfigNeeded = await DoLogin(await GetConfigurationInfo("phone_number"));

                var mediaTask = DownloadMedia(stoppingToken);
                await mediaTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        async Task ClientUpdate(IObject arg)
        {
            if (arg is not Updates) return;

            Updates? updates = arg as Updates;
            if (updates == null) return;

            var updateArray = updates.UpdateList;
            var chats = updates.chats;
            long chatId = long.MaxValue;
            string chatTitle = "NoChatTitle";
            if (chats.Count > 0)
            {
                chatId = chats.First().Value.ID;
                chatTitle = chats.First().Value.Title;
            }

            var ignoredChatIds = await GetConfigurationInfo("IgnoredChatIds");
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

                var ignoredMessages = await GetConfigurationInfo("IgnoredMessages");
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

                if (message.media is MessageMediaDocument { document: Document document })
                {
                    int slash = document.mime_type.IndexOf('/');
                    if (slash < 0)
                    {
                        continue;
                    }

                    if (!document.mime_type.Contains("video"))
                    {
                        continue;
                    }

                    double duration = double.Parse(await GetConfigurationInfo("VideoDurationLimit"));
                    double maxDuration = double.Parse(await GetConfigurationInfo("VideoDurationMaxLimit"));
                    int minWidth = int.Parse(await GetConfigurationInfo("MinVideoWidth"));
                    int minHeight = int.Parse(await GetConfigurationInfo("MinVideoHeight"));
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
                            // 使用配置的分辨率进行判断
                            if (video.w >= minWidth && video.h >= minHeight)
                            {
                                isQualityEnough = true;
                            }
                        }
                    }

                    if (isDurationLimit || (!isVideo) || (!isQualityEnough))
                    {
                        continue;
                    }

                    var isExsist = await _mediaInfoRepository.FindAsync(x => x.MediaId == document.id);
                    if (isExsist != null)
                    {
                        continue;
                    }


                    string titleDirectoy = Path.Combine(await GetConfigurationInfo("SaveVideoPathPrefix"), chatId.ToString());
                    if (!Directory.Exists(titleDirectoy))
                    {
                        Directory.CreateDirectory(titleDirectoy);
                    }
                    string fileName = Path.Combine(titleDirectoy, $"{document.id}.{document.mime_type[(slash + 1)..]}");

                    MediaInfo canAdd = await _mediaInfoRepository.InsertAsync(new MediaInfo()
                    {
                        MediaId = document.id,
                        ChatId = chatId,
                        ChatTitle = chatTitle,
                        Message = message.message,
                        SavePath = fileName,
                        Size = document.size,
                        MimeType = document.mime_type,
                        IsExternalLinkGenerated = false,
                        ConcurrencyStamp = Guid.NewGuid().ToString("N")
                    });
                    if (canAdd != null)
                    {
                        _mediaQueue.AddItem(new MediaQueueModel()
                        {
                            MediaInfos = canAdd,
                            TObject = document,
                            IsPhoto = false
                        });
                    }

                }
                else if (message.media is MessageMediaPhoto { photo: Photo photo })
                {

                    var isExsist = await _mediaInfoRepository.FindAsync(x => x.MediaId == photo.id);
                    if (isExsist != null)
                    {
                        continue;
                    }


                    string titleDirectoy = Path.Combine(await GetConfigurationInfo("SavePhotoPathPrefix"), chatId.ToString());
                    if (!Directory.Exists(titleDirectoy))
                    {
                        Directory.CreateDirectory(titleDirectoy);
                    }
                    string fileName = Path.Combine(titleDirectoy, $"{photo.id}.jpg");

                    MediaInfo canAdd = await _mediaInfoRepository.InsertAsync(new MediaInfo()
                    {
                        MediaId = photo.id,
                        ChatId = chatId,
                        ChatTitle = chatTitle,
                        Message = message.message,
                        SavePath = fileName,
                        Size = photo.LargestPhotoSize.FileSize,
                        MimeType = "JPG",
                        IsExternalLinkGenerated = false,
                        ConcurrencyStamp = Guid.NewGuid().ToString("N")
                    });
                    if (canAdd != null)
                    {
                        _mediaQueue.AddItem(new MediaQueueModel()
                        {
                            MediaInfos = canAdd,
                            TObject = photo,
                            IsPhoto = true
                        });
                    }

                }
            }
        }

        public async Task DownloadMedia(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var model = await _mediaQueue.GetItemAsync(stoppingToken);
                    if (model?.MediaInfos == null || model.TObject == null)
                    {
                        continue;
                    }

                    var mediaInfo = model.MediaInfos;
                    long fileSize = model.IsPhoto 
                        ? ((Photo)model.TObject).LargestPhotoSize.FileSize 
                        : ((Document)model.TObject).size;

                    if (await IsSpaceUpperLimit(fileSize))
                    {
                        var isLoopDownload = bool.Parse(await GetConfigurationInfo("IsLoopDownload"));
                        if (!isLoopDownload)
                        {
                            await _mediaInfoRepository.DeleteAsync(mediaInfo.Id);
                            continue;
                        }
                        else
                        {
                            await DeleteOldestMediaUntilSpaceAvailable(fileSize);
                        }
                    }

                    await IsUpperLimit();
                    DeleteTempFiles(model.IsPhoto 
                        ? await GetConfigurationInfo("SavePhotoPathPrefix") 
                        : await GetConfigurationInfo("SaveVideoPathPrefix"));

                    if (model.IsPhoto)
                    {
                        using var fileStream = File.Create(mediaInfo.SavePath);
                        await _client!.DownloadFileAsync((Photo)model.TObject, fileStream);
                        fileStream.Close();
                    }
                    else
                    {
                        string fileNameTemp = $"{mediaInfo.SavePath}.temp";
                        using var fileStream = File.Create(fileNameTemp);
                        await _client!.DownloadFileAsync((Document)model.TObject, fileStream);
                        fileStream.Close();
                        File.Move(fileNameTemp, mediaInfo.SavePath, true);
                    }

                    await UpdateIsDownloadCompleted(mediaInfo.Id);
                    Logger.LogInformation($"{(model.IsPhoto ? "Photo" : "Video")} download completed {mediaInfo.SavePath}");
                }
                catch (Exception e)
                {
                    Logger.LogError($"Media download error:{e.Message}------{e.StackTrace}");
                }
            }
        }

        public async Task<double> CalculationDownloadsSize()
        {
            DateTime todayAtZero = DateTimeHelper.GetTodayAtZero();
            DateTime tomorrowAtZero = DateTimeHelper.GetTomorrowAtZero();
            using (_mediaInfoRepository.DisableTracking())
            {
                var result = await _mediaInfoRepository.GetListAsync(m =>
                    m.LastModificationTime >= todayAtZero &&
                    m.LastModificationTime < tomorrowAtZero);
                long size = result.Sum(x => x.Size);
                return StorageUnitConversionHelper.ByteToMB((double)(size));
            }
        }

        public async Task IsUpperLimit()
        {

            long bandwidth = long.Parse(await GetConfigurationInfo("Bandwidth"));
            double sizes = await CalculationDownloadsSize();
            Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Downloaded:{sizes}MB");
            if (sizes > bandwidth)
            {
                Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Download traffic reached the limit, pause download");
                Thread.Sleep(DateTimeHelper.GetUntilTomorrowTimeSpan());
                Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Download traffic and start over");
            }

        }

        public async Task<bool> IsSpaceUpperLimit(long sizes)
        {

            double availableFreeSpace = double.Parse(await GetConfigurationInfo("AvailableFreeSpace"));
            string driveName = await GetConfigurationInfo("SaveDrive");
            var driveAvailableMB = SpaceHelper.GetDriveAvailableMB(driveName);
            var sizesMB = StorageUnitConversionHelper.ByteToMB(sizes);
            if ((driveAvailableMB - sizesMB) < availableFreeSpace)
            {
                Logger.LogDebug($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Out of space stop downloading");
                await Task.Delay(1000);
                return true;
            }
            else
            {
                Logger.LogDebug($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Enough space available, start downloading");
                return false;
            }

        }

        public void DeleteTempFiles(string path)
        {
            if (Directory.Exists(path))
            {
                SpaceHelper.DeleteTempFiles(path);
                Logger.LogInformation("All .temp files have been deleted.");
            }
            else
            {
                Logger.LogError($"Directory {path} does not exist.");
            }
        }

        public async Task UpdateIsDownloadCompleted(long id)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                var mediaInfo = await _mediaInfoRepository.GetAsync(id);
                mediaInfo.IsDownloadCompleted = true;
                await uow.CompleteAsync();
            }
        }


        private async Task DeleteOldestMediaUntilSpaceAvailable(long requiredSpace)
        {
            while (await IsSpaceUpperLimit(requiredSpace))
            {
                using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
                {
                    try
                    {
                        var queryable = await _mediaInfoRepository.GetListAsync(x => x.IsDownloadCompleted && (!x.IsExternalLinkGenerated));

                        // 检查是否启用循环下载
                        var isLoopDownload = bool.Parse(await GetConfigurationInfo("IsLoopDownload"));
                        MediaInfo? mediaToDelete;

                        if (isLoopDownload)
                        {
                            Logger.LogInformation("循环下载已启用，使用时间密度算法删除视频");
                            // 启用循环下载时，优先删除时间密度高的视频
                            mediaToDelete = await GetHighDensityMediaToDelete(queryable.ToList());
                        }
                        else
                        {
                            Logger.LogInformation("循环下载未启用，使用默认算法删除最旧的视频");
                            // 未启用循环下载时，按原来的逻辑删除最旧的视频
                            mediaToDelete = queryable
                                .OrderBy(x => x.LastModificationTime)
                                .FirstOrDefault();
                        }

                        if (mediaToDelete == null)
                        {
                            Logger.LogWarning("No media files found to delete.");
                            break;
                        }

                        SpaceHelper.DeleteFile(mediaToDelete.SavePath);

                        var mediaInfo = await _mediaInfoRepository.GetAsync(mediaToDelete.Id);
                        mediaInfo.IsExternalLinkGenerated = true;
                        Logger.LogInformation($"Deleted media file: {mediaToDelete.SavePath}, Size: {mediaInfo.Size} bytes, Created: {mediaInfo.LastModificationTime:yyyy-MM-dd HH:mm:ss}");
                        await uow.CompleteAsync();
                    }
                    catch
                    {
                        await uow.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 获取需要删除的高密度媒体文件
        /// 时间密度高的定义：同一时间窗口内获取到的多个已下载完成的视频
        /// </summary>
        /// <param name="mediaList">已下载完成的媒体列表</param>
        /// <returns>需要删除的媒体文件</returns>
        private async Task<MediaInfo?> GetHighDensityMediaToDelete(List<MediaInfo> mediaList)
        {
            if (mediaList == null || mediaList.Count == 0)
            {
                Logger.LogWarning("媒体列表为空，无法执行时间密度删除算法");
                return null;
            }

            // 从配置中获取时间窗口大小（分钟），默认为2分钟
            int timeWindowMinutes = 2;
            try
            {
                timeWindowMinutes = int.Parse(await GetConfigurationInfo("TimeDensityWindowMinutes"));
                Logger.LogInformation($"从配置中读取时间密度窗口大小: {timeWindowMinutes} 分钟");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"无法从配置中读取时间密度窗口大小，使用默认值 2 分钟。错误: {ex.Message}");
            }

            Logger.LogInformation($"开始执行时间密度删除算法，共有 {mediaList.Count} 个已下载完成的媒体文件，时间窗口: {timeWindowMinutes} 分钟");

            // 按创建时间分组，计算每个时间窗口的媒体数量
            var mediaGroups = mediaList
                .GroupBy(x => {
                    var time = x.CreationTime;
                    // 将分钟除以时间窗口大小，实现按指定时间窗口分组
                    int timeBlock = time.Minute / timeWindowMinutes;
                    return new {
                        Year = time.Year,
                        Month = time.Month,
                        Day = time.Day,
                        Hour = time.Hour,
                        TimeBlock = timeBlock
                    };
                })
                .Select(g => new {
                    TimeKey = g.Key,
                    Count = g.Count(),
                    Medias = g.ToList(),
                    // 计算这个时间窗口的实际起始和结束分钟
                    StartMinute = g.Key.TimeBlock * timeWindowMinutes,
                    EndMinute = Math.Min((g.Key.TimeBlock * timeWindowMinutes) + timeWindowMinutes - 1, 59)
                })
                .OrderByDescending(g => g.Count) // 按数量降序排列，优先处理数量多的组
                .ThenBy(g => g.TimeKey.Year)
                .ThenBy(g => g.TimeKey.Month)
                .ThenBy(g => g.TimeKey.Day)
                .ThenBy(g => g.TimeKey.Hour)
                .ThenBy(g => g.TimeKey.TimeBlock)
                .ToList();

            Logger.LogInformation($"时间分组完成，共分为 {mediaGroups.Count} 个时间组（按每 {timeWindowMinutes} 分钟分组）");

            // 记录每个时间组的详细信息
            foreach (var group in mediaGroups.Take(5)) // 只记录前5个组，避免日志过多
            {
                Logger.LogInformation($"时间组 {group.TimeKey.Year}-{group.TimeKey.Month:D2}-{group.TimeKey.Day:D2} {group.TimeKey.Hour:D2}:{group.StartMinute:D2}-{group.EndMinute:D2} 包含 {group.Count} 个已下载完成的文件");
            }

            // 如果没有找到分组或者所有分组都只有一个文件，则按原来的逻辑删除最旧的文件
            if (mediaGroups.Count == 0 || mediaGroups.All(g => g.Count == 1))
            {
                Logger.LogInformation("未找到高密度时间组，使用默认算法删除最旧的文件");
                var oldestFile = mediaList
                    .OrderBy(x => x.CreationTime)
                    .FirstOrDefault();
                
                if (oldestFile != null)
                {
                    Logger.LogInformation($"选择删除最旧的文件: {oldestFile.SavePath}, 创建时间: {oldestFile.CreationTime:yyyy-MM-dd HH:mm:ss}");
                }
                
                return oldestFile;
            }

            // 找到第一个数量大于1的分组（时间密度最高的分组）
            var highDensityGroup = mediaGroups.FirstOrDefault(g => g.Count > 1);
            if (highDensityGroup != null)
            {
                Logger.LogInformation($"找到高密度时间组: {highDensityGroup.TimeKey.Year}-{highDensityGroup.TimeKey.Month:D2}-{highDensityGroup.TimeKey.Day:D2} {highDensityGroup.TimeKey.Hour:D2}:{highDensityGroup.StartMinute:D2}-{highDensityGroup.EndMinute:D2}, 包含 {highDensityGroup.Count} 个已下载完成的文件");
                
                // 在高密度分组中，按创建时间排序，删除最旧的文件
                var oldestInHighDensity = highDensityGroup.Medias
                    .OrderBy(x => x.CreationTime)
                    .FirstOrDefault();
                
                if (oldestInHighDensity != null)
                {
                    Logger.LogInformation($"在高密度时间组中选择删除最旧的文件: {oldestInHighDensity.SavePath}, 创建时间: {oldestInHighDensity.CreationTime:yyyy-MM-dd HH:mm:ss}");
                }
                
                return oldestInHighDensity;
            }

            // 如果没有找到高密度分组，则按原来的逻辑删除最旧的文件
            Logger.LogInformation("未找到高密度时间组，使用默认算法删除最旧的文件");
            var defaultOldestFile = mediaList
                .OrderBy(x => x.CreationTime)
                .FirstOrDefault();
            
            if (defaultOldestFile != null)
            {
                Logger.LogInformation($"选择删除最旧的文件: {defaultOldestFile.SavePath}, 创建时间: {defaultOldestFile.CreationTime:yyyy-MM-dd HH:mm:ss}");
            }
            
            return defaultOldestFile;
        }

        public override void Dispose()
        {
            TGClinet?.Dispose();
            _serviceScope.Dispose();
            base.Dispose();
        }

    }
}
