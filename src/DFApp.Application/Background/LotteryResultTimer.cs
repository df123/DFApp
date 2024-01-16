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
            Logger.LogInformation($"开始任务......{lotteryType}");
            List<LotteryResult> result = await _resultReadOnly.GetListAsync(item => item.Code == code && item.Name == lotteryType);

            if (result == null || result.Count <= 0)
            {
                string dayStart, dayEnd;
                dayStart = "2013-01-01";//KL8的开始时间是2020年小于2013年所有可以直接用2013
                dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
                await GetAllLotteryResults(dayStart, dayEnd, 1, lotteryTypeEng);
            }

            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                || DateTime.Now.DayOfWeek == DayOfWeek.Thursday
                || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday)
            {
                string day = DateTime.Now.ToString("yyyy-MM-dd");
                List<LotteryResult> result1 = await _resultReadOnly.GetListAsync(item => item.Date != null && item.Date.StartsWith(day));
                if (result1 == null || result1.Count <= 0)
                {
                    using (var uom = _unitOfWorkManager.Begin())
                    {
                        await UpdatePrizegrades(lotteryType, lotteryTypeEng);
                        LotteryResult lotteryResult = (await _resultReadOnly.GetQueryableAsync()).OrderByDescending(x => x.Code).First();
                        string dayStart = (lotteryResult.Date!.Split('('))[0];
                        dayStart = DateTime.Parse(dayStart).AddDays(1).ToString("yyyy-MM-dd");
                        await GetCurrentLotteryResult(dayStart, 0, lotteryTypeEng);
                        await uom.CompleteAsync();
                    }
                }
            }

            Logger.LogInformation($"任务结束......{lotteryType}");
        }


        private async Task GetCurrentLotteryResult(string dayStart, int pageNo, string lotteryType)
        {
            string dayEnd;
            dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
            LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, 1, lotteryType);
            if (dto.Result != null && dto.Result.Count >= 0)
            {
                List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result);

                await _lotteryResultRepository.InsertManyAsync(result);


                if (dto.PageNo < dto.PageNum)
                {
                    await GetCurrentLotteryResult(dayStart, pageNo + 1, lotteryType);
                }
            }
        }

        private async Task GetAllLotteryResults(string dayStart, string dayEnd, int pageNo, string lotteryType)
        {
            LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, pageNo, lotteryType);

            List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result!);

            await _lotteryResultRepository.InsertManyAsync(result);

            if (dto.PageNo < dto.PageNum)
            {
                await GetAllLotteryResults(dayStart, dayEnd, pageNo + 1, lotteryType);
            }

        }

        private async Task UpdatePrizegrades(string lotteryType, string lotteryTypeEng)
        {
            var query = from x in await _resultReadOnly.GetQueryableAsync()
                        join y in await _prizegradesReadOnly.GetQueryableAsync()
                        on x.Id equals y.LotteryResultId into z
                        from z2 in z.DefaultIfEmpty()
                        where x.Name == lotteryType
                        select new { result = x, prize = z2 };

            var queryList = query.ToList();


            foreach (var item in queryList)
            {
                if (item.prize == null)
                {
                    string dayStart = (item.result.Date!.Split('('))[0];

                    LotteryInputDto dto = await GetLotteryResult(dayStart, dayStart, 1, lotteryTypeEng);

                    if (dto.Result != null && dto.Result.Count >= 0)
                    {
                        List<LotteryResult> resultPrize = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result);

                        foreach (var prize in resultPrize)
                        {
                            if (prize.Prizegrades != null)
                            {
                                foreach (var prizeId in prize.Prizegrades)
                                {
                                    prizeId.LotteryResultId = item.result.Id;
                                }

                                await _lotteryPrizegradesRepository.InsertManyAsync(prize.Prizegrades);
                            }
                        }
                    }
                }
            }
        }

        private async Task<LotteryInputDto> GetLotteryResult(string dayStart, string dayEnd, int pageNo, string lotteryType)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Host", "www.cwl.gov.cn");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:102.0) Gecko/20100101 Firefox/102.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");

            HttpResponseMessage message = await client.GetAsync($"https://www.cwl.gov.cn/cwl_admin/front/cwlkj/search/kjxx/findDrawNotice?name={lotteryType}&issueCount=&issueStart=&issueEnd=&dayStart={dayStart}&dayEnd={dayEnd}&pageNo={pageNo}&pageSize=30&week=&systemType=PC");

            message.EnsureSuccessStatusCode();

            LotteryInputDto? dto = JsonSerializer.Deserialize<LotteryInputDto>(await message.Content.ReadAsStringAsync());

            if (dto == null)
            {
                dto = new LotteryInputDto();
            }

            try
            {
                Logger.LogInformation($"获取到数据：{dto.Total}");
            }
            catch (Exception)
            {

                Logger.LogInformation($"获取到数据失败");
            }

            return dto;

        }


    }
}
