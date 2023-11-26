using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DF.Telegram.Lottery
{
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
