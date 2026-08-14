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
    private DownloaderSettings? _settings;
    private HttpClient? _httpClient;
    private volatile bool _isStopping;
    private int _reconnectRunning;
    private readonly CancellationTokenSource _stopCts = new();

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
        _settings = settings;
        _httpClient = httpClient;
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
                // SignalR 内部自建 HttpClient，不受 DI 的 IHttpClientFactory 代理配置影响，这里显式禁用代理
                options.HttpMessageHandlerFactory = handler =>
                {
                    if (handler is HttpClientHandler c)
                    {
                        c.UseProxy = false;
                    }
                    return handler;
                };
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

        _connection.Reconnecting += async _ =>
        {
            // JWT 有效期 60 分钟，重连前先重新登录刷新 token，避免过期 token 导致重连 401 失败
            _logger.LogWarning("SignalR 连接断开，正在刷新 Token 并重连");
            await RefreshTokenSafelyAsync();
        };

        _connection.Reconnected += async _ =>
        {
            _logger.LogInformation("SignalR 重新连接成功");
            await _connection.SendAsync("JoinDownloadGroup");
            OnConnectionChanged?.Invoke(true);
        };

        _connection.Closed += _exception =>
        {
            _logger.LogWarning("SignalR 连接关闭");
            OnConnectionChanged?.Invoke(false);
            // 内置自动重连（5 次）耗尽后进入 Closed，这里接管手动无限重连
            if (!_isStopping)
            {
                _ = ReconnectLoopAsync();
            }
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
    /// 重新登录刷新 JWT，失败时记录日志但不抛出（保持旧 token 继续重试）
    /// </summary>
    private async Task RefreshTokenSafelyAsync()
    {
        try
        {
            if (_settings != null && _httpClient != null)
            {
                await LoginAsync(_settings, _httpClient);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "刷新 Token 失败，将沿用旧 Token 重试");
        }
    }

    /// <summary>
    /// 手动无限重连：每次重连前刷新 token，成功后加入下载通知组。
    /// 带指数退避（5s 起，上限 60s），停止时退出。
    /// </summary>
    private async Task ReconnectLoopAsync()
    {
        if (Interlocked.CompareExchange(ref _reconnectRunning, 1, 0) != 0)
        {
            return;
        }

        var delay = TimeSpan.FromSeconds(5);
        try
        {
            while (!_isStopping)
            {
                try
                {
                    await RefreshTokenSafelyAsync();
                    await _connection!.StartAsync();
                    await _connection.SendAsync("JoinDownloadGroup");
                    _logger.LogInformation("SignalR 重新连接成功");
                    OnConnectionChanged?.Invoke(true);
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SignalR 重连失败，{DelaySeconds} 秒后重试", delay.TotalSeconds);
                }

                try
                {
                    await Task.Delay(delay, _stopCts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (delay < TimeSpan.FromSeconds(60))
                {
                    delay += delay;
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref _reconnectRunning, 0);
        }
    }

    /// <summary>
    /// 停止连接
    /// </summary>
    public async Task StopAsync()
    {
        _isStopping = true;
        _stopCts.Cancel();
        if (_connection != null)
        {
            await _connection.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isStopping = true;
        _stopCts.Cancel();
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
