using DF.Telegram.Lottery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace DF.Telegram.Background
{
    public class LotteryResultTimer : QuartzBackgroundWorkerBase
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultRepository;
        private readonly IObjectMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        public LotteryResultTimer(IRepository<LotteryResult, long> lotteryResultRepository
            , IObjectMapper mapper
            , IHttpClientFactory httpClientFactory)
        {
            JobDetail = JobBuilder
                .Create<LotteryResultTimer>()
                .WithIdentity(nameof(LotteryResultTimer))
                .Build();
            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(LotteryResultTimer))
                .WithCronSchedule("0 0 22 * * ?")
                .Build();
            _lotteryResultRepository = lotteryResultRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            List<LotteryResult> result = await _lotteryResultRepository.GetListAsync(item => item.Code == "2013001");

            if (result == null || result.Count <= 0)
            {
                string dayStart, dayEnd;
                dayStart = "2013-01-01";
                dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
                await GetAllLotteryResults(dayStart, dayEnd, 1);
            }

            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                || DateTime.Now.DayOfWeek == DayOfWeek.Thursday
                || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday)
            {
                string day = DateTime.Now.ToString("yyyy-MM-dd");
                List<LotteryResult> result1 = await _lotteryResultRepository.GetListAsync(item => item.Date != null && item.Date.StartsWith(day));
                if (result1 == null || result1.Count <= 0)
                {
                    LotteryResult lotteryResult = await _lotteryResultRepository.LastOrDefaultAsync();
                    string dayStart = (lotteryResult.Date!.Split('('))[0];
                    await GetCurrentLotteryResult(dayStart, 0);
                }
            }
        }

        private async Task GetCurrentLotteryResult(string dayStart, int pageNo)
        {
            string dayEnd;
            dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
            LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, 1);
            if (dto.Result != null && dto.Result.Count >= 0)
            {
                List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result);

                await _lotteryResultRepository.InsertManyAsync(result);


                if (dto.PageNo < dto.PageNum)
                {
                    await GetCurrentLotteryResult(dayStart, pageNo + 1);
                }
            }
        }

        public async Task GetAllLotteryResults(string dayStart, string dayEnd, int pageNo)
        {
            LotteryInputDto dto = await GetLotteryResult(dayStart, dayEnd, pageNo);

            List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result!);

            await _lotteryResultRepository.InsertManyAsync(result);

            if (dto.PageNo < dto.PageNum)
            {
                await GetAllLotteryResults(dayStart, dayEnd, pageNo + 1);
            }

        }

        private async Task<LotteryInputDto> GetLotteryResult(string dayStart, string dayEnd, int pageNo)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Host", "www.cwl.gov.cn");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:102.0) Gecko/20100101 Firefox/102.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");

            HttpResponseMessage message = await client.GetAsync($"https://www.cwl.gov.cn/cwl_admin/front/cwlkj/search/kjxx/findDrawNotice?name=ssq&issueCount=&issueStart=&issueEnd=&dayStart={dayStart}&dayEnd={dayEnd}&pageNo={pageNo}&pageSize=30&week=&systemType=PC");

            message.EnsureSuccessStatusCode();

            LotteryInputDto? dto = JsonSerializer.Deserialize<LotteryInputDto>(await message.Content.ReadAsStringAsync());

            if (dto == null)
            {
                dto = new LotteryInputDto();
            }

            return dto;

        }


    }
}
