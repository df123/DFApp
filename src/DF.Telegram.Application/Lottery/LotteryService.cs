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
                winDto.BuyAmount = item.Where(v => v.ColorType == "1").Count() * 2;
                winDto.WinAmount = 0;
                var tempList = item.OrderBy(o => o.Id).ToList();

                for (var i = 0; i < tempList.Count; i = i + 7)
                {
                    int redWin = 0;
                    foreach (LotteryResult lotteryResultItem in dto)
                    {
                        string[] reds = lotteryResultItem.Red!.Split(',');
                        for (int n = 0; n < reds.Length; n++)
                        {
                            if (reds[n] == tempList[n + i + 1].Number)
                            {
                                redWin++;
                            }
                        }
                        if (lotteryResultItem.Blue == tempList[i].Number)
                        {
                            winDto.WinAmount += int.Parse(JudgeWin(redWin, true));
                        }
                        else
                        {
                            winDto.WinAmount += int.Parse(JudgeWin(redWin, false));
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
