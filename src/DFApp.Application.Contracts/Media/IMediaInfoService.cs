using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Bookkeeping.Expenditure;
using Volo.Abp.Application.Dtos;
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
