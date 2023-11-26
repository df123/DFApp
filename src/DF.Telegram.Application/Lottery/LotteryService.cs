using DF.Telegram.Lottery.Statistics;
using DF.Telegram.Management;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DF.Telegram.Lottery
{
    public class LotteryService : CrudAppService<
        LotteryInfo,
        LotteryDto,
        long,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotteryDto>, ILotteryService
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultrepository;
        private readonly IRepository<LotteryInfo, long> _lotteryInforepository;

        public LotteryService(
            IRepository<LotteryInfo, long> repository
            , IRepository<LotteryResult, long> lotteryResultrepository) : base(repository)
        {
            _lotteryResultrepository = lotteryResultrepository;
            _lotteryInforepository = repository;
        }

        public async Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItem()
        {
            PagedResultDto<StatisticsWinItemDto> pagedResultDto = new PagedResultDto<StatisticsWinItemDto>();
            List<LotteryResult> dto = await _lotteryResultrepository.GetListAsync();
            List<LotteryInfo> info = await _lotteryInforepository.GetListAsync();

            var infoGroup = info.GroupBy(item => item.IndexNo);

            List<StatisticsWinItemDto> results = new List<StatisticsWinItemDto>();

            foreach (var item in infoGroup)
            {
                var tempList = item.OrderBy(o => o.Id).ToList();

                for (var i = 0; i < tempList.Count; i = i + 7)
                {
                    foreach (LotteryResult lotteryResultItem in dto)
                    {
                        int redWin = 0;
                        StatisticsWinItemDto winDto = new StatisticsWinItemDto();
                        winDto.Code = item.Key.ToString();
                        winDto.WinCode = lotteryResultItem.Code;
                        winDto.WinAmount = 0;
                        string[] reds = lotteryResultItem.Red!.Split(',');


                        for (int v = 0; v < 6; v++)
                        {
                            winDto.BuyLottery.Reds.Add(tempList[v + i].Number);
                            winDto.WinLottery.Reds.Add(reds[v]);
                            for (int w = 0; w < reds.Length; w++)
                            {
                                if (tempList[v + i].Number == reds[w])
                                {
                                    redWin++;
                                    break;
                                }
                            }

                        }
                        winDto.BuyLottery.Blue = tempList[i + 6].Number;
                        winDto.WinLottery.Blue = lotteryResultItem.Blue;
                        int winMoney = int.Parse(JudgeWin(redWin, lotteryResultItem.Blue == tempList[i + 6].Number));
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

        public async Task<List<StatisticsWinDto>> GetStatisticsWin()
        {
            List<LotteryResult> dto = await _lotteryResultrepository.GetListAsync();
            List<LotteryInfo> info = await _lotteryInforepository.GetListAsync();

            var infoGroup = info.GroupBy(item => item.IndexNo);

            List<StatisticsWinDto> results = new List<StatisticsWinDto>();

            foreach (var item in infoGroup)
            {
                StatisticsWinDto winDto = new StatisticsWinDto();
                winDto.Code = item.Key.ToString();
                winDto.BuyAmount = item.Where(v => v.ColorType == "1").Count() * 2 * dto.Count;
                winDto.WinAmount = 0;
                var tempList = item.OrderBy(o => o.Id).ToList();

                for (var i = 0; i < tempList.Count; i = i + 7)
                {
                    foreach (LotteryResult lotteryResultItem in dto)
                    {
                        int redWin = 0;
                        string[] reds = lotteryResultItem.Red!.Split(',');

                        for (int v = 0; v < 6; v++)
                        {
                            for (int w = 0; w < reds.Length; w++)
                            {
                                if (tempList[v + i].Number == reds[w])
                                {
                                    redWin++;
                                    break;
                                }
                            }
                            
                        }
                        int winMoney = int.Parse(JudgeWin(redWin, lotteryResultItem.Blue == tempList[i + 6].Number));
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

    }
}
