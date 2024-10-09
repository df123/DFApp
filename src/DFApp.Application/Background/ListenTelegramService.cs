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


        private IQueueBase<DocumentQueueModel> _documentQueue;
        private IQueueBase<PhotoQueueModel> _photoQueue;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScope _serviceScope;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly IRepository<MediaInfo, long> _mediaInfoRepository;


        private ILogger Logger => _loggerFactory.CreateLogger(GetType().FullName!) ?? NullLogger.Instance;


        public ListenTelegramService(IServiceScopeFactory serviceScopeFactory)
        {
            _documentQueue = new QueueBase<DocumentQueueModel>();
            _photoQueue = new QueueBase<PhotoQueueModel>();

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

                var mediaTask = DownloadMedia(await GetConfigurationInfo("SaveVideoPathPrefix"), _documentQueue, stoppingToken);
                var photoTask = DownloadPhoto(await GetConfigurationInfo("SavePhotoPathPrefix"), _photoQueue, stoppingToken);
                await mediaTask.ConfigureAwait(false);
                await photoTask.ConfigureAwait(false);
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
                    bool isDurationLimit = false;
                    bool isVideo = false;

                    foreach (var attribute in document.attributes)
                    {
                        if (attribute is DocumentAttributeVideo video)
                        {
                            if (video.duration <= duration)
                            {
                                isDurationLimit = true;
                            }
                            isVideo = true;
                        }
                    }

                    if (isDurationLimit || (!isVideo))
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
                        MD5 = string.Empty,
                        MimeType = document.mime_type,
                        IsExternalLinkGenerated = false
                    });
                    if (canAdd != null)
                    {
                        _documentQueue.AddItem(new DocumentQueueModel()
                        {
                            MediaInfos = canAdd,
                            TObject = document,
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
                        MD5 = string.Empty,
                        MimeType = "JPG",
                        IsExternalLinkGenerated = false
                    });
                    if (canAdd != null)
                    {
                        _photoQueue.AddItem(new PhotoQueueModel()
                        {
                            MediaInfos = canAdd,
                            TObject = photo,
                        });
                    }

                }
            }
        }

        public async Task DownloadPhoto(string savePathPrefix, IQueueBase<PhotoQueueModel> queue, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var model = await queue.GetItemAsync(stoppingToken);
                    if (model == null)
                    {
                        continue;
                    }

                    Photo? photo = model.TObject;
                    if (photo == null)
                    {
                        continue;
                    }

                    MediaInfo? mediaInfo = model.MediaInfos;
                    if (mediaInfo == null)
                    {
                        continue;
                    }

                    if (await IsSpaceUpperLimit(photo.LargestPhotoSize.FileSize))
                    {
                        await _mediaInfoRepository.DeleteAsync(mediaInfo.Id);
                        continue;
                    }
                    await IsUpperLimit();
                    DeleteTempFiles(savePathPrefix);

                    using var fileStream = File.Create(model.MediaInfos!.SavePath);
                    var type = await _client!.DownloadFileAsync(photo, fileStream);
                    string MD5 = HashHelper.CalculateMD5(fileStream);

                    fileStream.Close();
                    await UpdateMD5ById(mediaInfo.Id, MD5);

                    Logger.LogDebug($"Photo download completed {model.MediaInfos!.SavePath}");
                }
                catch (Exception e)
                {
                    Logger.LogError($"Photo download error:{e.Message}------{e.StackTrace}");
                }
            }

        }

        public async Task DownloadMedia(string savePathPrefix, IQueueBase<DocumentQueueModel> queue, CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var model = await queue.GetItemAsync(stoppingToken);
                    if (model == null)
                    {
                        continue;
                    }

                    Document? document = model.TObject;
                    if (document == null)
                    {
                        continue;
                    }

                    MediaInfo? mediaInfo = model.MediaInfos;
                    if (mediaInfo == null)
                    {
                        continue;
                    }
                    if (await IsSpaceUpperLimit(document.size + ListenTelegramConst.SpaceUpperLimitInBytes))
                    {
                        await _mediaInfoRepository.DeleteAsync(mediaInfo.Id);
                        continue;
                    }
                    await IsUpperLimit();
                    DeleteTempFiles(savePathPrefix);
                    string fileNameTemp = $"{mediaInfo.SavePath}.temp";
                    using var fileStream = File.Create(fileNameTemp);
                    await _client!.DownloadFileAsync(document, fileStream);
                    string MD5 = HashHelper.CalculateMD5(fileStream);
                    fileStream.Close();
                    File.Move(fileNameTemp, mediaInfo.SavePath, true);
                    await UpdateMD5ById(mediaInfo.Id, MD5);


                    Logger.LogDebug($"Video download completed {mediaInfo.SavePath}");
                }
                catch (Exception e)
                {
                    Logger.LogError($"Video download error:{e.Message}------{e.StackTrace}");
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

        public async Task UpdateMD5ById(long id, string md5)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                var mediaInfo = await _mediaInfoRepository.GetAsync(id);
                mediaInfo.MD5 = md5;
                await uow.CompleteAsync();
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
