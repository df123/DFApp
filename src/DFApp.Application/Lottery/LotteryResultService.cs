using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Lottery
{
    [Authorize(DFAppPermissions.Lottery.Default)]
    public class LotteryResultService : CrudAppService<
        LotteryResult
        , LotteryResultDto
        , long
        , PagedAndSortedResultRequestDto
        , CreateUpdateLotteryResultDto>, ILotteryResultService
    {
        public LotteryResultService(IRepository<LotteryResult, long> repository) : base(repository)
        {
        }
    }
}
