using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DFApp.Aria2;
using DFApp.Aria2.Notifications;
using DFApp.Aria2.Request;
using DFApp.Aria2.Response.TellStatus;
using DFApp.Rss;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Background;

/// <summary>
/// Aria2 后台处理服务，通过 WebSocket 连接 Aria2 服务
/// 接收下载通知、处理 RPC 响应、发送队列中的请求
/// </summary>
public class Aria2BackgroundWorker : BackgroundService
{
    private readonly Aria2Manager _manager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IQueueManagement _queueManagement;
    private readonly ILogger<Aria2BackgroundWorker> _logger;

    public Aria2BackgroundWorker(
        Aria2Manager manager,
        IServiceProvider serviceProvider,
        IQueueManagement queueManagement,
        ILogger<Aria2BackgroundWorker> logger)
    {
        _manager = manager;
        _serviceProvider = serviceProvider;
        _queueManagement = queueManagement;
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

                var receiveTask = ReceiveMessagesAsync(clientWebSocket, stoppingToken);
                var sendTask = SendQueuedRequestsAsync(clientWebSocket, stoppingToken);

                await Task.WhenAll(receiveTask, sendTask);

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
    /// 从队列中获取请求并发送到 Aria2
    /// </summary>
    private async Task SendQueuedRequestsAsync(ClientWebSocket ws, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
        {
            try
            {
                var items = _queueManagement.GetQueueValue<Aria2RequestDto>("Aria2RequestQueue");
                if (items != null && items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        string json = JsonSerializer.Serialize(item);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        if (ws.State != WebSocketState.Open) break;
                        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
                        _logger.LogDebug("已发送 Aria2 请求: {Method}, Id: {Id}", item.Method, item.Id);
                    }
                    _queueManagement.ClearQueue("Aria2RequestQueue");
                }

                await Task.Delay(500, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送 Aria2 请求异常");
            }
        }
    }

    /// <summary>
    /// 处理接收到的 WebSocket 消息
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

            ResponseBase? dto;
            if (message.Contains("\"id\":") && !message.Contains("\"error\":"))
            {
                // 请求响应（如 TellStatus 响应）
                dto = JsonSerializer.Deserialize<TellStatusResponse>(message);
            }
            else if (message.Contains("\"method\":"))
            {
                // 通知事件
                var notification = JsonSerializer.Deserialize<Aria2Notification>(message);
                dto = notification;

                // 下载完成时更新 RSS 订阅下载记录
                if (notification != null && notification.Method == Aria2Consts.OnDownloadComplete)
                {
                    await UpdateDownloadRecordStatusAsync(notification);
                }
            }
            else
            {
                return;
            }

            if (dto != null)
            {
                var requests = await _manager.ProcessResponseAsync(dto);
                if (requests != null && requests.Count > 0)
                {
                    // 将 Aria2Request 转换为 Aria2RequestDto 并加入发送队列
                    var dtos = requests.Select(r => new Aria2RequestDto
                    {
                        JSONRPC = r.JSONRPC,
                        Method = r.Method,
                        Id = r.Id,
                        Params = new List<object>(r.Params)
                    }).ToList();

                    _queueManagement.AddQueueValue("Aria2RequestQueue", dtos);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Aria2 消息异常");
        }
    }

    /// <summary>
    /// 下载完成时更新 RSS 订阅下载记录状态
    /// </summary>
    private async Task UpdateDownloadRecordStatusAsync(Aria2Notification notification)
    {
        if (notification.Params == null || notification.Params.Count == 0) return;

        string gid = notification.Params[0].GID;
        if (string.IsNullOrEmpty(gid)) return;

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISqlSugarRepository<RssSubscriptionDownload, long>>();

        var downloads = await repository.GetListAsync(d => d.Aria2Gid == gid);
        foreach (var download in downloads)
        {
            download.DownloadStatus = 2;
            download.DownloadCompleteTime = DateTime.Now;
            await repository.UpdateAsync(download);
        }

        if (downloads.Count > 0)
        {
            _logger.LogInformation("更新订阅下载记录状态: {Gid} -> 完成，共 {Count} 条", gid, downloads.Count);
        }
    }
}
