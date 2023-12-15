using DF.Telegram.Lottery.Statistics;
using DF.Telegram.Permissions;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

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

        public LotteryService(
            IRepository<LotteryInfo, long> repository
            , IRepository<LotteryResult, long> lotteryResultrepository) : base(repository)
        {
            GetPolicyName = TelegramPermissions.Lottery.Default;
            GetListPolicyName = TelegramPermissions.Lottery.Default;
            CreatePolicyName = TelegramPermissions.Lottery.Create;
            UpdatePolicyName = TelegramPermissions.Lottery.Edit;
            DeletePolicyName = TelegramPermissions.Lottery.Delete;

            _lotteryResultrepository = lotteryResultrepository;
            _lotteryInforepository = repository;
        }

        public async Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItem()
        {
            PagedResultDto<StatisticsWinItemDto> pagedResultDto = new PagedResultDto<StatisticsWinItemDto>();
            List<LotteryResult> lotteryResults = await _lotteryResultrepository.GetListAsync();
            List<LotteryInfo> info = await _lotteryInforepository.GetListAsync();

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

        public async Task<List<StatisticsWinDto>> GetStatisticsWin()
        {
            List<LotteryResult> lotteryResults = await _lotteryResultrepository.GetListAsync();
            List<LotteryInfo> info = await _lotteryInforepository.GetListAsync();

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

        public async Task<LotteryDto> CreateLotteryBatch(List<CreateUpdateLotteryDto> dtos)
        {
            Check.NotNullOrEmpty(dtos, nameof(dtos));
            LotteryInfo infoStart = (await _lotteryInforepository.GetQueryableAsync()).OrderByDescending(item => item.IndexNo).First();
            List<LotteryInfo> info = ObjectMapper.Map<List<CreateUpdateLotteryDto>, List<LotteryInfo>>(dtos);
            await _lotteryInforepository.InsertManyAsync(info);
            LotteryInfo infoEnd = (await _lotteryInforepository.GetQueryableAsync()).OrderByDescending(item => item.IndexNo).First();

            if (infoEnd.Id > infoStart.Id)
            {
                return ObjectMapper.Map<LotteryInfo, LotteryDto>(infoEnd);
            }

            throw new System.Exception("批量保存失败");

        }
    }
}
