using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.Lottery
{
    public interface ILotteryResultService :ICrudAppService<
        LotteryResultDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotteryResultDto
        >
    {
    }
}
