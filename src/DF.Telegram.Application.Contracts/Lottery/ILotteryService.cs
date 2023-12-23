﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DF.Telegram.Lottery.Statistics;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DF.Telegram.Lottery
{
    public interface ILotteryService : ICrudAppService<
        LotteryDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotteryDto>
    {
        Task<LotteryDto> CreateLotteryBatch(List<CreateUpdateLotteryDto> dtos);
        Task<List<StatisticsWinDto>> GetStatisticsWin(string? purchasedPeriod, string? winningPeriod);
        Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItem(string? purchasedPeriod, string? winningPeriod);
        Task<List<LotteryDto>> CalculateCombination(LotteryCombinationDto dto);
    }

}
