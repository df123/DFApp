using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DFApp.Lottery.Consts;
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
        Task<List<StatisticsWinDto>> GetStatisticsWin(string? purchasedPeriod, string? winningPeriod,string lotteryType);
        Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItemInputDto(StatisticsInputDto dto);
        Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItem(StatisticsWinItemRequestDto input);
        Task<List<LotteryDto>> CalculateCombination(LotteryCombinationDto dto);
        List<ConstsDto> GetLotteryConst();
        Task<PagedResultDto<LotteryGroupDto>> GetListGrouped(PagedAndSortedResultRequestDto input);
        Task<int> GetLatestIndexNoByType(string lotteryType);
        Task DeleteLotteryGroupByIndexNoAndGroupId(int indexNo, long groupId);
    }

}
