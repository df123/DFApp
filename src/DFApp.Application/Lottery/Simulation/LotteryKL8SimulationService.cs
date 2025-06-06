using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using DFApp.Lottery.Consts;
using DFApp.Lottery.Simulation.KL8;

namespace DFApp.Lottery.Simulation
{
    [Authorize(DFAppPermissions.Lottery.Default)]
    public class LotteryKL8SimulationService : CrudAppService<
        LotterySimulation,
        LotterySimulationDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotterySimulationDto>, ILotteryKL8SimulationService
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultRepository;
        private readonly IRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;

        public LotteryKL8SimulationService(
            IRepository<LotterySimulation, Guid> repository,
            IRepository<LotteryResult, long> lotteryResultRepository,
            IRepository<LotteryPrizegrades, long> lotteryPrizegradesRepository) : base(repository)
        {
            GetPolicyName = DFAppPermissions.Lottery.Default;
            GetListPolicyName = DFAppPermissions.Lottery.Default;
            CreatePolicyName = DFAppPermissions.Lottery.Create;
            UpdatePolicyName = DFAppPermissions.Lottery.Edit;
            DeletePolicyName = DFAppPermissions.Lottery.Delete;
            _lotteryResultRepository = lotteryResultRepository;
            _lotteryPrizegradesRepository = lotteryPrizegradesRepository;
        }

        public async Task<bool> GenerateRandomNumbersAsync(GenerateRandomNumbersDto input)
        {
            var random = new Random();
            var result = new List<LotterySimulation>();

            var maxGroupIdLinq = (await Repository.GetQueryableAsync())
                .Where(x => x.TermNumber == input.TermNumber)
                .Select(x => (int?)x.GroupId);
            var maxGroupId = await AsyncExecuter.MaxAsync(maxGroupIdLinq) ?? 0;

            var groupId = maxGroupId + 1;

            // 遍历所有玩法类型
            for (int playType = 1; playType <= 10; playType++)
            {
                for (int i = 0; i < input.Count; i++)
                {
                    var numbers = new HashSet<int>();
                    // 根据玩法生成对应数量的不重复号码(1-80)
                    while (numbers.Count < playType)
                    {
                        numbers.Add(random.Next(1, 81));
                    }

                    foreach (var number in numbers.OrderBy(x => x))
                    {
                        result.Add(new LotterySimulation
                        {
                            GameType = LotteryGameType.快乐8,
                            BallType = LotteryBallType.Red,
                            Number = number,
                            GroupId = groupId,
                            TermNumber = input.TermNumber
                        });
                    }

                    groupId++;
                }
            }

            await Repository.InsertManyAsync(result);
            return true;
        }

        public async Task<WinningStatisticsDto> CalculateWinningAmountAsync(int termNumber)
        {
            var lotteryResult = await _lotteryResultRepository.FirstOrDefaultAsync(x =>
                x.Code == termNumber.ToString() && x.Name == LotteryConst.KL8);

            if (lotteryResult == null)
            {
                return new WinningStatisticsDto { TotalAmount = 0 };
            }

            var simulationGroups = (await Repository.GetListAsync(x => x.TermNumber == termNumber && x.GameType == LotteryGameType.快乐8))
                .GroupBy(x => x.GroupId);

            var statistics = new WinningStatisticsDto
            {
                WinningDetails = new List<WinningDetailDto>()
            };

            var winningNumbers = lotteryResult.Red!.Split(',').Select(int.Parse).ToList();

            foreach (var group in simulationGroups)
            {
                var selectedNumbers = group.Select(x => x.Number).ToList();
                var matchCount = selectedNumbers.Intersect(winningNumbers).Count();

                var detail = new WinningDetailDto
                {
                    GroupId = group.Key,
                    RedMatches = matchCount,
                    WinningAmount = await CalculateK8Prize(termNumber.ToString(), selectedNumbers.Count(), matchCount)
                };

                statistics.WinningDetails.Add(detail);
                statistics.TotalAmount += detail.WinningAmount;
            }

            return statistics;
        }

        private async Task<decimal> CalculateK8Prize(string termNumber, int selectedCount, int matchCount)
        {
            // 获取该期奖金设置
            var result = await _lotteryResultRepository.FirstOrDefaultAsync(x =>
                x.Code == termNumber && x.Name == LotteryConst.KL8);
            if (result == null) return 0;

            var prizegrades = await _lotteryPrizegradesRepository.GetListAsync(x =>
                x.LotteryResultId == result.Id && x.Type == $"x{selectedCount}z{matchCount}");

            // 构造枚举名称
            var name = $"x{selectedCount}z{matchCount}";

            return decimal.Parse(prizegrades.FirstOrDefault(x => x.Type == name)?.TypeMoney ?? "0");
        }

