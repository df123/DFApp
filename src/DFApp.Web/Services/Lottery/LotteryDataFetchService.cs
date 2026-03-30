using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Lottery;

/// <summary>
/// 彩票数据获取服务，负责从代理服务器获取彩票开奖数据
/// </summary>
public class LotteryDataFetchService : AppServiceBase
{
    private readonly ISqlSugarRepository<LotteryResult, long> _lotteryResultRepository;
    private readonly ISqlSugarRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LotteryDataFetchService> _logger;

    public LotteryDataFetchService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<LotteryResult, long> lotteryResultRepository,
        ISqlSugarRepository<LotteryPrizegrades, long> lotteryPrizegradesRepository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<LotteryDataFetchService> logger)
        : base(currentUser, permissionChecker)
    {
        _lotteryResultRepository = lotteryResultRepository;
        _lotteryPrizegradesRepository = lotteryPrizegradesRepository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 获取彩票数据
    /// </summary>
    public async Task<LotteryDataFetchResponseDto> FetchLotteryData(LotteryDataFetchRequestDto input)
    {
        var response = new LotteryDataFetchResponseDto();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("开始获取彩票数据 - 彩票类型: {LotteryType}, 开始日期: {DayStart}, 结束日期: {DayEnd}, 页码: {PageNo}",
                input.LotteryType, input.DayStart, input.DayEnd, input.PageNo);

            // 使用代理服务器获取数据
            string proxyServerUrl = LotteryConst.GetLotteryProxyUrl(_configuration);
            string queryString = $"name={input.LotteryType}&issueCount=&issueStart=&issueEnd=&dayStart={input.DayStart}&dayEnd={input.DayEnd}&pageNo={input.PageNo}&pageSize=30&week=&systemType=PC";
            string requestUrl = $"{proxyServerUrl}/api/proxy/lottery/findDrawNotice?{queryString}";
            response.RequestUrl = requestUrl;

            _logger.LogInformation("通过代理服务器请求URL: {RequestUrl}", requestUrl);

            // 创建HTTP客户端
            using var client = _httpClientFactory.CreateClient();

            _logger.LogInformation("发送代理HTTP请求...");

            // 发送请求到代理服务器
            var httpResponse = await client.GetAsync(requestUrl);
            response.StatusCode = (int)httpResponse.StatusCode;

            _logger.LogInformation("HTTP响应状态码: {StatusCode}", response.StatusCode);

            // 确保请求成功
            httpResponse.EnsureSuccessStatusCode();

            // 读取响应内容
            string responseContent = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("响应内容长度: {Length} 字符", responseContent.Length);

            // 记录响应内容（仅前500字符，避免日志过长）
            if (responseContent.Length > 500)
            {
                _logger.LogInformation("响应内容前500字符: {Content}...", responseContent.Substring(0, 500));
            }
            else
            {
                _logger.LogInformation("响应内容: {Content}", responseContent);
            }

            // 反序列化响应
            LotteryInputDto? dto = JsonSerializer.Deserialize<LotteryInputDto>(responseContent);

            if (dto == null)
            {
                _logger.LogWarning("反序列化响应失败，响应为null");
                dto = new LotteryInputDto();
            }

            response.Data = dto;
            response.Success = true;
            response.Message = $"成功获取到 {dto.Total} 条数据";

            _logger.LogInformation("获取到数据总数: {Total}, 当前页: {PageNo}/{PageNum}, 每页大小: {PageSize}",
                dto.Total, dto.PageNo, dto.PageNum, dto.PageSize);

            if (dto.Result != null && dto.Result.Count > 0)
            {
                _logger.LogInformation("当前页数据条数: {Count}", dto.Result.Count);

                // 记录第一条数据的详细信息
                var firstResult = dto.Result[0];
                _logger.LogInformation("第一条数据 - 彩票类型: {Name}, 期号: {Code}, 开奖日期: {Date}, 红球: {Red}, 蓝球: {Blue}",
                    firstResult.Name, firstResult.Code, firstResult.Date, firstResult.Red, firstResult.Blue);

                // 如果需要保存到数据库
                if (input.SaveToDatabase)
                {
                    _logger.LogInformation("开始保存数据到数据库...");

                    try
                    {
                        _lotteryResultRepository.BeginTran();

                        var results = new List<LotteryResult>();

                        foreach (var item in dto.Result)
                        {
                            // TODO: 使用 Mapperly 映射
                            var lotteryResult = MapResultItemToLotteryResult(item);

                            // 手动处理Prizegrades映射
                            if (item.Prizegrades != null && item.Prizegrades.Count > 0)
                            {
                                lotteryResult.Prizegrades = new List<LotteryPrizegrades>();
                                foreach (var prizegrade in item.Prizegrades)
                                {
                                    var lotteryPrizegrade = new LotteryPrizegrades
                                    {
                                        Type = prizegrade.Type,
                                        TypeNum = prizegrade.TypeNum,
                                        TypeMoney = prizegrade.TypeMoney,
                                        LotteryResultId = 0 // 插入后更新
                                    };
                                    lotteryResult.Prizegrades.Add(lotteryPrizegrade);
                                }
                            }

                            results.Add(lotteryResult);
                        }

                        // 先保存 LotteryResult
                        await _lotteryResultRepository.InsertAsync(results);
                        response.SavedCount = results.Count;

                        // 保存关联的 Prizegrades
                        var allPrizegrades = new List<LotteryPrizegrades>();
                        foreach (var result in results)
                        {
                            if (result.Prizegrades != null)
                            {
                                foreach (var pg in result.Prizegrades)
                                {
                                    pg.LotteryResultId = result.Id;
                                    allPrizegrades.Add(pg);
                                }
                            }
                        }

                        if (allPrizegrades.Count > 0)
                        {
                            await _lotteryPrizegradesRepository.InsertAsync(allPrizegrades);
                        }

                        _lotteryResultRepository.CommitTran();
                        _logger.LogInformation("成功保存 {Count} 条数据到数据库", response.SavedCount);
                    }
                    catch (Exception ex)
                    {
                        _lotteryResultRepository.RollbackTran();
                        _logger.LogError(ex, "保存数据到数据库时发生错误");
                        response.Message += $", 但保存到数据库失败: {ex.Message}";
                    }
                }
            }
            else
            {
                _logger.LogWarning("未获取到任何数据");
                response.Message = "未获取到任何数据";
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP请求异常: {Message}", ex.Message);
            response.Success = false;
            response.Message = $"HTTP请求异常: {ex.Message}";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "请求超时: {Message}", ex.Message);
            response.Success = false;
            response.Message = $"请求超时: {ex.Message}";
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON解析异常: {Message}", ex.Message);
            response.Success = false;
            response.Message = $"JSON解析异常: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未知异常: {Message}", ex.Message);
            response.Success = false;
            response.Message = $"未知异常: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation("请求完成，耗时: {ResponseTime} 毫秒", response.ResponseTime);
        }

        return response;
    }

    /// <summary>
    /// 获取双色球最新数据
    /// </summary>
    public async Task<LotteryDataFetchResponseDto> FetchSSQLatestData()
    {
        _logger.LogInformation("获取双色球最新数据");

        var request = new LotteryDataFetchRequestDto
        {
            LotteryType = LotteryConst.SSQ_ENG,
            DayStart = DateTime.Now.ToString("yyyy-MM-dd"),
            DayEnd = DateTime.Now.ToString("yyyy-MM-dd"),
            PageNo = 1,
            SaveToDatabase = true
        };

        return await FetchLotteryData(request);
    }

    /// <summary>
    /// 获取快乐8最新数据
    /// </summary>
    public async Task<LotteryDataFetchResponseDto> FetchKL8LatestData()
    {
        _logger.LogInformation("获取快乐8最新数据");

        var request = new LotteryDataFetchRequestDto
        {
            LotteryType = LotteryConst.KL8_ENG,
            DayStart = DateTime.Now.ToString("yyyy-MM-dd"),
            DayEnd = DateTime.Now.ToString("yyyy-MM-dd"),
            PageNo = 1,
            SaveToDatabase = true
        };

        return await FetchLotteryData(request);
    }

    /// <summary>
    /// 测试彩票API连接
    /// </summary>
    public async Task<LotteryDataFetchResponseDto> TestLotteryApiConnection(string lotteryType = LotteryConst.SSQ_ENG)
    {
        _logger.LogInformation("测试彩票API连接 - 彩票类型: {LotteryType}", lotteryType);

        var request = new LotteryDataFetchRequestDto
        {
            LotteryType = lotteryType,
            DayStart = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd"),
            DayEnd = DateTime.Now.ToString("yyyy-MM-dd"),
            PageNo = 1,
            SaveToDatabase = false
        };

        return await FetchLotteryData(request);
    }

    /// <summary>
    /// 将 ResultItemDto 手动映射为 LotteryResult 实体
    /// </summary>
    private LotteryResult MapResultItemToLotteryResult(ResultItemDto item)
    {
        // TODO: 使用 Mapperly 映射
        return new LotteryResult
        {
            Name = item.Name,
            Code = item.Code,
            DetailsLink = item.DetailsLink,
            VideoLink = item.VideoLink,
            Date = item.Date,
            Week = item.Week,
            Red = item.Red,
            Blue = item.Blue,
            Blue2 = item.Blue2,
            Sales = item.Sales,
            PoolMoney = item.PoolMoney,
            Content = item.Content,
            AddMoney = item.AddMoney,
            AddMoney2 = item.AddMoney2,
            Msg = item.Msg,
            Z2Add = item.Z2Add,
            M2Add = item.M2Add
        };
    }
}
