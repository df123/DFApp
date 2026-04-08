using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Background;

/// <summary>
/// Aria2 后台处理服务，用于处理队列中的 Aria2 请求
/// </summary>
public class Aria2BackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Aria2BackgroundWorker> _logger;

    public Aria2BackgroundWorker(
        IServiceProvider serviceProvider,
        ILogger<Aria2BackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Aria2 后台处理服务启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: 实现队列消费和 Aria2 RPC 调用逻辑
                await Task.Delay(2000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // 正常停止
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aria2 后台处理服务出错");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Aria2 后台处理服务停止");
    }
}
