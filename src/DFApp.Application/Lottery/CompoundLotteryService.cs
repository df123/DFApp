using DFApp.Lottery.Consts;
using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace DFApp.Lottery
{
    /// <summary>
    /// 复式投注服务
    /// </summary>
    [Authorize(DFAppPermissions.Lottery.Default)]
    public class CompoundLotteryService : ApplicationService, ICompoundLotteryService
    {
        private readonly IRepository<LotteryInfo, long> _lotteryInforepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<CompoundLotteryService> _logger;

        public CompoundLotteryService(
            IRepository<LotteryInfo, long> lotteryInforepository,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger<CompoundLotteryService> logger)
        {
            _lotteryInforepository = lotteryInforepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        /// <summary>
        /// 计算复式投注组合
        /// </summary>
        public async Task<CompoundLotteryResultDto> CalculateCompoundCombination(CompoundLotteryInputDto dto)
        {
            // 验证输入
            string validationError = await ValidateCompoundInput(dto);
            if (!string.IsNullOrEmpty(validationError))
            {
                throw new ArgumentException(validationError);
            }

            var result = new CompoundLotteryResultDto();

            try
            {
                // 转换彩票类型为中文名称
                var lotteryTypeChinese = ConvertLotteryTypeToChinese(dto.LotteryType);

                // 根据彩票类型生成组合
                List<string> combinations;
                if (lotteryTypeChinese == LotteryConst.SSQ)
                {
                    combinations = GenerateSSQCombinations(dto.RedNumbers, dto.BlueNumbers);
                    result.TotalCombinations = combinations.Count;
                    result.TotalAmount = combinations.Count * 2m; // 每注2元
                }
                else if (lotteryTypeChinese == LotteryConst.KL8)
                {
                    if (!dto.PlayType.HasValue)
                    {
                        throw new ArgumentException("快乐8复式投注必须指定玩法类型");
                    }
                    combinations = GenerateKL8Combinations(dto.KL8Numbers, dto.PlayType.Value);
                    result.TotalCombinations = combinations.Count;
                    result.TotalAmount = combinations.Count * 2m; // 每注2元
                }
                else
                {
                    throw new ArgumentException($"不支持的彩票类型: {dto.LotteryType}");
                }

                result.CombinationDetails = combinations;

                // 保存到数据库（使用中文彩票类型）
                var databaseDto = new CompoundLotteryInputDto
                {
                    Period = dto.Period,
                    LotteryType = lotteryTypeChinese,
                    RedNumbers = dto.RedNumbers,
                    BlueNumbers = dto.BlueNumbers,
                    KL8Numbers = dto.KL8Numbers,
                    PlayType = dto.PlayType
                };
                result.CreatedLotteries = await SaveCombinationsToDatabase(databaseDto, combinations);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算复式投注组合失败");
                throw new UserFriendlyException("计算复式投注组合失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 生成双色球复式组合
        /// </summary>
        public List<string> GenerateSSQCombinations(List<string> reds, List<string> blues)
        {
            if (reds == null || blues == null)
                throw new ArgumentException("红球和蓝球列表不能为空");

            if (reds.Count < 6)
                throw new ArgumentException("双色球复式投注红球数量不能少于6个");

            if (blues.Count < 1)
                throw new ArgumentException("双色球复式投注蓝球数量不能少于1个");

            var combinations = new List<string>();

            // 生成所有6选红球组合
            var redCombinations = GetCombinations(reds, 6);

            foreach (var redCombo in redCombinations)
            {
                foreach (var blue in blues)
                {
                    var redString = string.Join(",", redCombo.OrderBy(x => int.Parse(x)));
                    combinations.Add($"{redString}|{blue}");
                }
            }

            return combinations;
        }

        /// <summary>
        /// 转换彩票类型代码为中文名称
        /// </summary>
        private string ConvertLotteryTypeToChinese(string lotteryType)
        {
            return lotteryType.ToLower() switch
            {
                "ssq" => LotteryConst.SSQ,
                "kl8" => LotteryConst.KL8,
                _ => lotteryType
            };
        }

        /// <summary>
        /// 生成快乐8复式组合
        /// </summary>
        public List<string> GenerateKL8Combinations(List<string> numbers, LotteryKL8PlayType playType)
        {
            if (numbers == null)
                throw new ArgumentException("号码列表不能为空");

            if (numbers.Count < (int)playType)
                throw new ArgumentException($"快乐8{playType}玩法需要至少{(int)playType}个号码");

            // 去除重复号码
            numbers = numbers.Distinct().ToList();

            // 生成指定个数的组合
            var combinations = GetCombinations(numbers, (int)playType);

            return combinations.Select(combo => string.Join(",", combo.OrderBy(x => int.Parse(x)))).ToList();
        }

        /// <summary>
        /// 验证复式投注输入
        /// </summary>
        public async Task<string> ValidateCompoundInput(CompoundLotteryInputDto dto)
        {
            if (dto == null)
                return "输入数据不能为空";

            if (dto.Period <= 0)
                return "期号必须大于0";

            if (string.IsNullOrWhiteSpace(dto.LotteryType))
                return "彩票类型不能为空";

            // 验证双色球
            if (dto.LotteryType == LotteryConst.SSQ_ENG)
            {
                if (dto.RedNumbers == null || dto.RedNumbers.Count == 0)
                    return "双色球红球号码不能为空";

                if (dto.BlueNumbers == null || dto.BlueNumbers.Count == 0)
                    return "双色球蓝球号码不能为空";

                // 验证红球范围 (1-33)
                foreach (var red in dto.RedNumbers)
                {
                    if (!int.TryParse(red, out int redNum) || redNum < 1 || redNum > 33)
                        return $"红球号码 {red} 超出范围 (1-33)";
                }

                // 验证蓝球范围 (1-16)
                foreach (var blue in dto.BlueNumbers)
                {
                    if (!int.TryParse(blue, out int blueNum) || blueNum < 1 || blueNum > 16)
                        return $"蓝球号码 {blue} 超出范围 (1-16)";
                }

                // 检查红球重复
                if (dto.RedNumbers.Count != dto.RedNumbers.Distinct().Count())
                    return "红球号码不能重复";

                // 检查蓝球重复
                if (dto.BlueNumbers.Count != dto.BlueNumbers.Distinct().Count())
                    return "蓝球号码不能重复";

                // 检查红蓝球重复
                if (dto.RedNumbers.Intersect(dto.BlueNumbers).Any())
                    return "红球和蓝球不能重复";
            }
            // 验证快乐8
            else if (dto.LotteryType == LotteryConst.KL8_ENG)
            {
                if (dto.KL8Numbers == null || dto.KL8Numbers.Count == 0)
                    return "快乐8号码不能为空";

                if (!dto.PlayType.HasValue)
                    return "快乐8玩法类型不能为空";

                if (dto.KL8Numbers.Count < (int)dto.PlayType.Value)
                    return $"快乐8{dto.PlayType.Value}玩法需要至少{(int)dto.PlayType.Value}个号码";

                // 验证号码范围 (1-80)
                foreach (var number in dto.KL8Numbers)
                {
                    if (!int.TryParse(number, out int num) || num < 1 || num > 80)
                        return $"快乐8号码 {number} 超出范围 (1-80)";
                }

                // 检查重复号码
                if (dto.KL8Numbers.Count != dto.KL8Numbers.Distinct().Count())
                    return "快乐8号码不能重复";
            }
            else
            {
                return $"不支持的彩票类型: {dto.LotteryType}";
            }

            return string.Empty;
        }

        /// <summary>
        /// 生成组合 (数学组合算法)
        /// </summary>
        private List<List<string>> GetCombinations(List<string> source, int combinationSize)
        {
            var result = new List<List<string>>();

            void Backtrack(int start, List<string> current)
            {
                if (current.Count == combinationSize)
                {
                    result.Add(new List<string>(current));
                    return;
                }

                for (int i = start; i < source.Count; i++)
                {
                    current.Add(source[i]);
                    Backtrack(i + 1, current);
                    current.RemoveAt(current.Count - 1);
                }
            }

            Backtrack(0, new List<string>());
            return result;
        }

        /// <summary>
        /// 保存组合到数据库
        /// </summary>
        private async Task<List<LotteryDto>> SaveCombinationsToDatabase(CompoundLotteryInputDto dto, List<string> combinations)
        {
            var lotteryInfos = new List<LotteryInfo>();

            // 获取下一个组ID
            var lastInfo = await _lotteryInforepository.GetListAsync(x => x.IndexNo == dto.Period && x.LotteryType == dto.LotteryType);
            int nextGroupId = lastInfo.Any() ? lastInfo.Max(x => x.GroupId) + 1 : 0;

            int currentGroupId = nextGroupId;
            foreach (var combination in combinations)
            {
                if (dto.LotteryType == LotteryConst.SSQ)
                {
                    var parts = combination.Split('|');
                    var redNumbers = parts[0].Split(',');
                    var blueNumber = parts[1];

                    // 添加红球
                    foreach (var red in redNumbers)
                    {
                        lotteryInfos.Add(new LotteryInfo
                        {
                            IndexNo = dto.Period,
                            Number = red,
                            ColorType = "0",
                            LotteryType = dto.LotteryType,
                            GroupId = currentGroupId
                        });
                    }

                    // 添加蓝球
                    lotteryInfos.Add(new LotteryInfo
                    {
                        IndexNo = dto.Period,
                        Number = blueNumber,
                        ColorType = "1",
                        LotteryType = dto.LotteryType,
                        GroupId = currentGroupId
                    });

                    currentGroupId++;
                }
                else if (dto.LotteryType == LotteryConst.KL8)
                {
                    var numbers = combination.Split(',');

                    foreach (var number in numbers)
                    {
                        lotteryInfos.Add(new LotteryInfo
                        {
                            IndexNo = dto.Period,
                            Number = number,
                            ColorType = "0", // 快乐8统一使用"0"
                            LotteryType = dto.LotteryType,
                            GroupId = currentGroupId
                        });
                    }

                    currentGroupId++;
                }
            }

            // 批量保存
            if (lotteryInfos.Any())
            {
                using (var uom = _unitOfWorkManager.Begin(true, true))
                {
                    try
                    {
                        await _lotteryInforepository.InsertManyAsync(lotteryInfos);
                        await uom.CompleteAsync();
                    }
                    catch (Exception)
                    {
                        await uom.RollbackAsync();
                        throw;
                    }
                }
            }

            // 返回保存的彩票DTO
            var savedLotteries = await _lotteryInforepository.GetListAsync(x => 
                x.IndexNo == dto.Period && 
                x.LotteryType == dto.LotteryType && 
                x.GroupId >= nextGroupId);

            return ObjectMapper.Map<List<LotteryInfo>, List<LotteryDto>>(savedLotteries);
        }
    }
}
