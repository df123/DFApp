using System.Net.Http.Json;
using DFApp.Downloader.Core.Configuration;
using DFApp.Downloader.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace DFApp.Downloader.Core.SignalR;

/// <summary>
/// DFApp 下载通知 SignalR 客户端
/// </summary>
public class DownloadNotificationClient : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly ILogger<DownloadNotificationClient> _logger;
    private string? _jwtToken;

    /// <summary>下载完成事件</summary>
    public event Action<DownloadNotification>? OnDownloadCompleted;

    /// <summary>连接状态变化事件</summary>
    public event Action<bool>? OnConnectionChanged;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public DownloadNotificationClient(ILogger<DownloadNotificationClient> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 登录 DFApp 获取 JWT Token
    /// </summary>
    public async Task LoginAsync(DownloaderSettings settings, HttpClient httpClient)
    {
        var loginUrl = $"{settings.DfAppUrl}/api/account/login";
        var request = new LoginRequest
        {
            UserName = settings.DfAppUsername,
            Password = settings.DfAppPassword
        };

        var response = await httpClient.PostAsJsonAsync(loginUrl, request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _jwtToken = result?.AccessToken;

        if (string.IsNullOrEmpty(_jwtToken))
        {
            throw new InvalidOperationException("登录失败，未获取到 AccessToken");
        }

        _logger.LogInformation("登录 DFApp 成功");
    }

    /// <summary>
    /// 启动 SignalR 连接
    /// </summary>
    public async Task StartAsync(DownloaderSettings settings)
    {
        if (string.IsNullOrEmpty(_jwtToken))
        {
            throw new InvalidOperationException("请先调用 LoginAsync 获取 Token");
        }

        _connection = new HubConnectionBuilder()
            .WithUrl($"{settings.DfAppUrl}/hubs/download-notification", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
            })
            .WithAutomaticReconnect(new[] {
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        _connection.On<MediaDownloadNotification>("DownloadCompleted", notification =>
        {
            _logger.LogInformation("收到下载通知: {FileName}", notification.FileName);
            OnDownloadCompleted?.Invoke(notification);
        });

        _connection.Reconnected += async _ =>
        {
            _logger.LogInformation("SignalR 重新连接成功");
            await _connection.SendAsync("JoinDownloadGroup");
            OnConnectionChanged?.Invoke(true);
        };

        _connection.Closed += _ =>
        {
            _logger.LogWarning("SignalR 连接关闭");
            OnConnectionChanged?.Invoke(false);
            return Task.CompletedTask;
        };

        await _connection.StartAsync();
        await _connection.SendAsync("JoinDownloadGroup");

        _logger.LogInformation("SignalR 连接已建立");
        OnConnectionChanged?.Invoke(true);
    }

    /// <summary>
    /// 停止连接
    /// </summary>
    public async Task StopAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
