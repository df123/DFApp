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
using DFApp.Lottery.Simulation.SSQ;

namespace DFApp.Lottery.Simulation
{
    [Authorize(DFAppPermissions.Lottery.Default)]
    public class LotterySSQSimulationService : CrudAppService<
        LotterySimulation,
        LotterySimulationDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateLotterySimulationDto>, ILotterySSQSimulationService
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultRepository;
        private readonly IRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;

        public LotterySSQSimulationService(
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

        /// <summary>
        /// 生成随机号码
        /// </summary>
        /// <param name="input">生成参数，包含生成组数、彩票类型和期号</param>
        /// <returns>生成的随机号码列表</returns>
        public async Task<bool> GenerateRandomNumbersAsync(GenerateRandomNumbersDto input)
        {
            var random = new Random();
            var result = new List<LotterySimulation>();

            // 获取当前期号下最大的 groupId
            var maxGroupIdLinq = (await Repository.GetQueryableAsync())
                .Where(x => x.TermNumber == input.TermNumber)
                .Select(x => (int?)x.GroupId);
            var maxGroupId = await AsyncExecuter.MaxAsync(maxGroupIdLinq) ?? 0;
            
            // 新的 groupId 在最大值基础上 +1
            var groupId = maxGroupId + 1;

            for (int i = 0; i < input.Count; i++)
            {
                // 使用HashSet存储已选择的红球号码
                var redBalls = new HashSet<int>();
                // 生成6个不重复的红球(1-33)
                while (redBalls.Count < 6)
                {
                    redBalls.Add(random.Next(1, 34));
                }

                // 添加红球
                foreach (var number in redBalls)
                {
                    result.Add(new LotterySimulation
                    {
                        GameType = input.GameType,
                        BallType = LotteryBallType.Red,
                        Number = number,
                        GroupId = groupId,
                        TermNumber = input.TermNumber
                    });
                }

                // 生成1个蓝球(1-16)
                result.Add(new LotterySimulation
                {
                    GameType = input.GameType,
                    BallType = LotteryBallType.Blue,
                    Number = random.Next(1, 17),
                    GroupId = groupId,
                    TermNumber = input.TermNumber
                });

                groupId++;
            }

            await Repository.InsertManyAsync(result);

            return true;
        }

        /// <summary>
        /// 计算中奖金额
        /// </summary>
        /// <param name="termNumber">期号</param>
        /// <returns>中奖统计结果</returns>
        public async Task<WinningStatisticsDto> CalculateWinningAmountAsync(int termNumber)
        {
            // 获取当期开奖结果
            var lotteryResult = await _lotteryResultRepository.FirstOrDefaultAsync(x => 
                x.Code == termNumber.ToString() && x.Name == LotteryConst.SSQ);
            if (lotteryResult == null)
            {
                return new WinningStatisticsDto { TotalAmount = 0 };
            }

            // 获取当期投注记录，按组分类
            var simulationGroups = (await Repository.GetListAsync(x => x.TermNumber == termNumber))
                .GroupBy(x => x.GroupId);

            var statistics = new WinningStatisticsDto();
            statistics.WinningDetails = new List<WinningDetailDto>();

            foreach (var group in simulationGroups)
            {
                var detail = new WinningDetailDto { GroupId = group.Key };
                
                // 分离红球和蓝球
                var redBalls = group.Where(x => x.BallType == LotteryBallType.Red)
                    .Select(x => x.Number).ToList();
                var blueBall = group.FirstOrDefault(x => x.BallType == LotteryBallType.Blue)?.Number;

                // 计算中奖红球数
                var winningRedBalls = lotteryResult.Red!.Split(',')
                    .Select(int.Parse)
                    .Intersect(redBalls)
                    .ToList();
                
                // 判断蓝球是否中奖
                var winningBlueBall = blueBall.HasValue && 
                    blueBall.Value == int.Parse(lotteryResult.Blue!);

                detail.RedMatches = winningRedBalls.Count;
                detail.BlueMatches = winningBlueBall ? 1 : 0;
                
                // 计算中奖金额
                detail.WinningAmount = await CalculatePrizeAmount(
                    winningRedBalls.Count,
                    winningBlueBall,
                    termNumber.ToString());

                statistics.WinningDetails.Add(detail);
                statistics.TotalAmount += detail.WinningAmount;
            }

            return statistics;
        }

        /// <summary>
        /// 计算具体奖项金额
        /// </summary>
        /// <param name="redMatches">红球匹配数</param>
        /// <param name="blueMatches">蓝球是否匹配</param>
        /// <param name="termNumber">期号</param>
        /// <returns>中奖金额</returns>
        private async Task<decimal> CalculatePrizeAmount(int redMatches, bool blueMatches, string termNumber)
        {
            // 获取该期奖金设置
            var result = await _lotteryResultRepository.FirstOrDefaultAsync(x => 
                x.Code == termNumber && x.Name == LotteryConst.SSQ);
            if (result == null) return 0;

            var prizegrades = await _lotteryPrizegradesRepository.GetListAsync(x => 
                x.LotteryResultId == result.Id);

            // 确定中奖等级
            string? prizeLevel = null;
            if (blueMatches)
            {
                switch (redMatches)
                {
                    case 6: prizeLevel = "1"; break;  // 一等奖
                    case 5: prizeLevel = "3"; break;  // 三等奖
                    case 4: prizeLevel = "4"; break;  // 四等奖
                    case 3: prizeLevel = "5"; break;  // 五等奖
                    case 2:
                    case 1:
                    case 0: prizeLevel = "6"; break;  // 六等奖
                }
            }
            else
            {
                switch (redMatches)
                {
                    case 6: prizeLevel = "2"; break;  // 二等奖
                    case 5: prizeLevel = "4"; break;  // 四等奖
                    case 4: prizeLevel = "5"; break;  // 五等奖
                }
            }

            if (prizeLevel == null) return 0;

            var prize = prizegrades.FirstOrDefault(x => x.Type == prizeLevel);
            return prize != null && decimal.TryParse(prize.TypeMoney, out decimal amount) ? amount : 0;
        }

        public async Task<StatisticsDto> GetStatisticsAsync()
        {
            var statistics = new StatisticsDto();
            var simulations = await Repository.GetListAsync();
            
            var groupedByTerm = simulations
                .GroupBy(x => x.TermNumber)
                .OrderBy(x => x.Key);

            foreach (var term in groupedByTerm)
            {
                statistics.Terms.Add(term.Key);
                // 每注2元
                var purchaseAmount = term.GroupBy(x => x.GroupId).Count() * 2m;
                statistics.PurchaseAmounts.Add(purchaseAmount);
                
                var winningStats = await CalculateWinningAmountAsync(term.Key);
                statistics.WinningAmounts.Add(winningStats.TotalAmount);
            }

            return statistics;
        }

        /// <summary>
        /// 删除指定期号的所有模拟数据
        /// </summary>
        /// <param name="termNumber">期号</param>
        public async Task DeleteByTermNumberAsync(int termNumber)
        {
            await Repository.DeleteAsync(x => x.TermNumber == termNumber);
        }

        public override async Task<PagedResultDto<LotterySimulationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            // 获取所有数据
            var query = await Repository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(query) / 7;

            // 分组查询
            var groupedData = await AsyncExecuter.ToListAsync(
                query.GroupBy(x => new { x.TermNumber, x.GroupId, x.GameType })
                .Select(g => new
                {
                    g.Key.TermNumber,
                    g.Key.GroupId,
                    g.Key.GameType,
                    RedNumbers = string.Join(",", g.Where(x => x.BallType == LotteryBallType.Red)
                                                 .OrderBy(x => x.Number)
                                                 .Select(x => x.Number.ToString("D2"))),
                    BlueNumber = g.FirstOrDefault(x => x.BallType == LotteryBallType.Blue)!.Number.ToString("D2")
                })
                .OrderByDescending(x => x.TermNumber)
                .ThenBy(x => x.GroupId)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

            // 转换为DTO
            var items = groupedData.Select(g => new LotterySimulationDto
            {
                TermNumber = g.TermNumber,
                GroupId = g.GroupId,
                GameType = g.GameType,
                RedNumbers = g.RedNumbers,
                BlueNumber = g.BlueNumber
            }).ToList();

            return new PagedResultDto<LotterySimulationDto>(totalCount, items);
        }
    }
}
