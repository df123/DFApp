﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Media
{
    public interface IMediaInfoService : ICrudAppService<
        MediaInfoDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateMediaInfoDto>
    {
        Task<MediaInfoDto[]> GetByAccessHashID(MediaInfoDto mediaInfoDto);

        Task<MediaInfoDto[]> GetByValueSHA1(MediaInfoDto mediaInfoDto);

        Task<long> GetDownloadsSize();

        Task<MediaInfoDto[]> GetMediaNotReturn();

        QueueCountDto GetQueueCount();

        Task<ChartDataDto> GetChartData();

    }
}
