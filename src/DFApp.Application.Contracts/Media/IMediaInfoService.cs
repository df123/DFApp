using System.Collections.Generic;
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

        Task<long> GetDownloadsSize();

        Task<MediaInfoDto[]> GetMediaNotReturn();

        QueueCountDto GetQueueCount();

        Task<ChartDataDto> GetChartData();

    }
}
