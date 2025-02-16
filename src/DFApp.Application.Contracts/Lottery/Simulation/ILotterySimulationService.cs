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
        Task<ListResultDto<LotterySimulationDto>> GenerateRandomNumbersAsync(GenerateRandomNumbersDto input);
    }
}
