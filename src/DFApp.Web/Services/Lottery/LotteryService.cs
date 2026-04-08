using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.DTOs;
using DFApp.Web.DTOs.Lottery;
using DFApp.Web.DTOs.Lottery.Consts;
using DFApp.Web.DTOs.Lottery.Statistics;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;
using LotteryDto = DFApp.Web.DTOs.Lottery.LotteryDto;
using CreateUpdateLotteryDto = DFApp.Web.DTOs.Lottery.CreateUpdateLotteryDto;
using LotteryCombinationDto = DFApp.Web.DTOs.Lottery.LotteryCombinationDto;
using LotteryGroupDto = DFApp.Web.DTOs.Lottery.LotteryGroupDto;

namespace DFApp.Web.Services.Lottery;

/// <summary>
/// 彩票信息服务
/// </summary>
public class LotteryService : CrudServiceBase<LotteryInfo, long, LotteryDto, CreateUpdateLotteryDto, CreateUpdateLotteryDto>
{
    private readonly LotteryMapper _mapper = new();
    private readonly ISqlSugarRepository<LotteryResult, long> _lotteryResultRepository;
    private readonly ISqlSugarRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;
    private readonly ILogger<LotteryService> _logger;

    public LotteryService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<LotteryInfo, long> repository,
        ISqlSugarRepository<LotteryResult, long> lotteryResultRepository,
        ISqlSugarRepository<LotteryPrizegrades, long> lotteryPrizegradesRepository,
        ILogger<LotteryService> logger)
        : base(currentUser, permissionChecker, repository)
    {
        _lotteryResultRepository = lotteryResultRepository;
        _lotteryPrizegradesRepository = lotteryPrizegradesRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取中奖统计项（分页）
    /// </summary>
    public async Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItem(StatisticsWinItemRequestDto input)
    {
        return await GetStatisticsWinItemInternal(input.PurchasedPeriod, input.WinningPeriod, input.LotteryType, input.SkipCount, input.MaxResultCount);
    }

    /// <summary>
    /// 中奖统计项内部实现
    /// </summary>
    private async Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItemInternal(
        string? purchasedPeriod, string? winningPeriod, string lotteryType, int? skipCount, int? maxResultCount)
    {
        var pagedResultDto = new PagedResultDto<StatisticsWinItemDto>();
        List<LotteryResult> lotteryResults = await GetLotteryResultData(winningPeriod, lotteryType);
        List<LotteryInfo> info = await GetLotteryInfoData(purchasedPeriod, lotteryType);

        var infoGroup = info.GroupBy(item => item.IndexNo);
        var results = new List<StatisticsWinItemDto>();

        foreach (var item in infoGroup)
        {
            var tempList = item.OrderBy(o => o.Id).ToList();
            var groupIdList = tempList.GroupBy(x => x.GroupId);

            foreach (var groupId in groupIdList)
            {
                List<LotteryInfo> lotteryNumbers = groupId.OrderBy(x => x.Id).ToList();
                // 只处理对应期号的中奖结果
                var periodResult = lotteryResults.FirstOrDefault(x => x.Code == item.Key.ToString());
                if (periodResult != null && !string.IsNullOrEmpty(periodResult.Code))
                {
                    int redWin = 0;
                    var winDto = new StatisticsWinItemDto();
                    winDto.Code = item.Key.ToString();
                    winDto.WinCode = periodResult.Code;
                    winDto.WinAmount = 0;
                    string[] reds = periodResult.Red!.Split(',');

                    winDto.BuyLottery.Reds.AddRange(lotteryNumbers.Where(x => x.ColorType != "1").Select(x => x.Number).ToArray());
                    winDto.WinLottery.Reds.AddRange(reds);
                    winDto.BuyLottery.Blue = lotteryNumbers.FirstOrDefault(x => x.ColorType == "1")?.Number;
                    winDto.WinLottery.Blue = periodResult.Blue;

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

                    int winMoney = -2;
                    if (lotteryType == LotteryConst.SSQ)
                    {
                        LotteryInfo blueLotteryInfo = lotteryNumbers.First(x => x.ColorType == "1");
                        winMoney = await JudgeWin(redWin, periodResult.Blue == blueLotteryInfo.Number, periodResult.Code);
                    }
                    else
                    {
                        winMoney = await GetActualAmount(periodResult.Code, lotteryType, 10, redWin);
                    }

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

        // 应用排序
        results = results.OrderByDescending(x => x.Code).ToList();

        // 设置总数
        pagedResultDto.TotalCount = results.Count;

        // 应用分页
        if (skipCount.HasValue && maxResultCount.HasValue)
        {
            pagedResultDto.Items = results.Skip(skipCount.Value).Take(maxResultCount.Value).ToList();
        }
        else
        {
            pagedResultDto.Items = results;
        }

        return pagedResultDto;
    }

    /// <summary>
    /// 获取中奖统计
    /// </summary>
    public async Task<List<StatisticsWinDto>> GetStatisticsWin(string? purchasedPeriod, string? winningPeriod, string lotteryType)
    {
        List<LotteryResult> lotteryResults = await GetLotteryResultData(winningPeriod, lotteryType);
        List<LotteryInfo> info = await GetLotteryInfoData(purchasedPeriod, lotteryType);

        var infoGroup = info.GroupBy(item => item.IndexNo);
        var results = new List<StatisticsWinDto>();

        foreach (var item in infoGroup)
        {
            var tempList = item.OrderBy(o => o.Id).ToList();
            var groupIdList = tempList.GroupBy(x => x.GroupId);

            var winDto = new StatisticsWinDto();
            winDto.Code = item.Key.ToString();
            // 只计算单期对应的购买金额
            winDto.BuyAmount = groupIdList.Count() * 2;
            winDto.WinAmount = 0;

            // 只处理对应期号的中奖结果
            var periodResult = lotteryResults.FirstOrDefault(x => x.Code == item.Key.ToString());
            if (periodResult != null && !string.IsNullOrEmpty(periodResult.Code))
            {
                foreach (var groupId in groupIdList)
                {
                    List<LotteryInfo> lotteryNumbers = groupId.OrderBy(x => x.Id).ToList();
                    int redWin = 0;
                    string[] reds = periodResult.Red!.Split(',');

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

                    int winMoney = -2;
                    if (lotteryType == LotteryConst.SSQ)
                    {
                        LotteryInfo blueLotteryInfo = lotteryNumbers.First(x => x.ColorType == "1");
                        winMoney = await JudgeWin(redWin, periodResult.Blue == blueLotteryInfo.Number, periodResult.Code);
                    }
                    else
                    {
                        winMoney = await GetActualAmount(periodResult.Code, lotteryType, 10, redWin);
                    }

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

    /// <summary>
    /// 获取彩票开奖结果数据
    /// </summary>
    private async Task<List<LotteryResult>> GetLotteryResultData(string? winningPeriod, string lotteryType)
    {
        if (string.IsNullOrWhiteSpace(lotteryType))
            throw new BusinessException(nameof(lotteryType) + " 不能为空");

        List<LotteryResult> lotteryResults;

        if (!string.IsNullOrWhiteSpace(winningPeriod))
        {
            lotteryResults = await _lotteryResultRepository.GetListAsync(x => x.Code == winningPeriod && x.Name == lotteryType);
        }
        else
        {
            lotteryResults = await _lotteryResultRepository.GetListAsync(x => x.Name == lotteryType);
        }

        return lotteryResults;
    }

    /// <summary>
    /// 获取彩票购买信息数据
    /// </summary>
    private async Task<List<LotteryInfo>> GetLotteryInfoData(string? purchasedPeriod, string lotteryType)
    {
        if (string.IsNullOrWhiteSpace(lotteryType))
            throw new BusinessException(nameof(lotteryType) + " 不能为空");

        List<LotteryInfo> info;

        if (!string.IsNullOrWhiteSpace(purchasedPeriod) && int.TryParse(purchasedPeriod, out int purchasedPeriodInt))
        {
            info = await Repository.GetListAsync(x => x.IndexNo == purchasedPeriodInt && x.LotteryType == lotteryType);
        }
        else
        {
            info = await Repository.GetListAsync(x => x.LotteryType == lotteryType);
        }

        return info;
    }

    /// <summary>
    /// 判断双色球中奖金额
    /// </summary>
    private async Task<int> JudgeWin(int redWinCounts, bool blueWin, string winningPeriod)
    {
        if (blueWin)
        {
            switch (redWinCounts)
            {
                case 6:
                    return await GetActualAmount(winningPeriod, LotteryConst.SSQ, 0, 1);
                case 5:
                    return 3000;
                case 4:
                    return 200;
                case 3:
                    return 10;
                case 0:
                case 1:
                case 2:
                    return 5;
            }
        }
        else
        {
            switch (redWinCounts)
            {
                case 6:
                    return await GetActualAmount(winningPeriod, LotteryConst.SSQ, 0, 2);
                case 5:
                    return 200;
                case 4:
                    return 10;
            }
        }
        return 0;
    }

    /// <summary>
    /// 获取实际奖金金额
    /// </summary>
    private async Task<int> GetActualAmount(string winningPeriod, string lotteryType, int selectedCount, int prize)
    {
        if (string.IsNullOrWhiteSpace(winningPeriod))
            throw new BusinessException(nameof(winningPeriod) + " 不能为空");

        var result = await _lotteryResultRepository.GetFirstOrDefaultAsync(x =>
            x.Code == winningPeriod && x.Name == lotteryType);
        if (result == null) return 0;

        var prizegrades = await _lotteryPrizegradesRepository.GetListAsync(x =>
            x.LotteryResultId == result.Id);

        if (prizegrades != null && prizegrades.Count > 0)
        {
            string xType = string.Empty;
            if (LotteryConst.SSQ == lotteryType)
            {
                xType = prize.ToString();
            }
            else
            {
                xType = $"x{selectedCount}z{prize}";
            }
            LotteryPrizegrades? prizegrade = prizegrades.FirstOrDefault(x => x.Type == xType);
            if (prizegrade != null && prizegrade.TypeMoney != null)
            {
                if (int.TryParse(prizegrade.TypeMoney, out int typeMoney))
                {
                    return typeMoney;
                }
                else
                {
                    int sum = 0;
                    MatchCollection matchs = Regex.Matches(prizegrade.TypeMoney, @"\d+");
                    foreach (Match match in matchs)
                    {
                        if (match.Success)
                        {
                            sum += int.Parse(match.Value);
                        }
                    }
                    return sum;
                }
            }
        }
        return 0;
    }

    /// <summary>
    /// 批量创建彩票
    /// </summary>
    public async Task<LotteryDto> CreateLotteryBatch(List<CreateUpdateLotteryDto> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            throw new BusinessException(nameof(dtos) + " 不能为空");

        List<LotteryInfo> info = dtos.Select(d => _mapper.MapToEntity(d)).ToList();

        var queryable = Repository.GetQueryable();
        LotteryInfo? startInfo = queryable
            .Where(x => x.LotteryType == dtos[0].LotteryType && x.IndexNo == dtos[0].IndexNo)
            .OrderByDescending(item => item.Id)
            .ToList()
            .OrderByDescending(item => item.GroupId)
            .FirstOrDefault();

        int groupId = startInfo != null ? startInfo.GroupId + 1 : 0;

        var tempGroups = info.GroupBy(x => x.GroupId);

        foreach (var item in tempGroups)
        {
            foreach (var item2 in item)
            {
                item2.GroupId = groupId;
            }
            groupId++;
        }

        // 使用 SqlSugar 事务替代 IUnitOfWorkManager
        try
        {
            Repository.BeginTran();
            await Repository.InsertAsync(info);
            Repository.CommitTran();
        }
        catch (Exception)
        {
            Repository.RollbackTran();
            throw;
        }

        LotteryInfo endInfo = queryable.OrderByDescending(item => item.Id).First();

        if (startInfo == null || startInfo.Id < endInfo.Id)
        {
            return MapToGetOutputDto(endInfo);
        }
        else
        {
            throw new BusinessException("添加数据失败!");
        }
    }

    /// <summary>
    /// 计算组合投注
    /// </summary>
    public async Task<List<LotteryDto>> CalculateCombination(LotteryCombinationDto dto)
    {
        if (dto.Reds == null)
            throw new BusinessException(nameof(dto.Reds) + " 不能为空");
        if (dto.Blues == null)
            throw new BusinessException(nameof(dto.Blues) + " 不能为空");

        if (dto.Blues.Count <= 0 || dto.Reds.Count <= 0 || dto.Period <= 2013000)
        {
            throw new BusinessException(nameof(dto) + " 参数无效");
        }

        var queryable = Repository.GetQueryable();
        LotteryInfo? infoGroupId = queryable
            .Where(x => x.IndexNo == dto.Period)
            .OrderByDescending(x => x.GroupId)
            .ToList()
            .FirstOrDefault();

        int groupId = 0;
        if (infoGroupId != null)
        {
            groupId = infoGroupId.GroupId + 1;
        }

        var infos = new List<LotteryInfo>();

        for (int m = 0; m < dto.Blues.Count; m++)
        {
            for (var i = 0; i < dto.Reds.Count; i++)
            {
                infos.Add(new LotteryInfo()
                {
                    IndexNo = dto.Period,
                    Number = dto.Blues[m],
                    ColorType = "1",
                    LotteryType = LotteryConst.SSQ,
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
                        LotteryType = LotteryConst.SSQ,
                        GroupId = groupId
                    });
                }

                groupId++;

                if (dto.Reds.Count <= 6)
                {
                    break;
                }
            }
        }

        if (infos.Count > 0)
        {
            try
            {
                Repository.BeginTran();
                await Repository.InsertAsync(infos);
                Repository.CommitTran();
            }
            catch (Exception)
            {
                Repository.RollbackTran();
                throw;
            }
        }

        List<LotteryInfo> returnInfos = await Repository.GetListAsync(x =>
            x.IndexNo == dto.Period && x.GroupId >= (infoGroupId == null ? 0 : infoGroupId.GroupId));

        return returnInfos.Select(MapToGetOutputDto).ToList();
    }

    /// <summary>
    /// 获取彩票常量
    /// </summary>
    public List<ConstsDto> GetLotteryConst()
    {
        var constsDtos = new List<ConstsDto>();
        constsDtos.Add(new ConstsDto()
        {
            LotteryType = LotteryConst.SSQ,
            LotteryTypeEng = LotteryConst.SSQ_ENG
        });
        constsDtos.Add(new ConstsDto()
        {
            LotteryType = LotteryConst.KL8,
            LotteryTypeEng = LotteryConst.KL8_ENG
        });
        return constsDtos;
    }

    /// <summary>
    /// 获取中奖统计项（通过输入 DTO）
    /// </summary>
    public async Task<PagedResultDto<StatisticsWinItemDto>> GetStatisticsWinItemInputDto(StatisticsInputDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.PurchasedPeriod) && string.IsNullOrWhiteSpace(dto.WinningPeriod))
        {
            dto.WinningPeriod = dto.PurchasedPeriod;
        }

        if (!string.IsNullOrWhiteSpace(dto.WinningPeriod) && string.IsNullOrWhiteSpace(dto.PurchasedPeriod))
        {
            dto.PurchasedPeriod = dto.WinningPeriod;
        }

        var requestDto = new StatisticsWinItemRequestDto
        {
            PurchasedPeriod = dto.PurchasedPeriod,
            WinningPeriod = dto.WinningPeriod,
            LotteryType = dto.LotteryType
        };

        return await this.GetStatisticsWinItem(requestDto);
    }

    /// <summary>
    /// 获取分组列表（分页）
    /// </summary>
    public async Task<PagedResultDto<LotteryGroupDto>> GetListGrouped(PagedAndSortedResultRequestDto input)
    {
        var query = await Repository.GetListAsync();

        // 根据 Sorting 字段排序（仅支持 Id、IndexNo 等简单字段，格式为 "PropertyName" 或 "PropertyName DESC"）
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            var sorting = input.Sorting.Trim();
            var isDescending = sorting.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase);
            var propertyName = isDescending ? sorting[..^5].Trim() : sorting;

            query = propertyName.ToUpperInvariant() switch
            {
                "ID" => isDescending ? query.OrderByDescending(x => x.Id).ToList() : query.OrderBy(x => x.Id).ToList(),
                "INDEXNO" => isDescending ? query.OrderByDescending(x => x.IndexNo).ToList() : query.OrderBy(x => x.IndexNo).ToList(),
                "CREATIONTIME" => isDescending ? query.OrderByDescending(x => x.CreationTime).ToList() : query.OrderBy(x => x.CreationTime).ToList(),
                _ => query.OrderBy(x => x.Id).ToList()
            };
        }
        else
        {
            query = query.OrderBy(x => x.Id).ToList();
        }

        var groupedLotteries = query.GroupBy(x => new { x.IndexNo, x.GroupId, x.LotteryType });

        var totalCount = groupedLotteries.Count();

        var lotteryGroupDtos = new List<LotteryGroupDto>();

        foreach (var group in groupedLotteries)
        {
            var groupList = group.OrderBy(x => x.Id).ToList();
            var firstItem = groupList.First();

            var lotteryGroupDto = new LotteryGroupDto
            {
                Id = firstItem.Id,
                IndexNo = firstItem.IndexNo,
                LotteryType = firstItem.LotteryType,
                GroupId = firstItem.GroupId,
                CreationTime = firstItem.CreationTime,
                LastModificationTime = firstItem.LastModificationTime
            };

            var redNumbers = groupList.Where(x => x.ColorType == "0").Select(x => x.Number).ToList();
            var blueNumbers = groupList.Where(x => x.ColorType == "1").Select(x => x.Number).ToList();

            lotteryGroupDto.RedNumbers = string.Join(",", redNumbers.OrderBy(x => x));
            lotteryGroupDto.BlueNumber = blueNumbers.FirstOrDefault() ?? "";

            lotteryGroupDtos.Add(lotteryGroupDto);
        }

        if (input.MaxResultCount > 0)
        {
            lotteryGroupDtos = lotteryGroupDtos.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        }

        return new PagedResultDto<LotteryGroupDto>(totalCount, lotteryGroupDtos);
    }

    /// <summary>
    /// 获取指定类型的最新期号
    /// </summary>
    public async Task<int> GetLatestIndexNoByType(string lotteryType)
    {
        if (string.IsNullOrWhiteSpace(lotteryType))
            throw new BusinessException(nameof(lotteryType) + " 不能为空");

        var queryable = Repository.GetQueryable();
        var latestLottery = queryable
            .Where(x => x.LotteryType == lotteryType)
            .OrderByDescending(x => x.IndexNo)
            .ToList()
            .FirstOrDefault();

        return latestLottery?.IndexNo ?? 0;
    }

    /// <summary>
    /// 根据组号删除彩票组
    /// </summary>
    public async Task DeleteLotteryGroup(long groupId)
    {
        var queryable = Repository.GetQueryable();
        var groupLotteries = queryable.Where(x => x.GroupId == groupId).ToList();

        if (groupLotteries.Any())
        {
            try
            {
                Repository.BeginTran();
                await Repository.DeleteAsync(groupLotteries);
                Repository.CommitTran();
            }
            catch (Exception)
            {
                Repository.RollbackTran();
                throw;
            }
        }
    }

    /// <summary>
    /// 根据期号和组号删除彩票组
    /// </summary>
    public async Task DeleteLotteryGroupByIndexNoAndGroupId(int indexNo, long groupId)
    {
        var queryable = Repository.GetQueryable();
        // 按期号和组号删除，确保只删除指定期内的指定组
        var groupLotteries = queryable.Where(x => x.IndexNo == indexNo && x.GroupId == groupId).ToList();

        if (groupLotteries.Any())
        {
            try
            {
                Repository.BeginTran();
                await Repository.DeleteAsync(groupLotteries);
                Repository.CommitTran();
            }
            catch (Exception)
            {
                Repository.RollbackTran();
                throw;
            }
        }
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    protected override LotteryDto MapToGetOutputDto(LotteryInfo entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    protected override LotteryInfo MapToEntity(CreateUpdateLotteryDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    protected override void MapToEntity(CreateUpdateLotteryDto input, LotteryInfo entity)
    {
        _mapper.MapToEntity(input, entity);
    }
}
