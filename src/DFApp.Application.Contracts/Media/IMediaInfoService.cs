using System.Threading.Tasks;
using DFApp.CommonDtos;
using Volo.Abp.Application.Services;

namespace DFApp.Media
{
    public interface IMediaInfoService : ICrudAppService<
        MediaInfoDto,
        long,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateMediaInfoDto>
    {

        Task<ChartDataDto> GetChartData();

    }
}
