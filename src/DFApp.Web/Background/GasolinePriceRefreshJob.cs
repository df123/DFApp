using DFApp.Web.Services.ElectricVehicle;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace DFApp.Web.Background
{
    /// <summary>
    /// 油价数据刷新定时任务，每天晚上21点从外部 API 获取最新油价并更新数据库
    /// </summary>
    public class GasolinePriceRefreshJob : IJob
    {
        private readonly GasolinePriceRefresher _refresher;
        private readonly ILogger<GasolinePriceRefreshJob> _logger;

        public GasolinePriceRefreshJob(
            GasolinePriceRefresher refresher,
            ILogger<GasolinePriceRefreshJob> logger)
        {
            _refresher = refresher;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("开始执行油价刷新任务（刷新全部省份）");

            try
            {
                await _refresher.RefreshGasolinePricesAsync();

                _logger.LogInformation("油价刷新任务执行成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "油价刷新任务执行失败");
            }
        }
    }
}
