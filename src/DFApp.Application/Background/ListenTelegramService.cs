﻿using DFApp.Configuration;
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
        private readonly IMediaRepository _mediaInfoRepository;


        public ListenTelegramService(
        IQueueManagement queueManagement
        , IMediaRepository mediaInfoRepository
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
            if (arg is not Updates { updates: Update[] updateArray }) return;
            string title = string.Empty;
            if (arg is Updates)
            {
                Dictionary<long, ChatBase> chats = ((Updates)arg).Chats;
                title = $"{chats.First().Value.Title}:{chats.First().Value.ID}";
                foreach (var chat in chats.Values)
                {
                    Logger.LogDebug($"Title:{chat.Title},ID:{chat.ID},IsActive:{chat.IsActive},IsChannel:{chat.IsChannel},IsGroup:{chat.IsGroup}");
                }
            }

            foreach (Update update in updateArray)
            {
                if (update is not UpdateNewMessage { message: Message message })
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
                    MediaInfo? canAdd = await AddDownloadInfo(new MediaInfo()
                    {
                        AccessHash = document.access_hash,
                        TID = document.id,
                        Size = document.size,
                        MimeType = document.mime_type,
                        Title = title
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
                    MediaInfo? canAdd = await AddDownloadInfo(new MediaInfo()
                    {
                        AccessHash = photo.access_hash,
                        TID = photo.id,
                        Size = photo.LargestPhotoSize.FileSize,
                        MimeType = "JPG",
                        Title = title
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

        public async Task<MediaInfo?> AddDownloadInfo(MediaInfo mediaInfo)
        {
            MediaInfo[] isArray = await _mediaInfoRepository.GetByAccessHashID(mediaInfo.AccessHash, mediaInfo.TID, mediaInfo.Size);
            if (isArray != null && isArray.Length > 0)
            {
                Logger.LogDebug($"AccessHash:{mediaInfo.AccessHash},ID:{mediaInfo.TID},Size:{mediaInfo.Size},Already exists;");

                return null;
            }

            MediaInfo dto = await _mediaInfoRepository.InsertAsync(mediaInfo);
            if (dto != null && dto.Id > -1)
            {
                Logger.LogDebug($"New Media Save successfully;");
            }
            return dto;
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
                    string title = PathHelper.RemoveInvalidPath(PathHelper.SplitStringAndGetValueAtPosition(mediaInfo.Title, ":", 1));
                    string titleDirectoy = Path.Combine(savePathPrefix, title);
                    if (!Directory.Exists(titleDirectoy))
                    {
                        Directory.CreateDirectory(titleDirectoy);
                    }
                    string fileName = Path.Combine(titleDirectoy, $"{photo.id}.jpg");
                    using var fileStream = File.Create(fileName);
                    var type = await _client!.DownloadFileAsync(photo, fileStream);
                    string valueSHA1 = HashHelper.CalculationHash(fileStream);
                    fileStream.Close();

                    mediaInfo.AccessHash = photo.access_hash;
                    mediaInfo.TID = photo.id;
                    mediaInfo.SavePath = fileName;
                    mediaInfo.ValueSHA1 = valueSHA1;

                    await UpdateDownloadInfo(mediaInfo);

                    Logger.LogDebug($"Photo download completed {fileName}");
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
                    if (!await StartDownloadVideo())
                    {
                        continue;
                    }
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
                    if (IsSpaceUpperLimit(document.size + (2048L * 1024L * 1024L)))
                    {
                        continue;
                    }
                    await IsUpperLimit();
                    if (document.size / 1024 / 1024 <= 10)
                    {
                        continue;
                    }
                    DeleteTempFiles(savePathPrefix);
                    int slash = document.mime_type.IndexOf('/');
                    string title = PathHelper.RemoveInvalidPath(PathHelper.SplitStringAndGetValueAtPosition(mediaInfo.Title, ":", 1));
                    string fileName = Path.Combine(savePathPrefix, $"{document.id}{title}.{document.mime_type[(slash + 1)..]}");
                    string fileNameTemp = $"{fileName}.temp";
                    using var fileStream = File.Create(fileNameTemp);
                    await _client!.DownloadFileAsync(document, fileStream);
                    string valueSHA1 = HashHelper.CalculationHash(fileStream);
                    fileStream.Close();
                    File.Move(fileNameTemp, fileName, true);

                    mediaInfo.AccessHash = document.access_hash;
                    mediaInfo.TID = document.id;
                    mediaInfo.SavePath = fileName;
                    mediaInfo.ValueSHA1 = valueSHA1;

                    await UpdateDownloadInfo(mediaInfo);

                    Logger.LogDebug($"Video download completed {fileName}");
                }
                catch (Exception e)
                {
                    Logger.LogError($"Video download error:{e.Message}------{e.StackTrace}");
                }
            }

        }

        public async Task UpdateDownloadInfo(MediaInfo mediaInfo)
        {
            try
            {
                if (mediaInfo.ValueSHA1 == null) { return; }
                MediaInfo[] isArray = await _mediaInfoRepository.GetByValueSHA1(mediaInfo.ValueSHA1);
                if (isArray.Length > 0)
                {
                    await DeleteDownloadInfo(mediaInfo);
                    return;
                }

                await _mediaInfoRepository.UpdateAsync(mediaInfo);

                Logger.LogDebug($"AccessHash:{mediaInfo.AccessHash},ID:{mediaInfo.TID},Update successful;");
            }
            catch (Exception e)
            {
                Logger.LogError($"UpdateDownloadInfo  error:{e.Message}------{e.StackTrace}"); ;
            }

        }

        public async Task DeleteDownloadInfo(MediaInfo mediaInfo)
        {
            Logger.LogInformation($"Start deleting duplicates。ID:{mediaInfo.TID},AccessHash:{mediaInfo.AccessHash},Hash:{mediaInfo.ValueSHA1}");
            try
            {
                if (mediaInfo.SavePath == null) { return; }
                File.Delete(mediaInfo.SavePath);
                await _mediaInfoRepository.DeleteAsync(mediaInfo);

                Logger.LogInformation($"End delete successfully。ID:{mediaInfo.TID},AccessHash:{mediaInfo.AccessHash},Hash:{mediaInfo.ValueSHA1}");
            }
            catch (System.Exception e)
            {
                Logger.LogInformation($"End deletion failure, failure message{e.Message}。ID:{mediaInfo.TID},AccessHash:{mediaInfo.AccessHash},Hash:{mediaInfo.ValueSHA1}");
            }
        }

        public async Task<double> CalculationDownloadsSize()
        {
            long size = await _mediaInfoRepository.GetDownloadsSize();
            return StorageUnitConversionHelper.ByteToMB((double)(size));
        }

        public async Task<bool> StartDownloadVideo()
        {
            await Task.Delay(2000);
            if (DateTime.Now >= DateTimeHelper.GetTomorrowAtZero().AddHours(-9) && DateTime.Now <= DateTimeHelper.GetTomorrowAtZero().AddHours(-7))
            {
                return true;
            }
            return false;
        }

        public async Task IsUpperLimit()
        {
            long.TryParse(AppsettingsHelper.app("RunConfig", "Bandwidth"), out long bandwidth);
            double sizes = await CalculationDownloadsSize();
            Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Downloaded:{sizes}MB");
            if (sizes > bandwidth)
            {
                Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Download traffic reached the limit, pause download");
                Thread.Sleep(DateTimeHelper.GetUntilTomorrowTimeSpan());
                Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Download traffic and start over");
            }
        }

        public bool IsSpaceUpperLimit(long sizes)
        {
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




        public void Dispose()
        {
            TGClinet?.Dispose();
        }
    }
}
