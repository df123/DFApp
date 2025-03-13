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
            int numberCount = (int)input.PlayType;

            for (int i = 0; i < input.Count; i++)
            {
                var numbers = new HashSet<int>();
                // 根据玩法生成对应数量的不重复号码(1-80)
                while (numbers.Count < numberCount)
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

            var simulationGroups = (await Repository.GetListAsync(x => x.TermNumber == termNumber))
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
            var totalCount = await AsyncExecuter.CountAsync(query) / 20; // 每组20个号码

            var groupedData = await AsyncExecuter.ToListAsync(
                query.GroupBy(x => new { x.TermNumber, x.GroupId, x.GameType })
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

        public Task<StatisticsDto> GetStatisticsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
