using DFApp.Rss;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.DependencyInjection;

namespace DFApp.Background
{
    /// <summary>
    /// 磁盘空间检查后台任务，处理因空间不足而暂存的下载
    /// </summary>
    public class DiskSpaceCheckWorker : QuartzBackgroundWorkerBase, ITransientDependency
    {
        private readonly IRssSubscriptionService _rssSubscriptionService;

        public DiskSpaceCheckWorker(
            IRssSubscriptionService rssSubscriptionService)
        {
            _rssSubscriptionService = rssSubscriptionService;

            JobDetail = JobBuilder
                .Create<DiskSpaceCheckWorker>()
                .WithIdentity(nameof(DiskSpaceCheckWorker))
                .Build();

            Trigger = TriggerBuilder
                .Create()
                .WithIdentity(nameof(DiskSpaceCheckWorker))
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(10)
                    .RepeatForever())
                .Build();
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            Logger.LogInformation("开始执行磁盘空间检查任务");

            try
            {
                await _rssSubscriptionService.ProcessPendingDownloadsAsync();

                Logger.LogInformation("磁盘空间检查任务完成");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "磁盘空间检查任务执行失败");
            }
        }
    }
}
