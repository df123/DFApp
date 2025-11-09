using DFApp.LotteryProxy.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DFApp.LotteryProxy.Services;

/// <summary>
/// 彩票API代理服务
/// </summary>
public class LotteryProxyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LotteryProxyService> _logger;
    private readonly ProxySettings _proxySettings;

    public LotteryProxyService(
        IHttpClientFactory httpClientFactory,
        ILogger<LotteryProxyService> logger,
        ProxySettings proxySettings)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _proxySettings = proxySettings;
    }

    /// <summary>
    /// 代理彩票数据请求
    /// </summary>
    /// <param name="queryString">查询字符串</param>
    /// <returns>响应内容</returns>
    public async Task<IResult> ProxyRequestAsync(string queryString)
    {
        var targetUrl = $"/cwl_admin/front/cwlkj/search/kjxx/findDrawNotice?{queryString}";

        _logger.LogInformation("代理请求到目标URL: {TargetUrl}", targetUrl);

        using var client = _httpClientFactory.CreateClient();

        // 设置BaseAddress以确保HttpClient可以处理相对URL
        client.BaseAddress = new Uri(_proxySettings.TargetBaseUrl);

        // 配置请求头，模拟真实浏览器
        ConfigureClientHeaders(client);

        var maxRetries = _proxySettings.RetryCount;
        var retryDelay = TimeSpan.FromSeconds(_proxySettings.RetryDelaySeconds);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("发送请求 (尝试 {Attempt}/{MaxRetries})", attempt, maxRetries);

                using var response = await client.GetAsync(targetUrl);

                _logger.LogInformation("收到响应 - 状态码: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    // 使用UTF-8编码读取响应内容
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("成功获取响应内容，长度: {Length} 字符", content.Length);

                    // 记录响应内容的前500字符用于调试
                    if (content.Length > 500)
                    {
                        _logger.LogDebug("响应内容前500字符: {Content}", content.Substring(0, 500));
                    }
                    else
                    {
                        _logger.LogDebug("响应内容: {Content}", content);
                    }

                    // 检查响应内容是否为有效的JSON
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogWarning("响应内容为空，返回错误");
                        return Results.Problem(
                            detail: "目标服务器返回空响应",
                            statusCode: (int)System.Net.HttpStatusCode.BadGateway,
                            title: "代理请求失败"
                        );
                    }

                    // 检查响应内容是否为HTML（错误页面）
                    if (content.StartsWith("<!DOCTYPE html>") || content.StartsWith("<html"))
                    {
                        _logger.LogWarning("目标服务器返回HTML页面，可能是错误页面");
                        return Results.Problem(
                            detail: "目标服务器返回错误页面，可能是访问被拒绝",
                            statusCode: (int)System.Net.HttpStatusCode.BadGateway,
                            title: "代理请求失败"
                        );
                    }

                    // 设置响应头
                    var headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json; charset=utf-8" },
                        { "Access-Control-Allow-Origin", "*" },
                        { "Access-Control-Allow-Methods", "GET, POST, OPTIONS" },
                        { "Access-Control-Allow-Headers", "Content-Type, Authorization" }
                    };

                    return Results.Content(content, "application/json");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("目标服务器返回403 Forbidden，可能被反爬虫机制拦截");
                    return Results.Problem(
                        detail: "目标服务器拒绝访问，可能被反爬虫机制拦截",
                        statusCode: (int)System.Net.HttpStatusCode.BadGateway,
                        title: "代理请求失败"
                    );
                }
                else
                {
                    _logger.LogWarning("目标服务器返回错误状态码: {StatusCode}", response.StatusCode);

                    if (attempt == maxRetries)
                    {
                        return Results.Problem(
                            detail: $"目标服务器返回错误: {response.StatusCode}",
                            statusCode: (int)System.Net.HttpStatusCode.BadGateway,
                            title: "代理请求失败"
                        );
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP请求异常 (尝试 {Attempt}/{MaxRetries})", attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return Results.Problem(
                        detail: $"HTTP请求异常: {ex.Message}",
                        statusCode: (int)System.Net.HttpStatusCode.BadGateway,
                        title: "代理请求失败"
                    );
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "请求超时 (尝试 {Attempt}/{MaxRetries})", attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return Results.Problem(
                        detail: $"请求超时: {ex.Message}",
                        statusCode: (int)System.Net.HttpStatusCode.GatewayTimeout,
                        title: "代理请求超时"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "未知异常 (尝试 {Attempt}/{MaxRetries})", attempt, maxRetries);

                if (attempt == maxRetries)
                {
                    return Results.Problem(
                        detail: $"未知异常: {ex.Message}",
                        statusCode: (int)System.Net.HttpStatusCode.InternalServerError,
                        title: "代理请求失败"
                    );
                }
            }

            // 如果不是最后一次尝试，等待后重试
            if (attempt < maxRetries)
            {
                _logger.LogInformation("等待 {DelaySeconds} 秒后重试...", retryDelay.TotalSeconds);
                await Task.Delay(retryDelay);
            }
        }

        return Results.Problem(
            detail: "未知错误",
            statusCode: (int)System.Net.HttpStatusCode.InternalServerError,
            title: "代理请求失败"
        );
    }

    /// <summary>
    /// 配置HTTP客户端请求头
    /// </summary>
    /// <param name="client">HTTP客户端</param>
    private void ConfigureClientHeaders(HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(_proxySettings.TimeoutSeconds);

        // 清除默认请求头
        client.DefaultRequestHeaders.Clear();

        client.DefaultRequestHeaders.Add("Host", "www.cwl.gov.cn");
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:102.0) Gecko/20100101 Firefox/102.0");
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");

        _logger.LogDebug("已配置HTTP客户端请求头");
    }
}