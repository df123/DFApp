using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Mapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DFApp.Web.Background;

/// <summary>
/// 彩票开奖结果定时任务
/// 每天晚上 23 点执行，从代理服务获取双色球(SSQ)和快乐8(KL8)的最新开奖数据并更新数据库
/// </summary>
public class LotteryResultJob : IJob
{
    private readonly ISqlSugarRepository<LotteryResult, long> _lotteryResultRepository;
    private readonly ISqlSugarReadOnlyRepository<LotteryResult, long> _resultReadOnly;
    private readonly ISqlSugarRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;
    private readonly ISqlSugarReadOnlyRepository<LotteryPrizegrades, long> _prizegradesReadOnly;
    private readonly LotteryMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LotteryResultJob> _logger;

    public LotteryResultJob(
        ISqlSugarRepository<LotteryResult, long> lotteryResultRepository,
        ISqlSugarReadOnlyRepository<LotteryResult, long> resultReadOnly,
        ISqlSugarRepository<LotteryPrizegrades, long> lotteryPrizegradesRepository,
        ISqlSugarReadOnlyRepository<LotteryPrizegrades, long> prizegradesReadOnly,
        LotteryMapper mapper,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<LotteryResultJob> logger)
    {
        _lotteryResultRepository = lotteryResultRepository;
        _resultReadOnly = resultReadOnly;
        _lotteryPrizegradesRepository = lotteryPrizegradesRepository;
        _prizegradesReadOnly = prizegradesReadOnly;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // 处理双色球（SSQ），失败不影响其他彩票类型
        try
        {
            await StartWork(LotteryConst.SSQ, LotteryConst.SSQ_ENG, LotteryConst.SSQ_START_CODE);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理双色球(SSQ)时发生异常，将继续处理其他彩票类型");
        }

        // 处理快乐8（KL8），失败不影响其他彩票类型
        try
        {
            await StartWork(LotteryConst.KL8, LotteryConst.KL8_ENG, LotteryConst.KL8_STRAT_CODE);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理快乐8(KL8)时发生异常，将继续处理其他彩票类型");
        }
    }

