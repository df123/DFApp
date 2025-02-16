using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Lottery.Simulation
{
    [Authorize(DFAppPermissions.Lottery.Default)]
    public class LotterySimulationService : CrudAppService<
        LotterySimulation,
        LotterySimulationDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotterySimulationDto>, ILotterySimulationService
    {
        public LotterySimulationService(IRepository<LotterySimulation, long> repository) : base(repository)
        {
            GetPolicyName = DFAppPermissions.Lottery.Default;
            GetListPolicyName = DFAppPermissions.Lottery.Default;
            CreatePolicyName = DFAppPermissions.Lottery.Create;
            UpdatePolicyName = DFAppPermissions.Lottery.Edit;
            DeletePolicyName = DFAppPermissions.Lottery.Delete;
        }
    }
}
