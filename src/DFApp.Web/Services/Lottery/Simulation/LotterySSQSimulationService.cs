using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.DTOs;
using DFApp.Web.DTOs.Lottery;
using DFApp.Web.DTOs.Lottery.Simulation;
using DFApp.Web.DTOs.Lottery.Simulation.SSQ;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using SqlSugar;

namespace DFApp.Web.Services.Lottery.Simulation;

/// <summary>
/// 双色球模拟服务
/// </summary>
public class LotterySSQSimulationService : CrudServiceBase<LotterySimulation, Guid, LotterySimulationDto, CreateUpdateLotterySimulationDto, CreateUpdateLotterySimulationDto>
{
    private readonly LotteryMapper _mapper = new();
    private readonly ISqlSugarRepository<LotteryResult, long> _lotteryResultRepository;
    private readonly ISqlSugarRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;

    public LotterySSQSimulationService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<LotterySimulation, Guid> repository,
        ISqlSugarRepository<LotteryResult, long> lotteryResultRepository,
        ISqlSugarRepository<LotteryPrizegrades, long> lotteryPrizegradesRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _lotteryResultRepository = lotteryResultRepository;
        _lotteryPrizegradesRepository = lotteryPrizegradesRepository;
    }

    /// <summary>
    /// 生成随机号码
    /// </summary>
    public async Task<bool> GenerateRandomNumbersAsync(GenerateRandomNumbersDto input)
    {
        var random = new Random();
        var result = new List<LotterySimulation>();

        // 获取当前期号下最大的 groupId
        var queryable = Repository.GetQueryable();
        var existingGroups = queryable
            .Where(x => x.TermNumber == input.TermNumber)
            .ToList();
        var maxGroupId = existingGroups.Any() ? existingGroups.Max(x => x.GroupId) : 0;

        var groupId = maxGroupId + 1;

        for (int i = 0; i < input.Count; i++)
        {
            // 使用 HashSet 存储已选择的红球号码
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

        await Repository.InsertAsync(result);
        return true;
    }

    /// <summary>
    /// 计算中奖金额
    /// </summary>
    public async Task<WinningStatisticsDto> CalculateWinningAmountAsync(int termNumber)
    {
        // 获取当期开奖结果
        var lotteryResult = await _lotteryResultRepository.GetFirstOrDefaultAsync(x =>
            x.Code == termNumber.ToString() && x.Name == LotteryConst.SSQ);
        if (lotteryResult == null)
        {
            return new WinningStatisticsDto { TotalAmount = 0 };
        }

        // 获取当期投注记录，按组分类
        var simulations = await Repository.GetListAsync(x => x.TermNumber == termNumber && x.GameType == LotteryGameType.双色球);
        var simulationGroups = simulations.GroupBy(x => x.GroupId);

        var statistics = new WinningStatisticsDto
        {
            WinningDetails = new List<WinningDetailDto>()
        };

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
    private async Task<decimal> CalculatePrizeAmount(int redMatches, bool blueMatches, string termNumber)
    {
        // 获取该期奖金设置
        var result = await _lotteryResultRepository.GetFirstOrDefaultAsync(x =>
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

    /// <summary>
    /// 获取统计数据
    /// </summary>
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
    public async Task DeleteByTermNumberAsync(int termNumber)
    {
        await Repository.DeleteAsync(x => x.TermNumber == termNumber);
    }

    /// <summary>
    /// 获取分页列表（按组聚合）
    /// 使用数据库层面 GroupBy 和分页，避免加载全量数据到内存
    /// </summary>
    public async Task<PagedResultDto<LotterySimulationDto>> GetPagedListAsync(int skipCount, int maxResultCount)
    {
        var queryable = Repository.GetQueryable()
            .Where(x => x.GameType == LotteryGameType.双色球);

        // 在数据库层面获取分组总数
        var totalCount = await queryable
            .GroupBy(x => new { x.TermNumber, x.GroupId })
            .CountAsync();

        // 在数据库层面分页获取分组键（按期号降序、组号升序排列）
        var pagedKeys = await queryable
            .GroupBy(x => new { x.TermNumber, x.GroupId })
            .OrderBy(x => x.TermNumber, OrderByType.Desc)
            .OrderBy(x => x.GroupId, OrderByType.Asc)
            .Select(x => new { x.TermNumber, x.GroupId })
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        if (pagedKeys.Count == 0)
        {
            return new PagedResultDto<LotterySimulationDto>(totalCount, new List<LotterySimulationDto>());
        }

        // 只获取分页分组的详细数据，避免加载全量数据
        var termNumbers = pagedKeys.Select(k => k.TermNumber).Distinct().ToList();
        var groupIds = pagedKeys.Select(k => k.GroupId).ToList();

        var details = await Repository.GetListAsync(x =>
            termNumbers.Contains(x.TermNumber) &&
            groupIds.Contains(x.GroupId) &&
            x.GameType == LotteryGameType.双色球);

        // 在内存中组装 DTO（仅处理分页后的少量数据）
        var items = pagedKeys.Select(key =>
        {
            var groupDetails = details.Where(d => d.TermNumber == key.TermNumber && d.GroupId == key.GroupId);
            return new LotterySimulationDto
            {
                TermNumber = key.TermNumber,
                GroupId = key.GroupId,
                GameType = LotteryGameType.双色球,
                RedNumbers = string.Join(",", groupDetails
                    .Where(x => x.BallType == LotteryBallType.Red)
                    .OrderBy(x => x.Number)
                    .Select(x => x.Number.ToString("D2"))),
                BlueNumber = groupDetails.FirstOrDefault(x => x.BallType == LotteryBallType.Blue)?.Number.ToString("D2")
            };
        }).ToList();

        return new PagedResultDto<LotterySimulationDto>(totalCount, items);
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    protected override LotterySimulationDto MapToGetOutputDto(LotterySimulation entity)
    {
        return _mapper.MapToSSQDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    protected override LotterySimulation MapToEntity(CreateUpdateLotterySimulationDto input)
    {
        return _mapper.MapToEntityFromSSQ(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    protected override void MapToEntity(CreateUpdateLotterySimulationDto input, LotterySimulation entity)
    {
        _mapper.MapToEntityFromSSQ(input, entity);
    }
}
