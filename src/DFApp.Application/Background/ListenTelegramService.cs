using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Media;
using DFApp.Queue;
using Microsoft.Extensions.Logging;
using Starksoft.Net.Proxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TL;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Background
{
    public class ListenTelegramService : DFAppBackgroundWorkerBase, IDisposable
    {
        private WTelegram.Client? _client;
        public WTelegram.Client? TGClinet { get { return _client; } }
        public User? User => TGClinet?.User;

        public string ConfigNeeded { get; set; } = "start";

        private readonly IQueueManagement _queueManagement;
        private readonly IQueueBase<DocumentQueueModel> _documentQueue;
        private readonly IQueueBase<PhotoQueueModel> _photoQueue;
        private readonly IRepository<MediaInfo,long> _mediaInfoRepository;
        public const long SpaceUpperLimitInBytes = 2048L * 1024L * 1024L; // 2 GB


        public ListenTelegramService(
        IQueueManagement queueManagement
        , IRepository<MediaInfo,long> mediaInfoRepository
        , IConfigurationInfoRepository configurationInfoRepository)
            : base(ListenTelegramConst.ModuleName, configurationInfoRepository)
        {
            this._mediaInfoRepository = mediaInfoRepository;
            _queueManagement = queueManagement;
            _documentQueue = _queueManagement.AddQueue<DocumentQueueModel>(ListenTelegramConst.DocumentQueue);
            _photoQueue = _queueManagement.AddQueue<PhotoQueueModel>(ListenTelegramConst.PhotoQueue);
        }

        public override Task StartAsync(CancellationToken cancellationToken = default)
        {
            _executeTask = StartWork();

            if (_executeTask.IsCompleted)
            {
                return _executeTask;
            }

            return base.StartAsync(StoppingToken);
        }

        public override async Task RestartAsync(CancellationToken cancellationToken = default)
        {
            await base.RestartAsync(StoppingToken);
            _documentQueue.ResetSignal();
            _photoQueue.ResetSignal();
            await StartAsync(StoppingToken);
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

        public async Task StartWork()
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

                var mediaTask = DownloadMedia(await GetConfigurationInfo("SaveVideoPathPrefix"), _documentQueue, StoppingToken);
                var photoTask = DownloadPhoto(await GetConfigurationInfo("SavePhotoPathPrefix"), _photoQueue, StoppingToken);
                await mediaTask;
                await photoTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                _hasError = true;
                ErrorCount++;
                ErrorDescription = ex.Message;
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
                    string typeName = document.mime_type[..(slash)];
                    if (typeName.ToLower() != "video" && typeName.ToLower() != "image")
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

                    if (IsSpaceUpperLimit(photo.LargestPhotoSize.FileSize))
                    {
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
                    if (IsSpaceUpperLimit(document.size + SpaceUpperLimitInBytes))
                    {
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

        public async Task DeleteDownloadInfo(MediaInfo mediaInfo)
        {
            Logger.LogInformation($"Start deleting duplicates。Hash:{mediaInfo.MD5}");
            try
            {
                if (mediaInfo.SavePath == null) { return; }
                File.Delete(mediaInfo.SavePath);
                await _mediaInfoRepository.DeleteAsync(mediaInfo);

                Logger.LogInformation($"End delete successfully。Hash:{mediaInfo.MD5}");
            }
            catch (System.Exception e)
            {
                Logger.LogInformation($"End deletion failure, failure message{e.Message}。Hash:{mediaInfo.MD5}");
            }
        }

        // public async Task<double> CalculationDownloadsSize()
        // {
        //     long size = await _mediaInfoRepository.GetDownloadsSize();
        //     return StorageUnitConversionHelper.ByteToMB((double)(size));
        // }

        public async Task IsUpperLimit()
        {
            return;
// #if DEBUG
//             return;
// #endif

//             long.TryParse(AppsettingsHelper.app("RunConfig", "Bandwidth"), out long bandwidth);
//             double sizes = await CalculationDownloadsSize();
//             Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Downloaded:{sizes}MB");
//             if (sizes > bandwidth)
//             {
//                 Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Download traffic reached the limit, pause download");
//                 Thread.Sleep(DateTimeHelper.GetUntilTomorrowTimeSpan());
//                 Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Download traffic and start over");
//             }
        }

        public bool IsSpaceUpperLimit(long sizes)
        {

#if DEBUG
            return false;
#endif

            double availableFreeSpace = double.Parse(AppsettingsHelper.app("RunConfig", "AvailableFreeSpace"));
            string driveName = AppsettingsHelper.app("RunConfig", "SaveDrive");
            if ((SpaceHelper.GetDriveAvailableMB(driveName) - StorageUnitConversionHelper.ByteToMB(sizes)) < availableFreeSpace)
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
            var mediaInfo = await _mediaInfoRepository.GetAsync(id);
            mediaInfo.MD5 = md5;
            await _mediaInfoRepository.UpdateAsync(mediaInfo);
        }

        public void Dispose()
        {
            TGClinet?.Dispose();
        }
    }
}
