using DFApp.ElectricVehicle;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers.Quartz;

namespace DFApp.Background
{
    public class GasolinePriceRefreshWorker : QuartzBackgroundWorkerBase
    {
        private readonly GasolinePriceRefresher _gasolinePriceRefresher;
        private readonly ILogger<GasolinePriceRefreshWorker> _logger;

        public GasolinePriceRefreshWorker(
            GasolinePriceRefresher gasolinePriceRefresher,
            ILogger<GasolinePriceRefreshWorker> logger)
        {
            _gasolinePriceRefresher = gasolinePriceRefresher;
            _logger = logger;

            JobDetail = JobBuilder
                .Create<GasolinePriceRefreshWorker>()
                .WithIdentity(nameof(GasolinePriceRefreshWorker))
                .Build();

            // 每天晚上9点执行
            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(GasolinePriceRefreshWorker))
                .WithCronSchedule("0 0 21 * * ?")
                .Build();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("开始执行油价刷新任务（刷新全部省份）");

            try
            {
                await _gasolinePriceRefresher.RefreshGasolinePricesAsync();

                _logger.LogInformation("油价刷新任务执行成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "油价刷新任务执行失败");
            }
        }
    }
}
