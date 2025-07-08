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

                if (bool.Parse(GetConfigurationInfo("EnableProxy").Result))
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
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                try
                {
                    while (await IsSpaceUpperLimit(requiredSpace))
                    {

                        var queryable = await _mediaInfoRepository.GetListAsync(x => x.IsDownloadCompleted && (!x.IsExternalLinkGenerated));

                        var oldestMedia = queryable
                        .OrderBy(x => x.LastModificationTime)
                        .FirstOrDefault();

                        if (oldestMedia == null)
                        {
                            Logger.LogWarning("No media files found to delete.");
                            break;
                        }

                        SpaceHelper.DeleteFile(oldestMedia.SavePath);

                        var mediaInfo = await _mediaInfoRepository.GetAsync(oldestMedia.Id);
                        mediaInfo.IsExternalLinkGenerated = true;
                        Logger.LogInformation($"Deleted oldest media file: {oldestMedia.SavePath}");
                        await uow.CompleteAsync();
                    }

                }
                catch
                {
                    await uow.RollbackAsync();
                    throw;
                }
            }
        }

        public override void Dispose()
        {
            TGClinet?.Dispose();
            _serviceScope.Dispose();
            base.Dispose();
        }

    }
}
