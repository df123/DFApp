using DFApp.Background;
using DFApp.Bookkeeping.Expenditure;
using DFApp.Configuration;
using DFApp.Permissions;
using DFApp.Queue;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateMediaInfoDto>, IMediaInfoService
    {
        private readonly IMediaRepository _mediaInfoRepository;
        private readonly IQueueBase<DocumentQueueModel> _documentQueue;
        private readonly IQueueBase<PhotoQueueModel> _photoQueue;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly IQueueManagement _queueManagement;
        private readonly string _moduleName;
        private readonly IReadOnlyRepository<MediaInfo,long>  _mediaInfoReadOnlyRepository;

        public MediaInfoService(IMediaRepository repository
        , IConfigurationInfoRepository configurationInfoRepository
        , IQueueManagement queueManagement
        , IReadOnlyRepository<MediaInfo,long>  mediaInfoReadOnlyRepository) : base(repository)
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
            _mediaInfoReadOnlyRepository = mediaInfoReadOnlyRepository;
        }

        protected override async Task<IQueryable<MediaInfo>> CreateFilteredQueryAsync(FilterAndPagedAndSortedResultRequestDto input)
        {
            
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var query = await Repository.GetQueryableAsync();
                return query.Where(x => x.MediaId.ToString().Contains(input.Filter)
                || x.ChatTitle.Contains(input.Filter)
                || x.Message!.Contains(input.Filter));
            }
            else
            {
                return await Repository.GetQueryableAsync();
            }

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
            List<MediaInfo> lms = await _mediaInfoRepository.GetListAsync();
            var temp = lms.GroupBy(item => item.ChatTitle)
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
