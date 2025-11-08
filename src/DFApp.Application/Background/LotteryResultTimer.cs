using DFApp.Lottery;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace DFApp.Background
{
    public class LotteryResultTimer : QuartzBackgroundWorkerBase
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultRepository;
        private readonly IReadOnlyRepository<LotteryResult, long> _resultReadOnly;
        private readonly IRepository<LotteryPrizegrades, long> _lotteryPrizegradesRepository;
        private readonly IReadOnlyRepository<LotteryPrizegrades, long> _prizegradesReadOnly;

        private readonly IObjectMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        public LotteryResultTimer(IRepository<LotteryResult, long> lotteryResultRepository
            , IReadOnlyRepository<LotteryResult, long> resultReadOnly
            , IRepository<LotteryPrizegrades, long> lotteryPrizegradesRepository
            , IReadOnlyRepository<LotteryPrizegrades, long> prizegradesReadOnly
            , IObjectMapper mapper
            , IHttpClientFactory httpClientFactory
            , IUnitOfWorkManager unitOfWorkManager)
        {
            JobDetail = JobBuilder
                .Create<LotteryResultTimer>()
                .WithIdentity(nameof(LotteryResultTimer))
                .Build();
            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(LotteryResultTimer))
                .WithCronSchedule("0 0 23 * * ?")
                .Build();
            _lotteryResultRepository = lotteryResultRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _unitOfWorkManager = unitOfWorkManager;
            _lotteryPrizegradesRepository = lotteryPrizegradesRepository;
            _prizegradesReadOnly = prizegradesReadOnly;
            _resultReadOnly = resultReadOnly;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            await StartWork(LotteryConst.SSQ, LotteryConst.SSQ_ENG, LotteryConst.SSQ_START_CODE);
            await StartWork(LotteryConst.KL8, LotteryConst.KL8_ENG, LotteryConst.KL8_STRAT_CODE);
        }

        private async Task StartWork(string lotteryType, string lotteryTypeEng, string code)
        {
            Logger.LogInformation($"开始任务......{lotteryType} (英文类型: {lotteryTypeEng}, 起始代码: {code})");
            
            try
            {
                // 检查是否已有数据
                Logger.LogInformation($"检查数据库中是否存在彩票类型为 {lotteryType} 且代码为 {code} 的数据");
                List<LotteryResult> result = await _resultReadOnly.GetListAsync(item => item.Code == code && item.Name == lotteryType);
                Logger.LogInformation($"查询到 {result?.Count ?? 0} 条历史数据");

                if (result == null || result.Count <= 0)
                {
                    Logger.LogInformation($"未找到历史数据，开始获取所有历史数据");
                    string dayStart, dayEnd;
                    dayStart = "2013-01-01";//KL8的开始时间是2020年小于2013年所有可以直接用2013
                    dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
                    Logger.LogInformation($"获取历史数据范围: {dayStart} 至 {dayEnd}");
                    await GetAllLotteryResults(dayStart, dayEnd, 1, lotteryTypeEng);
                }
                else
                {
                    Logger.LogInformation($"已存在历史数据，跳过历史数据获取");
                }

                // 检查是否需要获取最新数据
                bool shouldFetchLatest = DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                    || DateTime.Now.DayOfWeek == DayOfWeek.Thursday
                    || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday
                    || lotteryType == LotteryConst.KL8;
                
                Logger.LogInformation($"是否需要获取最新数据: {shouldFetchLatest} (当前星期: {DateTime.Now.DayOfWeek})");

                if (shouldFetchLatest)
                {
                    string day = DateTime.Now.ToString("yyyy-MM-dd");
                    Logger.LogInformation($"检查今天 ({day}) 是否已有数据");
                    List<LotteryResult> result1 = await _resultReadOnly.GetListAsync(item => item.Date != null && item.Date.StartsWith(day));
                    Logger.LogInformation($"今天的数据条数: {result1?.Count ?? 0}");
                    
                    if (result1 == null || result1.Count <= 0)
                    {
                        Logger.LogInformation("今天没有数据，开始获取最新数据");
                        using (var uom = _unitOfWorkManager.Begin())
                        {
                            try
                            {
                                Logger.LogInformation("开始更新奖级信息");
                                await UpdatePrizegrades(lotteryType, lotteryTypeEng);
                                
                                Logger.LogInformation("获取最新一期开奖结果作为起始点");
                                LotteryResult lotteryResult = (await _resultReadOnly.GetQueryableAsync()).OrderByDescending(x => x.Code).First();
                                string dayStart = (lotteryResult.Date!.Split('('))[0];
                                dayStart = DateTime.Parse(dayStart).AddDays(1).ToString("yyyy-MM-dd");
                                Logger.LogInformation($"从 {dayStart} 开始获取最新数据");
                                
                                await GetCurrentLotteryResult(dayStart, 0, lotteryTypeEng);
                                await uom.CompleteAsync();
                                Logger.LogInformation("最新数据获取完成并提交事务");
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "获取最新数据时发生异常");
                                await uom.RollbackAsync();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        Logger.LogInformation("今天已有数据，跳过最新数据获取");
                    }
                }

                Logger.LogInformation($"任务成功结束......{lotteryType}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"任务执行失败......{lotteryType}");
                throw;
            }
        }


        private async Task GetCurrentLotteryResult(string dayStart, int pageNo, string lotteryType)
        {
            string dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
            Logger.LogInformation($"获取当前彩票结果 - 起始日期: {dayStart}, 结束日期: {dayEnd}, 彩票类型: {lotteryType}, 页码: {pageNo}");
            
            LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, 1, lotteryType);
            
            if (dto.Result != null && dto.Result.Count > 0)
            {
                Logger.LogInformation($"获取到 {dto.Result.Count} 条数据，开始映射并保存到数据库");
                List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result);

                try
                {
                    await _lotteryResultRepository.InsertManyAsync(result);
                    Logger.LogInformation($"成功保存 {result.Count} 条彩票结果到数据库");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "保存彩票结果到数据库时发生异常");
                    throw;
                }

                // 检查是否需要获取下一页数据
                if (dto.PageNo < dto.PageNum)
                {
                    Logger.LogInformation($"当前页 {dto.PageNo} 小于总页数 {dto.PageNum}，继续获取下一页数据");
                    await GetCurrentLotteryResult(dayStart, pageNo + 1, lotteryType);
                }
                else
                {
                    Logger.LogInformation($"已获取所有页数据，当前页 {dto.PageNo}，总页数 {dto.PageNum}");
                }
            }
            else
            {
                Logger.LogInformation("未获取到任何数据，结束当前彩票类型的数据获取");
            }
        }

        private async Task GetAllLotteryResults(string dayStart, string dayEnd, int pageNo, string lotteryType)
        {
            Logger.LogInformation($"获取所有历史彩票结果 - 起始日期: {dayStart}, 结束日期: {dayEnd}, 彩票类型: {lotteryType}, 页码: {pageNo}");
            
            LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, pageNo, lotteryType);
            
            if (dto.Result != null && dto.Result.Count > 0)
            {
                Logger.LogInformation($"获取到 {dto.Result.Count} 条历史数据，开始映射并保存到数据库");
                List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result);

                try
                {
                    await _lotteryResultRepository.InsertManyAsync(result);
                    Logger.LogInformation($"成功保存 {result.Count} 条历史彩票结果到数据库");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "保存历史彩票结果到数据库时发生异常");
                    throw;
                }

                // 检查是否需要获取下一页数据
                if (dto.PageNo < dto.PageNum)
                {
                    Logger.LogInformation($"当前页 {dto.PageNo} 小于总页数 {dto.PageNum}，继续获取下一页历史数据");
                    await GetAllLotteryResults(dayStart, dayEnd, pageNo + 1, lotteryType);
                }
                else
                {
                    Logger.LogInformation($"已获取所有历史页数据，当前页 {dto.PageNo}，总页数 {dto.PageNum}");
                }
            }
            else
            {
                Logger.LogInformation("未获取到任何历史数据，结束历史数据获取");
            }
        }

        private async Task UpdatePrizegrades(string lotteryType, string lotteryTypeEng)
        {
            Logger.LogInformation($"开始更新奖级信息 - 彩票类型: {lotteryType} (英文: {lotteryTypeEng})");
            
            try
            {
                // 查询没有奖级信息的彩票结果
                Logger.LogInformation("查询没有奖级信息的彩票结果");
                var query = from x in await _resultReadOnly.GetQueryableAsync()
                            join y in await _prizegradesReadOnly.GetQueryableAsync()
                            on x.Id equals y.LotteryResultId into z
                            from z2 in z.DefaultIfEmpty()
                            where x.Name == lotteryType
                            select new { result = x, prize = z2 };

                var queryList = query.ToList();
                Logger.LogInformation($"查询到 {queryList.Count} 条彩票结果记录");
                
                int noPrizeCount = queryList.Count(x => x.prize == null);
                Logger.LogInformation($"其中 {noPrizeCount} 条记录没有奖级信息");

                int processedCount = 0;
                foreach (var item in queryList)
                {
                    if (item.prize == null)
                    {
                        Logger.LogInformation($"处理彩票结果 ID: {item.result.Id}, 代码: {item.result.Code}, 日期: {item.result.Date}");
                        
                        try
                        {
                            string dayStart = (item.result.Date!.Split('('))[0];
                            Logger.LogInformation($"获取 {dayStart} 的奖级信息");

                            LotteryInputDto dto = await GetLotteryResult(dayStart, dayStart, 1, lotteryTypeEng);

                            if (dto.Result != null && dto.Result.Count > 0)
                            {
                                Logger.LogInformation($"获取到 {dto.Result.Count} 条奖级数据");
                                List<LotteryResult> resultPrize = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result);

                                foreach (var prize in resultPrize)
                                {
                                    if (prize.Prizegrades != null && prize.Prizegrades.Count > 0)
                                    {
                                        Logger.LogInformation($"为彩票结果 ID: {item.result.Id} 添加 {prize.Prizegrades.Count} 条奖级信息");
                                        
                                        foreach (var prizeId in prize.Prizegrades)
                                        {
                                            prizeId.LotteryResultId = item.result.Id;
                                        }

                                        await _lotteryPrizegradesRepository.InsertManyAsync(prize.Prizegrades);
                                        processedCount++;
                                    }
                                    else
                                    {
                                        Logger.LogWarning($"彩票结果代码: {prize.Code} 没有奖级信息");
                                    }
                                }
                            }
                            else
                            {
                                Logger.LogWarning($"未获取到 {dayStart} 的奖级数据");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"处理彩票结果 ID: {item.result.Id} 的奖级信息时发生异常");
                        }
                    }
                }
                
                Logger.LogInformation($"奖级信息更新完成，共处理 {processedCount} 条记录");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "更新奖级信息时发生异常");
                throw;
            }
        }

        private async Task<LotteryInputDto> GetLotteryResult(string dayStart, string dayEnd, int pageNo, string lotteryType)
        {
            string requestUrl = $"https://www.cwl.gov.cn/cwl_admin/front/cwlkj/search/kjxx/findDrawNotice?name={lotteryType}&issueCount=&issueStart=&issueEnd=&dayStart={dayStart}&dayEnd={dayEnd}&pageNo={pageNo}&pageSize=30&week=&systemType=PC";
            
            Logger.LogInformation($"开始获取彩票数据 - 彩票类型: {lotteryType}, 开始日期: {dayStart}, 结束日期: {dayEnd}, 页码: {pageNo}");
            Logger.LogInformation($"请求URL: {requestUrl}");
            
            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Host", "www.cwl.gov.cn");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:102.0) Gecko/20100101 Firefox/102.0");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");

                Logger.LogInformation("发送HTTP请求...");
                
                HttpResponseMessage message = await client.GetAsync(requestUrl);
                
                Logger.LogInformation($"HTTP响应状态码: {(int)message.StatusCode} ({message.StatusCode})");
                
                message.EnsureSuccessStatusCode();
                
                string responseContent = await message.Content.ReadAsStringAsync();
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
                
                LotteryInputDto? dto = JsonSerializer.Deserialize<LotteryInputDto>(responseContent);
                
                if (dto == null)
                {
                    Logger.LogWarning("反序列化响应失败，响应为null，创建空对象");
                    dto = new LotteryInputDto();
                }
                else
                {
                    Logger.LogInformation($"反序列化成功 - 总数据量: {dto.Total}, 当前页: {dto.PageNo}/{dto.PageNum}, 每页大小: {dto.PageSize}");
                    
                    if (dto.Result != null)
                    {
                        Logger.LogInformation($"当前页数据条数: {dto.Result.Count}");
                        
                        // 记录第一条数据的详细信息
                        if (dto.Result.Count > 0)
                        {
                            var firstResult = dto.Result[0];
                            Logger.LogInformation($"第一条数据 - 彩票类型: {firstResult.Name}, 期号: {firstResult.Code}, 开奖日期: {firstResult.Date}, 红球: {firstResult.Red}, 蓝球: {firstResult.Blue}");
                        }
                    }
                    else
                    {
                        Logger.LogWarning("响应中的Result字段为null");
                    }
                }

                return dto;
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, $"HTTP请求异常: {ex.Message}");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogError(ex, $"请求超时: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, $"JSON解析异常: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"未知异常: {ex.Message}");
                throw;
            }
        }


    }
}
