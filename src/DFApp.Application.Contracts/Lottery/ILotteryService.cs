using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DFApp.Lottery.Statistics;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Lottery
{
    public interface ILotteryService : ICrudAppService<
        LotteryDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotteryDto>
    {
        Task<LotteryDto> CreateLotteryBatch(List<CreateUpdateLotteryDto> dtos);
        Task<List<StatisticsWinDto>> GetStatisticsWin(string? purchasedPeriod, string? winningPeriod);
        Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItem(string? purchasedPeriod, string? winningPeriod);
        Task<List<LotteryDto>> CalculateCombination(LotteryCombinationDto dto);
    }

}
