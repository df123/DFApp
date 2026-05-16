using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DFApp.Aria2;
using DFApp.Aria2.Notifications;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Background;

/// <summary>
/// Aria2 后台处理服务，通过 WebSocket 连接 Aria2 服务接收下载通知
/// </summary>
public class Aria2BackgroundWorker : BackgroundService
{
    private readonly Aria2Manager _manager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Aria2BackgroundWorker> _logger;

    public Aria2BackgroundWorker(
        Aria2Manager manager,
        IServiceProvider serviceProvider,
        ILogger<Aria2BackgroundWorker> logger)
    {
        _manager = manager;
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
                string aria2ws;
                using (var scope = _serviceProvider.CreateScope())
                {
                    var configRepo = scope.ServiceProvider.GetRequiredService<IConfigurationInfoRepository>();
                    aria2ws = await configRepo.GetConfigurationInfoValue("aria2ws", "DFApp.Web.Background.Aria2BackgroundWorker");
                }

                if (string.IsNullOrWhiteSpace(aria2ws))
                {
                    _logger.LogWarning("Aria2 WebSocket 连接地址未配置，10秒后重试");
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }

                using var clientWebSocket = new ClientWebSocket();
                await clientWebSocket.ConnectAsync(new Uri(aria2ws), stoppingToken);
                _logger.LogInformation("已连接到 Aria2 WebSocket: {Url}", aria2ws);

                await ReceiveMessagesAsync(clientWebSocket, stoppingToken);

                _logger.LogInformation("Aria2 WebSocket 连接已断开，5秒后重试");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aria2 WebSocket 连接异常，5秒后重试");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Aria2 后台处理服务停止");
    }

    /// <summary>
    /// 接收 WebSocket 消息并处理
    /// </summary>
    private async Task ReceiveMessagesAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = new byte[1024 * 1024 * 10];
        while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
        {
            try
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessMessageAsync(message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接收 Aria2 WebSocket 消息异常");
            }
        }
    }

    /// <summary>
    /// 处理接收到的 WebSocket 通知消息
    /// </summary>
    private async Task ProcessMessageAsync(string message)
    {
        try
        {
            if (message.Contains("\"error\":"))
            {
                _logger.LogDebug("Aria2 错误消息: {Message}", message);
                return;
            }

            _logger.LogDebug("Aria2 消息: {Message}", message);

            if (message.Contains("\"method\":"))
            {
                var notification = JsonSerializer.Deserialize<Aria2Notification>(message);
                if (notification != null)
                {
                    await _manager.ProcessResponseAsync(notification);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Aria2 消息异常");
        }
    }
}
