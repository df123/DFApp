using DF.Telegram.Lottery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace DF.Telegram.Background
{
    public class LotteryResultTimer : QuartzBackgroundWorkerBase
    {
        private readonly IRepository<LotteryResult, long> _lotteryResultRepository;
        private readonly ILotteryService _lotteryService;
        private readonly IObjectMapper _mapper;
        public LotteryResultTimer(IRepository<LotteryResult, long> lotteryResultRepository
            , ILotteryService lotteryService
            , IObjectMapper mapper)
        {
            JobDetail = JobBuilder
                .Create<LotteryResultTimer>()
                .WithIdentity(nameof(LotteryResultTimer))
                .Build();
            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(LotteryResultTimer))
                .WithCronSchedule("0 06 23 * * ?")
                .Build();
            _lotteryResultRepository = lotteryResultRepository;
            _lotteryService = lotteryService;
            _mapper = mapper;
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
                    await GetCurrentLotteryResult();
                }
            }
        }

        public async Task GetCurrentLotteryResult()
        {
            string dayStart, dayEnd;
            dayStart = dayEnd = DateTime.Now.ToString("yyyy-MM-dd");
            LotteryInputDto dto = await _lotteryService.GetLotteryResult(dayStart, dayEnd, 1);
            if(dto.Result != null && dto.Result.Count >= 0)
            {
                List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result);

                await _lotteryResultRepository.InsertManyAsync(result);
            }
        }

        public async Task GetAllLotteryResults(string dayStart, string dayEnd, int pageNo)
        {
            LotteryInputDto dto = await _lotteryService.GetLotteryResult(dayStart, dayEnd, pageNo);

            List<LotteryResult> result = _mapper.Map<List<ResultItemDto>, List<LotteryResult>>(dto.Result!);

            await _lotteryResultRepository.InsertManyAsync(result);

            if (dto.PageNo < dto.PageNum)
            {
                await GetAllLotteryResults(dayStart, dayEnd, pageNo + 1);
            }

        }


    }
}
