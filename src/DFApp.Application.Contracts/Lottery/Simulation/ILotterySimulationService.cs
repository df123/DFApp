using System;
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
    }
}
