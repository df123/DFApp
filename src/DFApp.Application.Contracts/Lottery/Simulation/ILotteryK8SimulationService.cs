using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Lottery.Simulation
{
    public interface ILotteryK8SimulationService : ICrudAppService<
        LotterySimulationDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotterySimulationDto>
    {
        Task<bool> GenerateRandomNumbersAsync(GenerateRandomNumbersDto input);
        Task<WinningStatisticsDto> CalculateWinningAmountAsync(int termNumber);
        Task DeleteByTermNumberAsync(int termNumber);
    }
}
