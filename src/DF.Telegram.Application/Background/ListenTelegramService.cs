using DF.Telegram.Helper;
using DF.Telegram.Media;
using DF.Telegram.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TL;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.ObjectMapping;

namespace DF.Telegram.Background
{
    public class ListenTelegramService : BackgroundWorkerBase
    {
        private readonly WTelegram.Client _client;
        private readonly IQueueBase<DocumentQueueModel> _documentQueue;
        private readonly IQueueBase<PhotoQueueModel> _photoQueue;
        private readonly IMediaRepository _mediaInfoRepository;
        private readonly IObjectMapper _objectMapper;
        public ListenTelegramService(
        WTelegram.Client client,
        IQueueBase<DocumentQueueModel> documentQueue,
        IQueueBase<PhotoQueueModel> photoQueue,
        IMediaRepository mediaInfoRepository,
        IObjectMapper objectMapper)
        {
            this._client = client;
            this._mediaInfoRepository = mediaInfoRepository;
            WTelegram.Helpers.Log = (lvl, str) => Logger.Log((LogLevel)lvl, str);
            this._objectMapper = objectMapper;
            _documentQueue = documentQueue;
            _photoQueue = photoQueue;
            Directory.CreateDirectory(AppsettingsHelper.app("RunConfig", "SaveVideoPathPrefix"));
            Directory.CreateDirectory(AppsettingsHelper.app("RunConfig", "SavePhotoPathPrefix"));

        }

        public async override Task StartAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Start listening for messages");
            TL.User user = await _client.LoginUserIfNeeded();
            _client.OnUpdate += ClientUpdate;
            var mediaTask = DownloadMedia(AppsettingsHelper.app("RunConfig", "SaveVideoPathPrefix"), _documentQueue, StoppingToken);
            var photoTask = DownloadPhoto(AppsettingsHelper.app("RunConfig", "SavePhotoPathPrefix"), _photoQueue, StoppingToken);
            await mediaTask;
            await photoTask;
        }

        public override async Task<Task> StopAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Turn off listening to messages");
            StoppingTokenSource.Cancel();
            await Task.Delay(5000);
            return Task.CompletedTask;
        }

        async Task ClientUpdate(IObject arg)
        {
            if (arg is not Updates { updates: Update[] updateArray }) return;
            string title = string.Empty;
            if (arg is Updates)
            {
                Dictionary<long, ChatBase> chats = ((Updates)arg).Chats;
                StringBuilder sb = new StringBuilder(8);
                foreach (var chat in chats.Values)
                {
                    sb.Append(chat.Title);
                    sb.Append(",");
                }
                sb.Remove(sb.Length - 1, 1);
                title = sb.ToString();
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
                    }) ;
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
                Logger.LogInformation($"AccessHash:{mediaInfo.AccessHash},ID:{mediaInfo.TID},Size:{mediaInfo.Size},Already exists;");

                return null;
            }

            MediaInfo dto = await _mediaInfoRepository.InsertAsync(mediaInfo);
            if (dto != null && dto.Id > -1)
            {
                Logger.LogInformation($"New Media Save successfully;");
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

                    await IsSpaceUpperLimit(photo.LargestPhotoSize.FileSize);
                    await IsUpperLimit();
                    DeleteTempFiles(savePathPrefix);
                    string fileName = Path.Combine(savePathPrefix, $"{photo.id}.jpg");
                    Logger.LogInformation($"Photo download {fileName},access:{photo.access_hash},id:{photo.id}");
                    using var fileStream = File.Create(fileName);
                    var type = await _client.DownloadFileAsync(photo, fileStream);
                    string valueSHA1 = HashHelper.CalculationHash(fileStream);
                    fileStream.Close();

                    mediaInfo.AccessHash = photo.access_hash;
                    mediaInfo.TID = photo.id;
                    mediaInfo.SavePath = fileName;
                    mediaInfo.ValueSHA1 = valueSHA1;

                    await UpdateDownloadInfo(mediaInfo);

                    Logger.LogInformation($"Photo download completed {fileName},access:{photo.access_hash},id:{photo.id}");
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
                    await IsSpaceUpperLimit(document.size + (2048L * 1024L * 1024L));
                    await IsUpperLimit();
                    if (document.size / 1024 / 1024 <= 10)
                    {
                        continue;
                    }
                    DeleteTempFiles(savePathPrefix);
                    int slash = document.mime_type.IndexOf('/');
                    string fileName = Path.Combine(savePathPrefix, $"{document.id}.{document.mime_type[(slash + 1)..]}");
                    string fileNameTemp = $"{fileName}.temp";
                    Logger.LogInformation($"Video download {fileName},access:{document.access_hash},id:{document.id}");
                    using var fileStream = File.Create(fileNameTemp);
                    await _client.DownloadFileAsync(document, fileStream);
                    string valueSHA1 = HashHelper.CalculationHash(fileStream);
                    fileStream.Close();
                    File.Move(fileNameTemp, fileName, true);

                    mediaInfo.AccessHash = document.access_hash;
                    mediaInfo.TID = document.id;
                    mediaInfo.SavePath = fileName;
                    mediaInfo.ValueSHA1 = valueSHA1;

                    await UpdateDownloadInfo(mediaInfo);

                    Logger.LogInformation($"Video download completed {fileName},access:{document.access_hash},id:{document.id}");
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

                Logger.LogInformation($"AccessHash:{mediaInfo.AccessHash},ID:{mediaInfo.TID},Update successful;");
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

        //private async Task DeleteFile()
        //{
        //    MediaInfo? mediaInfo = null;
        //    switch (AppsettingsHelper.app(new string[] { "RunConfig", "DeleteFileModle" }))
        //    {
        //        case "direct":
        //            mediaInfo = await _mediaInfoRepository.GetVideoEarliest();
        //            break;
        //        default:
        //            mediaInfo = await _mediaInfoRepository.GetVideoReturn();
        //            break;
        //    }
        //    if (mediaInfo != null)
        //    {
        //        if (mediaInfo.SavePath != null)
        //        {
        //            SpaceHelper.DeleteFile(mediaInfo.SavePath);
        //        }
        //        await _mediaInfoRepository.DeleteAsync(mediaInfo);
        //        Logger.LogInformation($"Delete ID:{mediaInfo.TID},AccessHash:{mediaInfo.AccessHash},Hash:{mediaInfo.ValueSHA1},Sizes:{mediaInfo.Size}");
        //    }
        //    else
        //    {
        //        Thread.Sleep(10000);
        //    }
        //}

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

        public async Task IsSpaceUpperLimit(long sizes)
        {
            double.TryParse(AppsettingsHelper.app("RunConfig", "AvailableFreeSpace"), out double availableFreeSpace);
            Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Available Space {SpaceHelper.GetDriveAvailableMB(AppsettingsHelper.app("RunConfig", "SaveDrive"))} MB");
            while (SpaceHelper.GetDriveAvailableMB(AppsettingsHelper.app("RunConfig", "SaveDrive")) - StorageUnitConversionHelper.ByteToMB(sizes) < availableFreeSpace)
            {
                if (bool.Parse(AppsettingsHelper.app(new string[] {
                "RunConfig", "IntervalTime","SpaceTime","Enable"})))
                {
                    Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}The available space has reached the lower limit, downloading is suspended");
                    Thread.Sleep(int.Parse(AppsettingsHelper.app(new string[] {
                "RunConfig", "IntervalTime","SpaceTime","Duration"})));
                }
                //await DeleteFile();
            }
            Logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Enough space available, start downloading");
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

    }
}
