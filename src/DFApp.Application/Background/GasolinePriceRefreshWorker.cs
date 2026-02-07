using DFApp.Configuration;
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
        private readonly IGasolinePriceService _gasolinePriceService;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly ILogger<GasolinePriceRefreshWorker> _logger;

        public GasolinePriceRefreshWorker(
            IGasolinePriceService gasolinePriceService,
            IConfigurationInfoRepository configurationInfoRepository,
            ILogger<GasolinePriceRefreshWorker> logger)
        {
            _gasolinePriceService = gasolinePriceService;
            _configurationInfoRepository = configurationInfoRepository;
            _logger = logger;

            JobDetail = JobBuilder
                .Create<GasolinePriceRefreshWorker>()
                .WithIdentity(nameof(GasolinePriceRefreshWorker))
                .Build();

            // 每天凌晨2点执行
            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(GasolinePriceRefreshWorker))
                .WithCronSchedule("0 0 2 * * ?")
                .Build();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("开始执行油价刷新任务");
            
            try
            {
                // 从配置获取默认省份
                string defaultProvince = "山东";
                try
                {
                    defaultProvince = await _configurationInfoRepository.GetConfigurationInfoValue("OilProvince", "DFApp.ElectricVehicle");
                    if (string.IsNullOrWhiteSpace(defaultProvince))
                    {
                        defaultProvince = "山东";
                    }
                }
                catch
                {
                    defaultProvince = "山东";
                }

                await _gasolinePriceService.RefreshGasolinePricesAsync(new RefreshGasolinePriceDto { Province = defaultProvince });
                
                _logger.LogInformation("油价刷新任务执行成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "油价刷新任务执行失败");
            }
        }
    }
}
