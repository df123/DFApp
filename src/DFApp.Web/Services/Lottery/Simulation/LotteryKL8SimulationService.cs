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
using DFApp.Web.DTOs.Lottery.Simulation.KL8;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using SqlSugar;

namespace DFApp.Web.Services.Lottery.Simulation;

/// <summary>
/// 快乐8模拟服务
/// </summary>
public class LotteryKL8SimulationService : CrudServiceBase<LotterySimulation, Guid, LotterySimulationDto, CreateUpdateLotterySimulationDto, CreateUpdateLotterySimulationDto>
{
    private readonly LotteryMapper _mapper = new();
    private readonly ISqlSugarRepository<LotteryResult, long> _lotteryResultRepository;
    private readonly ISqlSugarRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;

    public LotteryKL8SimulationService(
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

        // 遍历所有玩法类型（选1到选10）
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

        await Repository.InsertAsync(result);
        return true;
    }

    /// <summary>
    /// 计算中奖金额
    /// </summary>
    public async Task<WinningStatisticsDto> CalculateWinningAmountAsync(int termNumber)
    {
        var lotteryResult = await _lotteryResultRepository.GetFirstOrDefaultAsync(x =>
            x.Code == termNumber.ToString() && x.Name == LotteryConst.KL8);

        if (lotteryResult == null)
        {
            return new WinningStatisticsDto { TotalAmount = 0 };
        }

        var simulations = await Repository.GetListAsync(x => x.TermNumber == termNumber && x.GameType == LotteryGameType.快乐8);
        var simulationGroups = simulations.GroupBy(x => x.GroupId);

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
                WinningAmount = await CalculateK8Prize(termNumber.ToString(), selectedNumbers.Count, matchCount)
            };

            statistics.WinningDetails.Add(detail);
            statistics.TotalAmount += detail.WinningAmount;
        }

        return statistics;
    }

    /// <summary>
    /// 计算快乐8单注奖金
    /// </summary>
    private async Task<decimal> CalculateK8Prize(string termNumber, int selectedCount, int matchCount)
    {
        var result = await _lotteryResultRepository.GetFirstOrDefaultAsync(x =>
            x.Code == termNumber && x.Name == LotteryConst.KL8);
        if (result == null) return 0;

        var prizegrades = await _lotteryPrizegradesRepository.GetListAsync(x =>
            x.LotteryResultId == result.Id && x.Type == $"x{selectedCount}z{matchCount}");

        var name = $"x{selectedCount}z{matchCount}";
        return decimal.Parse(prizegrades.FirstOrDefault(x => x.Type == name)?.TypeMoney ?? "0");
    }

    /// <summary>
    /// 获取分页列表（按组聚合）
    /// 使用数据库层面 GroupBy 和分页，避免加载全量数据到内存
    /// </summary>
    public async Task<PagedResultDto<LotterySimulationDto>> GetPagedListAsync(int skipCount, int maxResultCount)
    {
        var queryable = Repository.GetQueryable()
            .Where(x => x.GameType == LotteryGameType.快乐8);

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
            x.GameType == LotteryGameType.快乐8);

        // 在内存中组装 DTO（仅处理分页后的少量数据）
        var items = pagedKeys.Select(key =>
        {
            var groupDetails = details.Where(d => d.TermNumber == key.TermNumber && d.GroupId == key.GroupId);
            return new LotterySimulationDto
            {
                TermNumber = key.TermNumber,
                GroupId = key.GroupId,
                GameType = LotteryGameType.快乐8,
                RedNumbers = string.Join(",", groupDetails
                    .OrderBy(x => x.Number)
                    .Select(x => x.Number.ToString("D2")))
            };
        }).ToList();

        return new PagedResultDto<LotterySimulationDto>(totalCount, items);
    }

    /// <summary>
    /// 根据期号删除模拟数据
    /// </summary>
    public async Task DeleteByTermNumberAsync(int termNumber)
    {
        await Repository.DeleteAsync(x => x.TermNumber == termNumber);
    }

    /// <summary>
    /// 获取统计数据
    /// </summary>
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

    /// <summary>
    /// 计算匹配号码数
    /// </summary>
    private async Task<int> CalculateMatchCount(int termNumber, List<int> selectedNumbers)
    {
        var lotteryResult = await _lotteryResultRepository.GetFirstOrDefaultAsync(x =>
            x.Code == termNumber.ToString() && x.Name == LotteryConst.KL8);

        if (lotteryResult == null)
            return 0;

        var winningNumbers = lotteryResult.Red!.Split(',').Select(int.Parse).ToList();
        return selectedNumbers.Intersect(winningNumbers).Count();
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    protected override LotterySimulationDto MapToGetOutputDto(LotterySimulation entity)
    {
        return _mapper.MapToKL8Dto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    protected override LotterySimulation MapToEntity(CreateUpdateLotterySimulationDto input)
    {
        return _mapper.MapToEntityFromKL8(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    protected override void MapToEntity(CreateUpdateLotterySimulationDto input, LotterySimulation entity)
    {
        _mapper.MapToEntityFromKL8(input, entity);
    }
}
