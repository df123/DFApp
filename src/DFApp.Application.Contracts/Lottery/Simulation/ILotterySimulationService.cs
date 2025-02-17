using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Lottery.Simulation
{
    public interface ILotterySimulationService : ICrudAppService<
        LotterySimulationDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotterySimulationDto>
    {
        Task<bool> GenerateRandomNumbersAsync(GenerateRandomNumbersDto input);
        Task<StatisticsDto> GetStatisticsAsync();
        Task DeleteByTermNumberAsync(int termNumber);
    }
}
