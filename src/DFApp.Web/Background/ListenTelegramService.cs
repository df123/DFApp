using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Background;

/// <summary>
/// Telegram 监听后台服务
/// </summary>
public class ListenTelegramService : BackgroundService
{
    private readonly ILogger<ListenTelegramService> _logger;

    public ListenTelegramService(ILogger<ListenTelegramService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 需要用户配置的参数名称（null 表示已连接）
    /// </summary>
    public string? ConfigNeeded { get; private set; } = "start";

    /// <summary>
    /// 已连接的用户信息
    /// </summary>
    public string? User { get; private set; }

    /// <summary>
    /// Telegram 客户端实例
    /// </summary>
    public object? TGClinet { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram 监听服务启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: 实现 Telegram 客户端监听逻辑
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // 正常停止
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telegram 监听服务出错");
                await Task.Delay(10000, stoppingToken);
            }
        }

        _logger.LogInformation("Telegram 监听服务停止");
    }

    /// <summary>
    /// 处理登录配置
    /// </summary>
    public Task<string> DoLogin(string value)
    {
        throw new NotImplementedException();
    }
}
