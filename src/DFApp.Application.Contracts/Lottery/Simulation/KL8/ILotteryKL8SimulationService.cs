using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Lottery.Simulation.KL8
{
    public interface ILotteryKL8SimulationService : ICrudAppService<
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