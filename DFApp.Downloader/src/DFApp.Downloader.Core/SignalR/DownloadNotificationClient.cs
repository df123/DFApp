using System.Net.Http.Json;
using System.Text.Json;
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

    /// <summary>最近一次连接/登录失败的错误信息，连接成功时为 null</summary>
    public string? LastConnectionError { get; private set; }

    /// <summary>登录后的 JWT 访问令牌，未登录时为 null（供调后端补漏 API 用）</summary>
    public string? AccessToken => _jwtToken;

    // 后端返回 camelCase，需匹配命名策略才能正确反序列化 accessToken 等字段
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        var loginUrl = $"{settings.DfAppUrl}/api/app/account/login";
        var request = new LoginRequest
        {
            Username = settings.DfAppUsername,
            Password = settings.DfAppPassword
        };

        try
        {
            var response = await httpClient.PostAsJsonAsync(loginUrl, request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // 优先使用响应体中的业务错误消息（如"用户名或密码错误""登录尝试次数过多"）
                LastConnectionError = TryExtractMessage(body) ?? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                throw new InvalidOperationException($"登录失败: {LastConnectionError}");
            }

            var result = JsonSerializer.Deserialize<LoginResponse>(body, JsonOptions);
            _jwtToken = result?.Data?.AccessToken;

            if (string.IsNullOrEmpty(_jwtToken))
            {
                LastConnectionError = "登录失败，未获取到 AccessToken";
                throw new InvalidOperationException(LastConnectionError);
            }

            LastConnectionError = null;
            _logger.LogInformation("登录 DFApp 成功");
        }
        catch (HttpRequestException ex)
        {
            // 网络层错误（DNS、连接拒绝、超时等）
            LastConnectionError = $"网络错误: {ex.Message}";
            throw;
        }
    }

    /// <summary>
    /// 从后端响应体中提取业务错误消息（兼容 { message: "..." } 结构）
    /// </summary>
    private static string? TryExtractMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var msgEl))
            {
                return msgEl.GetString();
            }
        }
        catch
        {
            // 响应体非 JSON，忽略
        }

        return null;
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

        try
        {
            await _connection.StartAsync();
            await _connection.SendAsync("JoinDownloadGroup");

            LastConnectionError = null;
            _logger.LogInformation("SignalR 连接已建立");
            OnConnectionChanged?.Invoke(true);
        }
        catch (Exception ex)
        {
            // SignalR negotiate/连接失败（常见于反向代理未正确转发 /hubs 路径，返回了 HTML）
            LastConnectionError = $"SignalR 连接失败: {ex.Message}";
            _logger.LogError(ex, "SignalR 连接失败");
            throw;
        }
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