    private async Task StartWork(string lotteryType, string lotteryTypeEng, string code)
    {
        _logger.LogInformation("开始任务......{LotteryType} (英文类型: {LotteryTypeEng}, 起始代码: {Code})", lotteryType, lotteryTypeEng, code);

        try
        {
            // 检查是否已有数据
            _logger.LogInformation("检查数据库中是否存在彩票类型为 {LotteryType} 且代码为 {Code} 的数据", lotteryType, code);
            List<LotteryResult> result = await _resultReadOnly.GetListAsync(item => item.Code == code && item.Name == lotteryType);
            _logger.LogInformation("查询到 {Count} 条历史数据", result?.Count ?? 0);

            if (result == null || result.Count <= 0)
            {
                _logger.LogInformation("未找到历史数据，开始获取所有历史数据");
                string dayStart, dayEnd;
                dayStart = "2013-01-01"; // KL8的开始时间是2020年，小于2013年所有可以直接用2013
                dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
                _logger.LogInformation("获取历史数据范围: {DayStart} 至 {DayEnd}", dayStart, dayEnd);
                await GetAllLotteryResults(dayStart, dayEnd, 1, lotteryTypeEng);
            }
            else
            {
                _logger.LogInformation("已存在历史数据，跳过历史数据获取");
            }

            // 检查是否需要获取最新数据
            bool shouldFetchLatest = DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                || DateTime.Now.DayOfWeek == DayOfWeek.Thursday
                || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday
                || lotteryType == LotteryConst.KL8;

            _logger.LogInformation("是否需要获取最新数据: {ShouldFetchLatest} (当前星期: {DayOfWeek})", shouldFetchLatest, DateTime.Now.DayOfWeek);

            if (shouldFetchLatest)
            {
                string day = DateTime.Now.ToString("yyyy-MM-dd");
                _logger.LogInformation("检查今天 ({Day}) 是否已有数据", day);
                List<LotteryResult> result1 = await _resultReadOnly.GetListAsync(item => item.Date != null && item.Date.StartsWith(day));
                _logger.LogInformation("今天的数据条数: {Count}", result1?.Count ?? 0);

                if (result1 == null || result1.Count <= 0)
                {
                    _logger.LogInformation("今天没有数据，开始获取最新数据");
                    _lotteryResultRepository.BeginTran();
                    try
                    {
                        _logger.LogInformation("开始更新奖级信息");
                        await UpdatePrizegrades(lotteryType, lotteryTypeEng);

                        _logger.LogInformation("获取最新一期开奖结果作为起始点");
                        LotteryResult lotteryResult = _resultReadOnly.GetQueryable().OrderByDescending(x => x.Code).First();
                        string dayStart = (lotteryResult.Date!.Split('('))[0];
                        _logger.LogInformation("从 {DayStart} 开始获取最新数据", dayStart);

                        await GetCurrentLotteryResult(dayStart, 0, lotteryTypeEng);
                        _lotteryResultRepository.CommitTran();
                        _logger.LogInformation("最新数据获取完成并提交事务");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取最新数据时发生异常");
                        _lotteryResultRepository.RollbackTran();
                        throw;
                    }
                }
                else
                {
                    _logger.LogInformation("今天已有数据，跳过最新数据获取");
                }
            }

            _logger.LogInformation("任务成功结束......{LotteryType}", lotteryType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务执行失败......{LotteryType}", lotteryType);
            throw;
        }
    }

    private async Task GetCurrentLotteryResult(string dayStart, int pageNo, string lotteryType)
    {
        string dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
        _logger.LogInformation("获取当前彩票结果 - 起始日期: {DayStart}, 结束日期: {DayEnd}, 彩票类型: {LotteryType}, 页码: {PageNo}", dayStart, dayEnd, lotteryType, pageNo);

        LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, pageNo, lotteryType);

        if (dto.Result != null && dto.Result.Count > 0)
        {
            _logger.LogInformation("获取到 {Count} 条数据，开始映射并保存到数据库", dto.Result.Count);
            List<LotteryResult> result = dto.Result.Select(item => _mapper.MapToEntityFromExternalResultItem(item)).ToList();

            try
            {
                await _lotteryResultRepository.InsertAsync(result);
                _logger.LogInformation("成功保存 {Count} 条彩票结果到数据库", result.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存彩票结果到数据库时发生异常");
                throw;
            }

            // 检查是否需要获取下一页数据
            if (dto.PageNo < dto.PageNum)
            {
                _logger.LogInformation("当前页 {PageNo} 小于总页数 {PageNum}，继续获取下一页数据", dto.PageNo, dto.PageNum);
                await GetCurrentLotteryResult(dayStart, pageNo + 1, lotteryType);
            }
            else
            {
                _logger.LogInformation("已获取所有页数据，当前页 {PageNo}，总页数 {PageNum}", dto.PageNo, dto.PageNum);
            }
        }
        else
        {
            _logger.LogInformation("未获取到任何数据，结束当前彩票类型的数据获取");
        }
    }

    private async Task GetAllLotteryResults(string dayStart, string dayEnd, int pageNo, string lotteryType)
    {
        _logger.LogInformation("获取所有历史彩票结果 - 起始日期: {DayStart}, 结束日期: {DayEnd}, 彩票类型: {LotteryType}, 页码: {PageNo}", dayStart, dayEnd, lotteryType, pageNo);

        LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, pageNo, lotteryType);

        if (dto.Result != null && dto.Result.Count > 0)
        {
            _logger.LogInformation("获取到 {Count} 条历史数据，开始映射并保存到数据库", dto.Result.Count);
            List<LotteryResult> result = dto.Result.Select(item => _mapper.MapToEntityFromExternalResultItem(item)).ToList();

            try
            {
                await _lotteryResultRepository.InsertAsync(result);
                _logger.LogInformation("成功保存 {Count} 条历史彩票结果到数据库", result.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存历史彩票结果到数据库时发生异常");
                throw;
            }

            // 检查是否需要获取下一页数据
            if (dto.PageNo < dto.PageNum)
            {
                _logger.LogInformation("当前页 {PageNo} 小于总页数 {PageNum}，继续获取下一页历史数据", dto.PageNo, dto.PageNum);
                await GetAllLotteryResults(dayStart, dayEnd, pageNo + 1, lotteryType);
            }
            else
            {
                _logger.LogInformation("已获取所有历史页数据，当前页 {PageNo}，总页数 {PageNum}", dto.PageNo, dto.PageNum);
            }
        }
        else
        {
            _logger.LogInformation("未获取到任何历史数据，结束历史数据获取");
        }
    }

    private async Task UpdatePrizegrades(string lotteryType, string lotteryTypeEng)
    {
        _logger.LogInformation("开始更新奖级信息 - 彩票类型: {LotteryType} (英文: {LotteryTypeEng})", lotteryType, lotteryTypeEng);

        try
        {
            // 查询该彩票类型的所有结果
            _logger.LogInformation("查询彩票类型为 {LotteryType} 的所有结果", lotteryType);
            var allResults = await _resultReadOnly.GetListAsync(x => x.Name == lotteryType);
            _logger.LogInformation("查询到 {Count} 条彩票结果记录", allResults.Count);

            if (allResults.Count == 0)
            {
                _logger.LogInformation("没有找到任何彩票结果记录，跳过奖级更新");
                return;
            }

            // 查询已有奖级的结果ID集合
            var resultIds = allResults.Select(r => r.Id).ToList();
            var existingPrizegrades = await _prizegradesReadOnly.GetListAsync(p => resultIds.Contains(p.LotteryResultId));
            var prizeResultIdSet = existingPrizegrades.Select(p => p.LotteryResultId).ToHashSet();

            int noPrizeCount = allResults.Count(r => !prizeResultIdSet.Contains(r.Id));
            _logger.LogInformation("其中 {NoPrizeCount} 条记录没有奖级信息", noPrizeCount);

            int processedCount = 0;
            foreach (var item in allResults)
            {
                if (!prizeResultIdSet.Contains(item.Id))
                {
                    _logger.LogInformation("处理彩票结果 ID: {Id}, 代码: {Code}, 日期: {Date}", item.Id, item.Code, item.Date);

                    try
                    {
                        string dayStart = (item.Date!.Split('('))[0];
                        _logger.LogInformation("获取 {DayStart} 的奖级信息", dayStart);

                        LotteryInputDto dto = await GetLotteryResult(dayStart, dayStart, 1, lotteryTypeEng);

                        if (dto.Result != null && dto.Result.Count > 0)
                        {
                            _logger.LogInformation("获取到 {Count} 条奖级数据", dto.Result.Count);

                            foreach (var resultItem in dto.Result)
                            {
                                if (resultItem.Prizegrades != null && resultItem.Prizegrades.Count > 0)
                                {
                                    _logger.LogInformation("为彩票结果 ID: {Id} 添加 {PrizeCount} 条奖级信息", item.Id, resultItem.Prizegrades.Count);

                                    var prizeEntities = resultItem.Prizegrades.Select(pg =>
                                    {
                                        var entity = _mapper.MapToEntityFromExternalPrizegradesItem(pg);
                                        entity.LotteryResultId = item.Id;
                                        return entity;
                                    }).ToList();

                                    await _lotteryPrizegradesRepository.InsertAsync(prizeEntities);
                                    processedCount++;
                                }
                                else
                                {
                                    _logger.LogWarning("彩票结果代码: {Code} 没有奖级信息", resultItem.Code);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("未获取到 {DayStart} 的奖级数据", dayStart);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理彩票结果 ID: {Id} 的奖级信息时发生异常", item.Id);
                    }
                }
            }

            _logger.LogInformation("奖级信息更新完成，共处理 {ProcessedCount} 条记录", processedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新奖级信息时发生异常");
            throw;
        }
    }

    private async Task<LotteryInputDto> GetLotteryResult(string dayStart, string dayEnd, int pageNo, string lotteryType)
    {
        // 使用代理服务器获取数据
        string proxyServerUrl = LotteryConst.GetLotteryProxyUrl(_configuration);
        string requestUrl = $"{proxyServerUrl}/api/proxy/lottery/findDrawNotice?name={lotteryType}&dayStart={dayStart}&dayEnd={dayEnd}&pageNo={pageNo}&pageSize=30&week=&systemType=PC";

        _logger.LogInformation("开始通过代理获取彩票数据 - 彩票类型: {LotteryType}, 开始日期: {DayStart}, 结束日期: {DayEnd}, 页码: {PageNo}", lotteryType, dayStart, dayEnd, pageNo);
        _logger.LogInformation("代理请求URL: {RequestUrl}", requestUrl);

        try
        {
            using var client = _httpClientFactory.CreateClient();
            // 设置超时时间
            client.Timeout = TimeSpan.FromSeconds(60);

            _logger.LogInformation("发送代理HTTP请求...");

            HttpResponseMessage message = await client.GetAsync(requestUrl);

            _logger.LogInformation("代理HTTP响应状态码: {StatusCode} ({Status})", (int)message.StatusCode, message.StatusCode);

            message.EnsureSuccessStatusCode();

            string responseContent = await message.Content.ReadAsStringAsync();
            _logger.LogInformation("代理响应内容长度: {Length} 字符", responseContent.Length);

            // 记录响应内容（仅前500字符，避免日志过长）
            if (responseContent.Length > 500)
            {
                _logger.LogInformation("代理响应内容前500字符: {Content}...", responseContent.Substring(0, 500));
            }
            else
            {
                _logger.LogInformation("代理响应内容: {Content}", responseContent);
            }

            LotteryInputDto? dto = JsonSerializer.Deserialize<LotteryInputDto>(responseContent);

            if (dto == null)
            {
                _logger.LogWarning("反序列化代理响应失败，响应为null，创建空对象");
                dto = new LotteryInputDto();
            }
            else
            {
                _logger.LogInformation("反序列化代理响应成功 - 总数据量: {Total}, 当前页: {PageNo}/{PageNum}, 每页大小: {PageSize}", dto.Total, dto.PageNo, dto.PageNum, dto.PageSize);

                if (dto.Result != null)
                {
                    _logger.LogInformation("当前页数据条数: {Count}", dto.Result.Count);

                    // 记录第一条数据的详细信息
                    if (dto.Result.Count > 0)
                    {
                        var firstResult = dto.Result[0];
                        _logger.LogInformation("第一条数据 - 彩票类型: {Name}, 期号: {Code}, 开奖日期: {Date}, 红球: {Red}, 蓝球: {Blue}", firstResult.Name, firstResult.Code, firstResult.Date, firstResult.Red, firstResult.Blue);
                    }
                }
                else
                {
                    _logger.LogWarning("代理响应中的Result字段为null");
                }
            }

            return dto;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "代理HTTP请求异常: {Message}", ex.Message);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "代理请求超时: {Message}", ex.Message);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "代理JSON解析异常: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "代理未知异常: {Message}", ex.Message);
            throw;
        }
    }
}
