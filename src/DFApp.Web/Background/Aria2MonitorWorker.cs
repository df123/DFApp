using DFApp.Aria2;
using DFApp.Configuration;
using DFApp.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DFApp.Web.Background
{
    /// <summary>
    /// Aria2 实时监控后台服务
    /// </summary>
    public class Aria2MonitorWorker : BackgroundService
    {
        private readonly Aria2RpcClient _aria2Client;
        private readonly IHubContext<Aria2Hub> _hubContext;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly ILogger<Aria2MonitorWorker> _logger;

        private List<string>? _lastActiveGids = new List<string>();
        private DateTime _lastUpdateTime = DateTime.MinValue;

        public Aria2MonitorWorker(
            Aria2RpcClient aria2Client,
            IHubContext<Aria2Hub> hubContext,
            IConfigurationInfoRepository configurationInfoRepository,
            ILogger<Aria2MonitorWorker> logger)
        {
            _aria2Client = aria2Client;
            _hubContext = hubContext;
            _configurationInfoRepository = configurationInfoRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Aria2 监控服务启动");

            // 错误计数器，避免频繁日志
            int errorCount = 0;
            DateTime lastErrorTime = DateTime.MinValue;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 检查是否启用监控
                    var enableMonitor = await _configurationInfoRepository.GetConfigurationInfoValue("Aria2EnableMonitor", "DFApp.Web.Background.Aria2MonitorWorker");
                    if (!string.IsNullOrWhiteSpace(enableMonitor) && enableMonitor.ToLower() == "false")
                    {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    // 获取全局状态
                    var globalStat = await _aria2Client.GetGlobalStatAsync();
                    await _hubContext.Clients.Group("Aria2Monitor").SendAsync("ReceiveGlobalStat", globalStat, stoppingToken);

                    // 获取活跃任务
                    var activeTasks = await _aria2Client.TellActiveAsync();
                    var currentActiveGids = activeTasks.Select(t => t.Gid).ToList();

                    // 检查是否有新任务或任务状态变化
                    if (!AreGidsEqual(_lastActiveGids, currentActiveGids) ||
                        (DateTime.UtcNow - _lastUpdateTime).TotalSeconds >= 5)
                    {
                        // 推送活跃任务列表
                        await _hubContext.Clients.Group("Aria2Monitor").SendAsync("ReceiveActiveTasks", activeTasks, stoppingToken);

                        // 获取等待任务
                        var waitingTasks = await _aria2Client.TellWaitingAsync();
                        await _hubContext.Clients.Group("Aria2Monitor").SendAsync("ReceiveWaitingTasks", waitingTasks, stoppingToken);

                        // 获取停止的任务（最新的10个）
                        var stoppedTasks = await _aria2Client.TellStoppedAsync(0, 10);
                        await _hubContext.Clients.Group("Aria2Monitor").SendAsync("ReceiveStoppedTasks", stoppedTasks, stoppingToken);

                        _lastActiveGids = currentActiveGids;
                        _lastUpdateTime = DateTime.UtcNow;
                    }

                    // 重置错误计数
                    errorCount = 0;

                    // 等待一段时间后再次检查
                    await Task.Delay(2000, stoppingToken);
                }
                catch (Exception ex)
                {
                    // 只在第一次错误或距离上次错误超过1分钟时记录日志
                    errorCount++;
                    if (errorCount == 1 || (DateTime.UtcNow - lastErrorTime).TotalMinutes >= 1)
                    {
                        _logger.LogError(ex, "Aria2 监控服务出错（连续错误次数: {ErrorCount}）。请检查 aria2 服务是否运行以及 RPC URL 配置是否正确。", errorCount);
                        lastErrorTime = DateTime.UtcNow;
                    }

                    // 出错后等待更长时间
                    await Task.Delay(10000, stoppingToken);
                }
            }
        }

        /// <summary>
        /// 比较 GID 列表是否相同
        /// </summary>
        private bool AreGidsEqual(List<string>? list1, List<string>? list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            var set1 = new HashSet<string>(list1);
            return set1.SetEquals(list2);
        }
    }
}
