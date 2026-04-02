using DFApp.Rss;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace DFApp.Web.Background
{
    /// <summary>
    /// 磁盘空间检查定时任务，处理因空间不足而暂存的下载任务
    /// </summary>
    public class DiskSpaceCheckJob : IJob
    {
        private readonly IRssSubscriptionService _rssSubscriptionService;
        private readonly ILogger<DiskSpaceCheckJob> _logger;

        public DiskSpaceCheckJob(
            IRssSubscriptionService rssSubscriptionService,
            ILogger<DiskSpaceCheckJob> logger)
        {
            _rssSubscriptionService = rssSubscriptionService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("开始执行磁盘空间检查任务");

            try
            {
                await _rssSubscriptionService.ProcessPendingDownloadsAsync();

                _logger.LogInformation("磁盘空间检查任务完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "磁盘空间检查任务执行失败");
            }
        }
    }
}
