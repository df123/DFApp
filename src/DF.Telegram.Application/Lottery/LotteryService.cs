﻿using DF.Telegram.Lottery.Statistics;
using DF.Telegram.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace DF.Telegram.Lottery
{
    [Authorize(TelegramPermissions.Lottery.Default)]
    public class LotteryService : CrudAppService<
        LotteryInfo,
        LotteryDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotteryDto>, ILotteryService
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultrepository;
        private readonly IRepository<LotteryInfo, long> _lotteryInforepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public LotteryService(
            IRepository<LotteryInfo, long> repository
            , IRepository<LotteryResult, long> lotteryResultrepository
            , IUnitOfWorkManager unitOfWorkManager) : base(repository)
        {
            GetPolicyName = TelegramPermissions.Lottery.Default;
            GetListPolicyName = TelegramPermissions.Lottery.Default;
            CreatePolicyName = TelegramPermissions.Lottery.Create;
            UpdatePolicyName = TelegramPermissions.Lottery.Edit;
            DeletePolicyName = TelegramPermissions.Lottery.Delete;

            _lotteryResultrepository = lotteryResultrepository;
            _lotteryInforepository = repository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItem(string? purchasedPeriod, string? winningPeriod)
        {
            PagedResultDto<StatisticsWinItemDto> pagedResultDto = new PagedResultDto<StatisticsWinItemDto>();
            List<LotteryResult> lotteryResults = await GetLotteryResultData(purchasedPeriod, winningPeriod);
            List<LotteryInfo> info = await GetLotteryInfoData(purchasedPeriod, winningPeriod);

            var infoGroup = info.GroupBy(item => item.IndexNo);

            List<StatisticsWinItemDto> results = new List<StatisticsWinItemDto>();

            foreach (var item in infoGroup)
            {
                List<LotteryInfo> tempList = item.OrderBy(o => o.Id).ToList();

                var groupIdList = tempList.GroupBy(x => x.GroupId);

                foreach (var groupId in groupIdList)
                {
                    List<LotteryInfo> lotteryNumbers = groupId.OrderBy(x => x.Id).ToList();
                    foreach (LotteryResult lotteryResultItem in lotteryResults)
                    {
                        int redWin = 0;
                        StatisticsWinItemDto winDto = new StatisticsWinItemDto();
                        winDto.Code = item.Key.ToString();
                        winDto.WinCode = lotteryResultItem.Code;
                        winDto.WinAmount = 0;
                        string[] reds = lotteryResultItem.Red!.Split(',');


                        winDto.BuyLottery.Reds.AddRange((lotteryNumbers.Where(x => x.ColorType != "1").Select(x => x.Number).ToArray())!);
                        winDto.WinLottery.Reds.AddRange(reds);
                        winDto.BuyLottery.Blue = lotteryNumbers.First(x => x.ColorType == "1").Number!;
                        winDto.WinLottery.Blue = lotteryResultItem.Blue!;

                        foreach (string red in reds)
                        {
                            foreach (LotteryInfo lotteryItem in lotteryNumbers)
                            {
                                if (lotteryItem.ColorType == "1")
                                {
                                    continue;
                                }
                                if (red == lotteryItem.Number)
                                {
                                    redWin++;
                                    break;
                                }
                            }
                        }

                        int winMoney = int.Parse(JudgeWin(redWin, lotteryResultItem.Blue == winDto.BuyLottery.Blue));
                        if (winMoney > 0)
                        {
                            winDto.WinAmount += winMoney;
                            winDto.BuyLotteryString = string.Join(",", string.Join(",", winDto.BuyLottery.Reds), winDto.BuyLottery.Blue);
                            winDto.WinLotteryString = string.Join(",", string.Join(",", winDto.WinLottery.Reds), winDto.WinLottery.Blue);
                            results.Add(winDto);
                        }

                    }
                }
            }
            pagedResultDto.Items = results;
            pagedResultDto.TotalCount = pagedResultDto.Items.Count;
            return pagedResultDto;
        }

        public async Task<List<StatisticsWinDto>> GetStatisticsWin(string? purchasedPeriod, string? winningPeriod)
        {
            List<LotteryResult> lotteryResults = await GetLotteryResultData(purchasedPeriod, winningPeriod);
            List<LotteryInfo> info = await GetLotteryInfoData(purchasedPeriod, winningPeriod);

            var infoGroup = info.GroupBy(item => item.IndexNo);

            List<StatisticsWinDto> results = new List<StatisticsWinDto>();

            foreach (var item in infoGroup)
            {
                StatisticsWinDto winDto = new StatisticsWinDto();
                winDto.Code = item.Key.ToString();
                winDto.BuyAmount = item.Where(v => v.ColorType == "1").Count() * 2 * lotteryResults.Count;
                winDto.WinAmount = 0;
                List<LotteryInfo> tempList = item.OrderBy(o => o.Id).ToList();

                var groupIdList = tempList.GroupBy(x => x.GroupId);

                foreach (var groupId in groupIdList)
                {
                    List<LotteryInfo> lotteryNumbers = groupId.OrderBy(x => x.Id).ToList();

                    foreach (LotteryResult lotteryResultItem in lotteryResults)
                    {
                        int redWin = 0;
                        string[] reds = lotteryResultItem.Red!.Split(',');

                        foreach (string s in reds)
                        {
                            foreach (LotteryInfo lotteryItem in lotteryNumbers)
                            {
                                if (lotteryItem.ColorType == "1")
                                {
                                    continue;
                                }
                                if (s == lotteryItem.Number)
                                {
                                    redWin++;
                                    break;
                                }
                            }
                        }

                        LotteryInfo blueLotteryInfo = lotteryNumbers.First(x => x.ColorType == "1");

                        int winMoney = int.Parse(JudgeWin(redWin, lotteryResultItem.Blue == blueLotteryInfo.Number));
                        if (winMoney > 0)
                        {
                            winDto.WinAmount += winMoney;
                        }
                    }
                }
                results.Add(winDto);
            }
            return results;
        }

        private async Task<List<LotteryResult>> GetLotteryResultData(string? purchasedPeriod, string? winningPeriod)
        {
            List<LotteryResult> lotteryResults;

            if (!string.IsNullOrWhiteSpace(winningPeriod))
            {
                lotteryResults = await _lotteryResultrepository.GetListAsync(x => x.Code == winningPeriod);
            }
            else
            {
                lotteryResults = await _lotteryResultrepository.GetListAsync();
            }

            return lotteryResults;
        }

        private async Task<List<LotteryInfo>> GetLotteryInfoData(string? purchasedPeriod, string? winningPeriod)
        {
            List<LotteryInfo> info;

            if (!string.IsNullOrWhiteSpace(purchasedPeriod) && int.TryParse(purchasedPeriod, out int purchasedPeriodInt))
            {
                info = await _lotteryInforepository.GetListAsync(x => x.IndexNo == purchasedPeriodInt);
            }
            else
            {
                info = await _lotteryInforepository.GetListAsync();
            }

            return info;
        }


        private string JudgeWin(int redWinCounts, bool blueWin)
        {
            if (blueWin)
            {
                switch (redWinCounts)
                {
                    case 6:
                        return "5000000";
                    case 5:
                        return "3000";
                    case 4:
                        return "200";
                    case 3:
                        return "10";
                    case 0:
                    case 1:
                    case 2:
                        return "5";
                    default:
                        return "-2";

                }
            }
            else
            {
                switch (redWinCounts)
                {
                    case 6:
                        return "100000";
                    case 5:
                        return "200";
                    case 4:
                        return "10";
                    default:
                        return "-2";
                }
            }
        }


        [Authorize(TelegramPermissions.Lottery.Create)]
        public async Task<LotteryDto> CreateLotteryBatch(List<CreateUpdateLotteryDto> dtos)
        {
            Check.NotNullOrEmpty(dtos, nameof(dtos));
            List<LotteryInfo> info = ObjectMapper.Map<List<CreateUpdateLotteryDto>, List<LotteryInfo>>(dtos);
            LotteryInfo? startInfo = (await _lotteryInforepository.GetQueryableAsync()).OrderByDescending(item => item.Id).FirstOrDefault();

            using (var uom = _unitOfWorkManager.Begin(true, true))
            {
                try
                {
                    await _lotteryInforepository.InsertManyAsync(info);
                    await uom.CompleteAsync();
                }
                catch (System.Exception)
                {
                    await uom.RollbackAsync();
                    throw;
                }

            }

            LotteryInfo endInfo = (await _lotteryInforepository.GetQueryableAsync()).OrderByDescending(item => item.Id).First();

            if (startInfo == null || startInfo.Id < endInfo.Id)
            {
                return ObjectMapper.Map<LotteryInfo, LotteryDto>(endInfo);
            }
            else
            {
                throw new System.Exception("添加数据失败!");
            }
        }

        [Authorize(TelegramPermissions.Lottery.Create)]
        public async Task<List<LotteryDto>> CalculateCombination(LotteryCombinationDto dto)
        {
            Check.NotNull(dto.Reds, nameof(dto.Reds));
            Check.NotNull(dto.Blues, nameof(dto.Blues));

            if (dto.Blues.Count <= 0 || dto.Reds.Count <= 0 || dto.Period <= 2013000)
            {
                throw new ArgumentException(nameof(dto));
            }

            LotteryInfo? infoGroupId = (await _lotteryInforepository.GetQueryableAsync()).Where(x => x.IndexNo == dto.Period).OrderByDescending(x => x.GroupId).FirstOrDefault();

            int groupId = 0;
            if (infoGroupId != null)
            {
                groupId = infoGroupId.GroupId + 1;
            }

            List<LotteryInfo> infos = new List<LotteryInfo>();

            for (int m = 0; m < dto.Blues.Count; m++)
            {
                for (var i = 0; i < dto.Reds.Count; i++)
                {

                    infos.Add(new LotteryInfo()
                    {
                        IndexNo = dto.Period,
                        Number = dto.Blues[m],
                        ColorType = "1",
                        GroupId = groupId
                    });

                    for (int j = 0, n = i; j < 6; j++)
                    {

                        int indexRed = 0;
                        if ((n + j) >= dto.Reds.Count)
                        {
                            indexRed = (n + j) - dto.Reds.Count;
                        }
                        else
                        {
                            indexRed = n + j;
                        }

                        infos.Add(new LotteryInfo()
                        {
                            IndexNo = dto.Period,
                            Number = dto.Reds[indexRed],
                            ColorType = "0",
                            GroupId = groupId
                        });
                    }

                    groupId++;
                }
            }

            if (infos.Count > 0)
            {
                using (var uom = _unitOfWorkManager.Begin(true, true))
                {
                    try
                    {
                        await _lotteryInforepository.InsertManyAsync(infos);
                        await uom.CompleteAsync();
                    }
                    catch (Exception)
                    {
                        await uom.RollbackAsync();
                        throw;
                    }
                }
            }

            List<LotteryInfo> returnInfos = await _lotteryInforepository.GetListAsync(x => x.IndexNo == dto.Period && x.GroupId >= (infoGroupId == null ? 0 : infoGroupId.GroupId));

            return ObjectMapper.Map<List<LotteryInfo>, List<LotteryDto>>(returnInfos);

        }


    }
}
