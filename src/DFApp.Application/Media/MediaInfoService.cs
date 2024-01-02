using DFApp.Background;
using DFApp.Helper;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Media
{
    [Authorize(DFAppPermissions.Medias.Default)]
    public class MediaInfoService : CrudAppService<
        MediaInfo,
        MediaInfoDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateMediaInfoDto>, IMediaInfoService
    {
        private readonly IMediaRepository _mediaInfoRepository;
        private readonly IQueueBase<DocumentQueueModel> _documentQueue;
        private readonly IQueueBase<PhotoQueueModel> _photoQueue;
        private readonly List<MediaInfoDto[]> _mediaQueue;

        public MediaInfoService(IMediaRepository repository,
            IQueueBase<DocumentQueueModel> documentQueue,
            IQueueBase<PhotoQueueModel> photoQueue,
            List<MediaInfoDto[]> mediaQueue) : base(repository)
        {
            _mediaInfoRepository = repository;
            _documentQueue = documentQueue;
            _photoQueue = photoQueue;
            GetPolicyName = DFAppPermissions.Medias.Default;
            GetListPolicyName = DFAppPermissions.Medias.Default;
            CreatePolicyName = DFAppPermissions.Medias.Create;
            UpdatePolicyName = DFAppPermissions.Medias.Edit;
            DeletePolicyName = DFAppPermissions.Medias.Delete;
            _mediaQueue = mediaQueue;
        }

        public async Task<MediaInfoDto[]> GetByAccessHashID(MediaInfoDto downloadInfo)
        {
            return ObjectMapper.Map<MediaInfo[], MediaInfoDto[]>(
                await _mediaInfoRepository.GetByAccessHashID(downloadInfo.AccessHash, downloadInfo.TID,downloadInfo.Size));
        }

        public async Task<MediaInfoDto[]> GetByValueSHA1(MediaInfoDto mediaInfoDto)
        {
            return ObjectMapper.Map<MediaInfo[], MediaInfoDto[]>(
                await _mediaInfoRepository.GetByValueSHA1(mediaInfoDto.ValueSHA1));
        }

        public async Task<long> GetDownloadsSize()
        {
            return await _mediaInfoRepository.GetDownloadsSize();
        }

        public QueueCountDto GetQueueCount()
        {
            QueueCountDto queueCountDto = new QueueCountDto();


            queueCountDto.PhotoCount = _photoQueue.GetConcurrentQueueCount();
            queueCountDto.DocumentCount = _documentQueue.GetConcurrentQueueCount();
            queueCountDto.TotalCount = queueCountDto.PhotoCount + queueCountDto.DocumentCount;

            return queueCountDto;
        }

        public async Task<MediaInfoDto[]> GetMediaNotReturn()
        {
            return ObjectMapper.Map<MediaInfo[], MediaInfoDto[]>(await _mediaInfoRepository.GetMediaNotReturn());
        }

        public async Task<List<string>> GetExternalLinkListDownload()
        {
            string returnDownloadUrlPrefix = AppsettingsHelper.app(new string[] { "RunConfig", "ReturnDownloadUrlPrefix" });
            Check.NotNullOrWhiteSpace(returnDownloadUrlPrefix, nameof(returnDownloadUrlPrefix));

            var temp = await _mediaInfoRepository.GetMediaNotReturn();

            if (temp != null && temp.Length > 0)
            {
                _mediaQueue.Add(ObjectMapper.Map<MediaInfo[], MediaInfoDto[]>(temp));
                return temp.Select(x => returnDownloadUrlPrefix + x.SavePath.Replace("./Telegram/", string.Empty).Replace("\\", "/")).ToList();
            }
            return new List<string>();
        }

        public async Task<string> GetExternalLinkDownload()
        {
            string returnDownloadUrlPrefix = AppsettingsHelper.app(new string[] { "RunConfig", "ReturnDownloadUrlPrefix" });
            Check.NotNullOrWhiteSpace(returnDownloadUrlPrefix, nameof(returnDownloadUrlPrefix));

            string photoSavePath = AppsettingsHelper.app("RunConfig", "SavePhotoPathPrefix");
            Check.NotNullOrWhiteSpace(photoSavePath, nameof(photoSavePath));

            var temp = await _mediaInfoRepository.GetMediaNotReturn();

            string zipPhotoName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.zip";
            string zipPhotoPathName = Path.Combine(Path.GetDirectoryName(photoSavePath)!, zipPhotoName);

            ZipFile.CreateFromDirectory(photoSavePath, zipPhotoPathName);

            StringBuilder stringBuilder = new StringBuilder();
            if (File.Exists(zipPhotoPathName))
            {
                stringBuilder.AppendLine(Path.Combine(returnDownloadUrlPrefix, zipPhotoName));
                _mediaQueue.Add(new MediaInfoDto[] {new MediaInfoDto() { SavePath=zipPhotoPathName} });
            }
            foreach (var mediaInfo in temp)
            {
                if (mediaInfo.SavePath == null || Path.GetExtension(mediaInfo.SavePath).Equals(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                stringBuilder.AppendLine($"{returnDownloadUrlPrefix}{mediaInfo.SavePath.Replace("../Telegram/", string.Empty).Replace("\\", "/")}");
            }

            if (temp != null && temp.Length > 0)
            {
                _mediaQueue.Add(ObjectMapper.Map<MediaInfo[], MediaInfoDto[]>(temp));

                return stringBuilder.ToString();
            }
            return string.Empty;
        }

        public async Task<string> MoveDownloaded()
        {
            try
            {
                if (_mediaQueue != null)
                {
                    Logger.LogInformation("Retrieve local to start deletion");
                    foreach (var item in _mediaQueue)
                    {
                        if (item != null && item.Length > 0)
                        {
                            var ids = item.Select(x => x.Id).ToList();
                            _documentQueue.Clear();
                            await _mediaInfoRepository.DeleteManyAsync(ids);
                            foreach (var item2 in item)
                            {
                                SpaceHelper.DeleteFile(item2.SavePath);
                            }
                        }
                    }
                    Logger.LogInformation("Fetch Local Delete Complete");
                    return "Success";
                }
                else
                {
                    return "None to be deleted";
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return ex.Message;
            }
        }

        public async Task<ChartDataDto> GetChartData()
        {
            List<MediaInfo> lms = await _mediaInfoRepository.GetAllTitleNotNullContainSoftDelete();
            var temp = lms.GroupBy(item => item.Title)
                .Select(item =>
                new
                {
                    Title = item.Key,
                    Count = item.Count()
                });

            ChartDataDto dto = new ChartDataDto();
            dto.Labels = new List<string>(temp.Count());
            dto.Datas = new List<int>(temp.Count());
            foreach (var item in temp)
            {
                dto.Labels.Add(item.Title!);
                dto.Datas.Add(item.Count);
            }
            return dto;
        }
    }
}