        public override async Task<PagedResultDto<LotterySimulationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await Repository.GetQueryableAsync();
            
            // 按组分组并计算每组的号码数量
            var groupCounts = await AsyncExecuter.ToListAsync(
                query.Where(x => x.GameType == LotteryGameType.快乐8)
                .GroupBy(x => new { x.TermNumber, x.GroupId })
                .Select(g => new { Count = g.Count() }));

            // 根据号码数量计算实际组数
            var totalCount = groupCounts.Sum(g => g.Count switch
            {
                10 => 1,
                9 => 1,
                8 => 1,
                7 => 1,
                6 => 1,
                5 => 1,
                4 => 1,
                3 => 1,
                2 => 1,
                1 => 1,
                _ => 0
            });

            var groupedData = await AsyncExecuter.ToListAsync(
                query.Where(x => x.GameType == LotteryGameType.快乐8)
                .GroupBy(x => new { x.TermNumber, x.GroupId, x.GameType })
                .Select(g => new
                {
                    g.Key.TermNumber,
                    g.Key.GroupId,
                    g.Key.GameType,
                    Numbers = string.Join(",", g.OrderBy(x => x.Number).Select(x => x.Number.ToString("D2")))
                })
                .OrderByDescending(x => x.TermNumber)
                .ThenBy(x => x.GroupId)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

            var items = groupedData.Select(g => new LotterySimulationDto
            {
                TermNumber = g.TermNumber,
                GroupId = g.GroupId,
                GameType = g.GameType,
                RedNumbers = g.Numbers,
            }).ToList();

            return new PagedResultDto<LotterySimulationDto>(totalCount, items);
        }

        public async Task DeleteByTermNumberAsync(int termNumber)
        {
            await Repository.DeleteAsync(x => x.TermNumber == termNumber);
        }

        public async Task<StatisticsDto> GetStatisticsAsync()
        {
            var statistics = new StatisticsDto();
            var simulations = await Repository.GetListAsync(x => x.GameType == LotteryGameType.快乐8);
            
            // 初始化所有玩法类型的数据列表
            foreach (LotteryKL8PlayType playType in Enum.GetValues(typeof(LotteryKL8PlayType)))
            {
                statistics.PurchaseAmountsByType[playType] = new List<decimal>();
                statistics.WinningAmountsByType[playType] = new List<decimal>();
            }

            var groupedByTerm = simulations
                .GroupBy(x => x.TermNumber)
                .OrderBy(x => x.Key);

            foreach (var term in groupedByTerm)
            {
                if (!statistics.Terms.Contains(term.Key))
                {
                    statistics.Terms.Add(term.Key);
                }

                // 按玩法类型分组统计
                foreach (LotteryKL8PlayType playType in Enum.GetValues(typeof(LotteryKL8PlayType)))
                {
                    var numbersInGroup = (int)playType;
                    var groupsForPlayType = term.GroupBy(x => x.GroupId)
                        .Where(g => g.Count() == numbersInGroup);

                    // 计算该玩法的投注金额（每注2元）
                    var purchaseAmount = groupsForPlayType.Count() * 2m;
                    statistics.PurchaseAmountsByType[playType].Add(purchaseAmount);

                    // 计算该玩法的中奖金额
                    decimal winningAmount = 0;
                    foreach (var group in groupsForPlayType)
                    {
                        var numbers = group.Select(x => x.Number).ToList();
                        var matchCount = await CalculateMatchCount(term.Key, numbers);
                        winningAmount += await CalculateK8Prize(term.Key.ToString(), numbersInGroup, matchCount);
                    }
                    statistics.WinningAmountsByType[playType].Add(winningAmount);
                }
            }

            return statistics;
        }

        private async Task<int> CalculateMatchCount(int termNumber, List<int> selectedNumbers)
        {
            var lotteryResult = await _lotteryResultRepository.FirstOrDefaultAsync(x =>
                x.Code == termNumber.ToString() && x.Name == LotteryConst.KL8);

            if (lotteryResult == null)
                return 0;

            var winningNumbers = lotteryResult.Red!.Split(',').Select(int.Parse).ToList();
            return selectedNumbers.Intersect(winningNumbers).Count();
        }
    }
}
