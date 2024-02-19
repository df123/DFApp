using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Media;
using DFApp.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Background
{
    public class MediaBackgroudService : DFAppBackgroundWorkerBase
    {
        private readonly IMediaRepository _mediaInfoRepository;
        private readonly IRepository<MediaExternalLink> _externalLinkRepository;
        private readonly IQueueManagement _queueManagement;
        private readonly IQueueBase<int> _queueGenerate;
        private readonly IQueueBase<long> _queueMove;
        private readonly Stopwatch _stopwatch;

        public MediaBackgroudService(IMediaRepository repository
            , IConfigurationInfoRepository configurationInfoRepository
            , IRepository<MediaExternalLink> externalLinkRepository
            , IQueueManagement queueManagement)
            : base(MediaBackgroudConst.ModuleName, configurationInfoRepository)
        {
            _mediaInfoRepository = repository;
            _externalLinkRepository = externalLinkRepository;
            _queueManagement = queueManagement;
            _queueGenerate = _queueManagement.GetQueue<int>(MediaBackgroudConst.QueueGenerate);
            _queueMove = _queueManagement.GetQueue<long>(MediaBackgroudConst.QueueMove);
            _stopwatch = Stopwatch.StartNew();

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
            _queueGenerate.ResetSignal();
            _queueMove.ResetSignal();
            await StartAsync(StoppingToken);
        }


        public async Task StartWork()
        {
            try
            {
                Task task1 = GenerateExternalLink();
                Task task2 = MoveFiles();
                await task1;
                await task2;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                _hasError = true;
                ErrorCount++;
                ErrorDescription = ex.Message;
            }
        }

        public async Task GenerateExternalLink()
        {
            int result = 0;
            while (!StoppingToken.IsCancellationRequested)
            {
                result = await _queueGenerate.GetItemAsync(StoppingToken);
                _stopwatch.Restart();
                string returnDownloadUrlPrefix = await GetConfigurationInfo("ReturnDownloadUrlPrefix");
                Check.NotNullOrWhiteSpace(returnDownloadUrlPrefix, nameof(returnDownloadUrlPrefix));

                string photoSavePath = await GetConfigurationInfo("SavePhotoPathPrefix");
                Check.NotNullOrWhiteSpace(photoSavePath, nameof(photoSavePath));

                var temp = await _mediaInfoRepository.GetListAsync(x => (!x.IsFileDeleted) && (!x.IsExternalLinkGenerated));

                if (temp == null || temp.Count <= 0)
                {
                    return;
                }

                string datetimeName = DateTime.Now.ToString("yyyyMMddHHmmss");
                string zipPhotoName = $"{datetimeName}.zip";
                string zipPhotoPathName = Path.Combine(Path.GetDirectoryName(photoSavePath)!, zipPhotoName);

                using ZipArchive archive = ZipFile.Open(zipPhotoPathName, ZipArchiveMode.Create);
                long size = 0;

                StringBuilder stringBuilder = new StringBuilder();
                if (File.Exists(zipPhotoPathName))
                {
                    stringBuilder.AppendLine(Path.Combine(returnDownloadUrlPrefix, zipPhotoName));
                }

                string zipType = await GetConfigurationInfo("ZipType");
                string replaceUrlPrefix = await GetConfigurationInfo("ReplaceUrlPrefix");
                foreach (var mediaInfo in temp)
                {
                    if (mediaInfo.SavePath == null || zipType.Contains(mediaInfo.MimeType!))
                    {
                        if (!string.IsNullOrWhiteSpace(mediaInfo.SavePath))
                        {
                            archive.CreateEntryFromFile(mediaInfo.SavePath, Path.GetFileName(mediaInfo.SavePath));
                            mediaInfo.IsExternalLinkGenerated = true;
                            size += mediaInfo.Size;
                        }

                        continue;
                    }
                    stringBuilder.AppendLine($"{Path.Combine(returnDownloadUrlPrefix, mediaInfo.SavePath.Replace(replaceUrlPrefix, string.Empty).Replace("\\", "/"))}");
                    mediaInfo.IsExternalLinkGenerated = true;
                }

                if (File.Exists(zipPhotoPathName))
                {
                    temp.Add(await _mediaInfoRepository.InsertAsync(new MediaInfo()
                    {
                        AccessHash = Random.Shared.NextInt64(),
                        TID = Random.Shared.NextInt64(),
                        Size = size,
                        SavePath = zipPhotoPathName,
                        ValueSHA1 = HashHelper.CalculationHash(zipPhotoPathName),
                        MimeType = "zip",
                        IsExternalLinkGenerated = true
                    }));
                }

                if (temp != null && temp.Count > 0)
                {
                    await _mediaInfoRepository.UpdateManyAsync(temp);
                    _stopwatch.Stop();
                    await _externalLinkRepository.InsertAsync(new MediaExternalLink()
                    {
                        Name = datetimeName,
                        Size = size,
                        TimeConsumed = _stopwatch.ElapsedMilliseconds,
                        IsRemove = false,
                        LinkContent = stringBuilder.ToString(),
                        MediaIds = string.Join(',', temp.Select(x => x.Id)),
                    });
                }

                _queueGenerate.Clear();
            }
        }


        public async Task MoveFiles()
        {
            long result = 0;
            while (!StoppingToken.IsCancellationRequested)
            {
                result = await _queueMove.GetItemAsync(StoppingToken);
                MediaExternalLink mediaExternalLink = await _externalLinkRepository.GetAsync(x => x.Id == result);
                if (mediaExternalLink != null && (!mediaExternalLink.IsRemove) && (!string.IsNullOrWhiteSpace(mediaExternalLink.MediaIds)))
                {
                    List<long> ids = (mediaExternalLink.MediaIds.Split(',')).Select(x => long.Parse(x)).ToList();
                    List<MediaInfo> medias = await _mediaInfoRepository.GetListAsync(x => ids.Contains(x.Id));

                    foreach (var item in medias)
                    {
                        if (item != null && (!string.IsNullOrWhiteSpace(item.SavePath)))
                        {
                            item.IsFileDeleted = true;
                            SpaceHelper.DeleteFile(item.SavePath!);
                        }
                    }

                    SpaceHelper.DeleteEmptyFolders(await GetConfigurationInfo("SavePhotoPathPrefix"));
                    SpaceHelper.DeleteEmptyFolders(await GetConfigurationInfo("SaveVideoPathPrefix"));

                    mediaExternalLink.IsRemove = true;
                    await _externalLinkRepository.UpdateAsync(mediaExternalLink);
                    await _mediaInfoRepository.UpdateManyAsync(medias);

                }
            }
        }

    }
}
