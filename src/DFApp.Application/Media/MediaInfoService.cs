using DFApp.Background;
using DFApp.Configuration;
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
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly IQueueManagement _queueManagement;
        private readonly string _moduleName;

        public MediaInfoService(IMediaRepository repository,
            IConfigurationInfoRepository configurationInfoRepository,
            IQueueManagement queueManagement) : base(repository)
        {
            _mediaInfoRepository = repository;
            _queueManagement = queueManagement;
            _documentQueue = _queueManagement.AddQueue<DocumentQueueModel>(ListenTelegramConst.DocumentQueue);
            _photoQueue = _queueManagement.AddQueue<PhotoQueueModel>(ListenTelegramConst.PhotoQueue);
            GetPolicyName = DFAppPermissions.Medias.Default;
            GetListPolicyName = DFAppPermissions.Medias.Default;
            CreatePolicyName = DFAppPermissions.Medias.Create;
            UpdatePolicyName = DFAppPermissions.Medias.Edit;
            DeletePolicyName = DFAppPermissions.Medias.Delete;
            _configurationInfoRepository = configurationInfoRepository;
            _moduleName = "DFApp.Media.MediaInfoService";
            
        }

        public async Task<MediaInfoDto[]> GetByAccessHashID(MediaInfoDto downloadInfo)
        {
            return ObjectMapper.Map<MediaInfo[], MediaInfoDto[]>(
                await _mediaInfoRepository.GetByAccessHashID(downloadInfo.AccessHash, downloadInfo.TID, downloadInfo.Size));
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

        private async Task<string> GetConfigurationInfo(string configurationName)
        {
            string v = await _configurationInfoRepository.GetConfigurationInfoValue(configurationName, _moduleName);
            return v;

        }

    }
}
