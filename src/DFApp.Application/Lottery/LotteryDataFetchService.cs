using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace DFApp.Lottery
{
    [Authorize(DFAppPermissions.Lottery.Default)]
    public class LotteryDataFetchService : DFAppAppService, ILotteryDataFetchService
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultRepository;
        private readonly IObjectMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IConfiguration _configuration;

        public LotteryDataFetchService(
            IRepository<LotteryResult, long> lotteryResultRepository,
            IObjectMapper mapper,
            IHttpClientFactory httpClientFactory,
            IUnitOfWorkManager unitOfWorkManager,
            IConfiguration configuration)
        {
            _lotteryResultRepository = lotteryResultRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _unitOfWorkManager = unitOfWorkManager;
            _configuration = configuration;
        }

        public async Task<LotteryDataFetchResponseDto> FetchLotteryData(LotteryDataFetchRequestDto input)
        {
            var response = new LotteryDataFetchResponseDto();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                Logger.LogInformation($"开始获取彩票数据 - 彩票类型: {input.LotteryType}, 开始日期: {input.DayStart}, 结束日期: {input.DayEnd}, 页码: {input.PageNo}");
                
                // 使用代理服务器获取数据
                string proxyServerUrl = LotteryConst.GetLotteryProxyUrl(_configuration);
                string queryString = $"name={input.LotteryType}&issueCount=&issueStart=&issueEnd=&dayStart={input.DayStart}&dayEnd={input.DayEnd}&pageNo={input.PageNo}&pageSize=30&week=&systemType=PC";
                string requestUrl = $"{proxyServerUrl}/api/proxy/lottery/findDrawNotice?{queryString}";
                response.RequestUrl = requestUrl;
                
                Logger.LogInformation($"通过代理服务器请求URL: {requestUrl}");
                
                // 创建HTTP客户端
                using var client = _httpClientFactory.CreateClient();
                
                Logger.LogInformation("发送代理HTTP请求...");
                
                // 发送请求到代理服务器
                var httpResponse = await client.GetAsync(requestUrl);
                response.StatusCode = (int)httpResponse.StatusCode;
                
                Logger.LogInformation($"HTTP响应状态码: {response.StatusCode}");
                
                // 确保请求成功
                httpResponse.EnsureSuccessStatusCode();
                
                // 读取响应内容
                string responseContent = await httpResponse.Content.ReadAsStringAsync();
                Logger.LogInformation($"响应内容长度: {responseContent.Length} 字符");
                
                // 记录响应内容（仅前500字符，避免日志过长）
                if (responseContent.Length > 500)
                {
                    Logger.LogInformation($"响应内容前500字符: {responseContent.Substring(0, 500)}...");
                }
                else
                {
                    Logger.LogInformation($"响应内容: {responseContent}");
                }
                
                // 反序列化响应
                LotteryInputDto? dto = JsonSerializer.Deserialize<LotteryInputDto>(responseContent);
                
                if (dto == null)
                {
                    Logger.LogWarning("反序列化响应失败，响应为null");
                    dto = new LotteryInputDto();
                }
                
                response.Data = dto;
                response.Success = true;
                response.Message = $"成功获取到 {dto.Total} 条数据";
                
                Logger.LogInformation($"获取到数据总数: {dto.Total}, 当前页: {dto.PageNo}/{dto.PageNum}, 每页大小: {dto.PageSize}");
                
                if (dto.Result != null && dto.Result.Count > 0)
                {
                    Logger.LogInformation($"当前页数据条数: {dto.Result.Count}");
                    
                    // 记录第一条数据的详细信息
                    if (dto.Result.Count > 0)
                    {
                        var firstResult = dto.Result[0];
                        Logger.LogInformation($"第一条数据 - 彩票类型: {firstResult.Name}, 期号: {firstResult.Code}, 开奖日期: {firstResult.Date}, 红球: {firstResult.Red}, 蓝球: {firstResult.Blue}");
                    }
                    
                    // 如果需要保存到数据库
                    if (input.SaveToDatabase)
                    {
                        Logger.LogInformation("开始保存数据到数据库...");
                        
                        using (var uom = _unitOfWorkManager.Begin())
                        {
                            try
                            {
                                List<LotteryResult> results = new List<LotteryResult>();
                                
                                foreach (var item in dto.Result)
                                {
                                    var lotteryResult = _mapper.Map<ResultItemDto, LotteryResult>(item);
                                    
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
                                                Result = lotteryResult // 设置导航属性
                                            };
                                            lotteryResult.Prizegrades.Add(lotteryPrizegrade);
                                        }
                                    }
                                    
                                    results.Add(lotteryResult);
                                }
                                
                                await _lotteryResultRepository.InsertManyAsync(results);
                                response.SavedCount = results.Count;
                                
                                await uom.CompleteAsync();
                                Logger.LogInformation($"成功保存 {response.SavedCount} 条数据到数据库");
                            }
                            catch (Exception ex)
                            {
                                await uom.RollbackAsync();
                                Logger.LogError(ex, "保存数据到数据库时发生错误");
                                response.Message += $", 但保存到数据库失败: {ex.Message}";
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogWarning("未获取到任何数据");
                    response.Message = "未获取到任何数据";
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, $"HTTP请求异常: {ex.Message}");
                response.Success = false;
                response.Message = $"HTTP请求异常: {ex.Message}";
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogError(ex, $"请求超时: {ex.Message}");
                response.Success = false;
                response.Message = $"请求超时: {ex.Message}";
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, $"JSON解析异常: {ex.Message}");
                response.Success = false;
                response.Message = $"JSON解析异常: {ex.Message}";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"未知异常: {ex.Message}");
                response.Success = false;
                response.Message = $"未知异常: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                response.ResponseTime = stopwatch.ElapsedMilliseconds;
                Logger.LogInformation($"请求完成，耗时: {response.ResponseTime} 毫秒");
            }
            
            return response;
        }

        public async Task<LotteryDataFetchResponseDto> FetchSSQLatestData()
        {
            Logger.LogInformation("获取双色球最新数据");
            
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

        public async Task<LotteryDataFetchResponseDto> FetchKL8LatestData()
        {
            Logger.LogInformation("获取快乐8最新数据");
            
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

        public async Task<LotteryDataFetchResponseDto> TestLotteryApiConnection(string lotteryType = LotteryConst.SSQ_ENG)
        {
            Logger.LogInformation($"测试彩票API连接 - 彩票类型: {lotteryType}");
            
            var request = new LotteryDataFetchRequestDto
            {
                LotteryType = lotteryType,
                DayStart = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd"), // 测试最近7天的数据
                DayEnd = DateTime.Now.ToString("yyyy-MM-dd"),
                PageNo = 1,
                SaveToDatabase = false // 测试连接不保存数据
            };
            
            return await FetchLotteryData(request);
        }
    }
}