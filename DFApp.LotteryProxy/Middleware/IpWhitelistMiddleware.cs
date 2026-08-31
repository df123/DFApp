using DFApp.LotteryProxy.Models;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace DFApp.LotteryProxy.Middleware;

/// <summary>
/// IP白名单中间件
/// </summary>
public class IpWhitelistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistMiddleware> _logger;
    private readonly ProxySettings _proxySettings;

    public IpWhitelistMiddleware(
        RequestDelegate next,
        ILogger<IpWhitelistMiddleware> logger,
        ProxySettings proxySettings)
    {
        _next = next;
        _logger = logger;
        _proxySettings = proxySettings;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);

        // 令牌校验（第二道门）：与 IP 白名单同时生效；默认 fail-closed——
        // 未显式豁免时，ProxyToken 为空同样拒绝，避免部署机漏配密钥导致代理裸奔
        var tokenCheckEnabled = !_proxySettings.AllowAnonymous || !string.IsNullOrEmpty(_proxySettings.ProxyToken);
        if (tokenCheckEnabled && !context.Request.Path.StartsWithSegments("/api/health"))
        {
            if (string.IsNullOrEmpty(_proxySettings.ProxyToken))
            {
                _logger.LogError("ProxyToken 未配置且未显式豁免，拒绝请求: {ClientIP} {Method} {Path}",
                    clientIp, context.Request.Method, context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                await context.Response.WriteAsync("503 Service Unavailable: 代理令牌未配置");
                return;
            }

            var token = context.Request.Headers["X-Proxy-Token"].ToString();
            // 固定时间比较，避免逐字节短路造成的时序侧信道
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token ?? string.Empty);
            var expectedBytes = System.Text.Encoding.UTF8.GetBytes(_proxySettings.ProxyToken);
            if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(tokenBytes, expectedBytes))
            {
                _logger.LogWarning("令牌校验失败: {ClientIP} {Method} {Path}", clientIp, context.Request.Method, context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("401 Unauthorized: 无效的 X-Proxy-Token");
                return;
            }
        }

        _logger.LogDebug("客户端IP: {ClientIP}，允许列表: [{AllowedIPs}]", clientIp, string.Join(", ", _proxySettings.AllowedIPs));

        if (!IsIpAllowed(clientIp))
        {
            _logger.LogWarning("未授权的IP访问: {ClientIP} {Method} {Path}", clientIp, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("403 Forbidden: IP地址不在允许列表中");
            return;
        }

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // 仅使用 RemoteIpAddress，因为它来自TCP连接，无法被客户端伪造
        // 不使用 X-Forwarded-For 和 X-Real-IP 头部，因为它们可以被伪造
        var ip = context.Connection.RemoteIpAddress?.ToString();

        // 如果是IPv6回环地址，转换为IPv4
        if (ip == "::1")
        {
            ip = "127.0.0.1";
        }
        // 如果是IPv4映射的IPv6地址（::ffff:x.x.x.x），提取IPv4部分
        else if (ip != null && ip.StartsWith("::ffff:"))
        {
            ip = ip.Substring(7); // 移除 "::ffff:" 前缀
        }

        return ip ?? "unknown";
    }

    private bool IsIpAllowed(string? clientIp)
    {
        if (string.IsNullOrEmpty(clientIp))
        {
            return false;
        }

        // 默认允许本地访问（127.0.0.1 和 ::1）
        if (clientIp == "127.0.0.1" || clientIp == "::1")
        {
            return true;
        }

        // 如果允许列表为空，则拒绝所有IP（生产环境）
        if (_proxySettings.AllowedIPs == null || _proxySettings.AllowedIPs.Count == 0)
        {
            _logger.LogDebug("IP白名单为空，拒绝所有IP访问（除本地外）");
            return false;
        }

        return _proxySettings.AllowedIPs.Contains(clientIp);
    }
}

/// <summary>
/// IP白名单中间件扩展方法
/// </summary>
public static class IpWhitelistMiddlewareExtensions
{
    public static IApplicationBuilder UseIpWhitelist(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IpWhitelistMiddleware>();
    }
}