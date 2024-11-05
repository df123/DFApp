using DFApp.Background;
using DFApp.Configuration;
using DFApp.Helper;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Media.ExternalLink
{
    [Authorize(DFAppPermissions.Medias.Default)]
    public class ExternalLinkService : CrudAppService<MediaExternalLink
        , ExternalLinkDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateExternalLinkDto>, IExternalLinkService
    {

        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public ExternalLinkService(IRepository<MediaExternalLink, long> repository
            , IBackgroundTaskQueue backgroundTaskQueue) : base(repository)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public override Task<ExternalLinkDto> CreateAsync(CreateUpdateExternalLinkDto input)
        {
            throw new UserFriendlyException("此接口不允许使用");
        }

        public override Task<ExternalLinkDto> UpdateAsync(long id, CreateUpdateExternalLinkDto input)
        {
            throw new UserFriendlyException("此接口不允许使用");
        }

        public override async Task DeleteAsync(long id)
        {
            if (id <= 0)
            {
                throw new UserFriendlyException("ID要大于0");
            }

            MediaExternalLink mediaExternalLink = await ReadOnlyRepository.FirstAsync(x => x.Id == id);
            if (!mediaExternalLink.IsRemove)
            {
                await this.RemoveFileAsync(id);
            }

            await base.DeleteAsync(id);
        }

        public Task<bool> GetExternalLink()
        {
            _backgroundTaskQueue.EnqueueTask(async (serviceScopeFactory, cancellationToken) =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                using var scope = serviceScopeFactory.CreateScope();
                var configurationInfoRepository = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
                var mediaInfoRepository = scope.ServiceProvider.GetRequiredService<IRepository<MediaInfo>>();
                var externalLinkRepository = scope.ServiceProvider.GetRequiredService<IRepository<MediaExternalLink>>();

                var returnDownloadUrlPrefix = await configurationInfoRepository.GetConfigurationInfoValue("ReturnDownloadUrlPrefix", MediaBackgroudConst.ModuleName);
                Check.NotNullOrWhiteSpace(returnDownloadUrlPrefix, nameof(returnDownloadUrlPrefix));

                string photoSavePath = await configurationInfoRepository.GetConfigurationInfoValue("SavePhotoPathPrefix", MediaBackgroudConst.ModuleName);
                Check.NotNullOrWhiteSpace(photoSavePath, nameof(photoSavePath));

                string zipType = await configurationInfoRepository.GetConfigurationInfoValue("ZipType", MediaBackgroudConst.ModuleName);

                List<MediaInfo> temp = await mediaInfoRepository.GetListAsync(x => !x.IsExternalLinkGenerated
                    && x.IsDownloadCompleted);


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


                string replaceUrlPrefix = await configurationInfoRepository.GetConfigurationInfoValue("ReplaceUrlPrefix", MediaBackgroudConst.ModuleName);
                foreach (var mediaInfo in temp)
                {

                    if (zipType.Contains(mediaInfo.MimeType) && File.Exists(mediaInfo.SavePath))
                    {
                        archive.CreateEntryFromFile(mediaInfo.SavePath, Path.GetFileName(mediaInfo.SavePath), CompressionLevel.NoCompression);
                        mediaInfo.IsExternalLinkGenerated = true;
                        size += mediaInfo.Size;

                        continue;
                    }

                    stringBuilder.AppendLine($"{Path.Combine(returnDownloadUrlPrefix, mediaInfo.SavePath.Replace(replaceUrlPrefix, string.Empty).Replace("\\", "/"))}");
                    mediaInfo.IsExternalLinkGenerated = true;
                }

                if (File.Exists(zipPhotoPathName))
                {
                    temp.Add(await mediaInfoRepository.InsertAsync(new MediaInfo()
                    {
                        MediaId = Random.Shared.NextInt64(),
                        ChatId = Random.Shared.NextInt64(),
                        ChatTitle = "zip",
                        Size = size,
                        SavePath = zipPhotoPathName,
                        MimeType = "zip",
                        IsExternalLinkGenerated = true,
                        IsDownloadCompleted = true,
                        ConcurrencyStamp = Guid.NewGuid().ToString("N")
                    }));
                }

                if (temp != null && temp.Count > 0)
                {
                    await mediaInfoRepository.UpdateManyAsync(temp);
                    stopwatch.Stop();

                    List<MediaExternalLinkMediaIds> mediaExternalLinkMediaIds = new List<MediaExternalLinkMediaIds>();

                    var mediaExternalLink = new MediaExternalLink()
                    {
                        Name = datetimeName,
                        Size = size,
                        TimeConsumed = stopwatch.ElapsedMilliseconds,
                        IsRemove = false,
                        LinkContent = stringBuilder.ToString(),
                        MediaIds = mediaExternalLinkMediaIds
                    };

                    foreach (var mediaInfo in temp)
                    {
                        mediaExternalLinkMediaIds.Add(new MediaExternalLinkMediaIds()
                        {
                            MediaId = mediaInfo.Id
                        });
                    }


                    await externalLinkRepository.InsertAsync(mediaExternalLink);
                }

            });

            return Task.FromResult(true);
        }

        public Task<bool> RemoveFileAsync(long id)
        {
            if (id <= 0)
            {
                throw new UserFriendlyException("ID要大于0");
            }

            _backgroundTaskQueue.EnqueueTask(async (serviceScopeFactory, cancellationToken) =>
            {

                using var scope = serviceScopeFactory.CreateScope();
                var externalLinkRepository = scope.ServiceProvider.GetRequiredService<IRepository<MediaExternalLink>>();
                var mediaInfoRepository = scope.ServiceProvider.GetRequiredService<IRepository<MediaInfo>>();
                var configurationInfoRepository = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
                var mediaExternalLinkMediaIdRepository = scope.ServiceProvider.GetRequiredService<IRepository<MediaExternalLinkMediaIds>>();


                MediaExternalLink mediaExternalLink = await externalLinkRepository.GetAsync(x => x.Id == id);
                if (!mediaExternalLink.IsRemove)
                {

                    var mediaExternalLinkMediaIds = await mediaExternalLinkMediaIdRepository.GetListAsync(x => x.MediaExternalLinkId == mediaExternalLink.Id);

                    List<long> ids = mediaExternalLinkMediaIds.Select(x => x.MediaId).ToList();
                    List<MediaInfo> medias = await mediaInfoRepository.GetListAsync(x => ids.Contains(x.Id));

                    foreach (var item in medias)
                    {
                        if (item != null && (!string.IsNullOrWhiteSpace(item.SavePath)))
                        {
                            SpaceHelper.DeleteFile(item.SavePath!);
                        }
                    }

                    var savePhotoPathPrefix = await configurationInfoRepository.GetConfigurationInfoValue("SavePhotoPathPrefix", MediaBackgroudConst.ModuleName);
                    var saveVideoPathPrefix = await configurationInfoRepository.GetConfigurationInfoValue("SaveVideoPathPrefix", MediaBackgroudConst.ModuleName);


                    SpaceHelper.DeleteEmptyFolders(savePhotoPathPrefix);
                    SpaceHelper.DeleteEmptyFolders(saveVideoPathPrefix);

                    mediaExternalLink.IsRemove = true;
                    await externalLinkRepository.UpdateAsync(mediaExternalLink);
                    await mediaInfoRepository.UpdateManyAsync(medias);

                }

            });

            return Task.FromResult(true);

        }
    }
}
