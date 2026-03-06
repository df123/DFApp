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

        _logger.LogInformation("客户端IP: {ClientIP}", clientIp);
        _logger.LogInformation("允许的IP列表: {AllowedIPs}", string.Join(", ", _proxySettings.AllowedIPs));
        _logger.LogInformation("允许的IP数量: {Count}", _proxySettings.AllowedIPs?.Count ?? 0);

        if (!IsIpAllowed(clientIp))
        {
            _logger.LogWarning("未授权的IP访问尝试: {ClientIP}", clientIp);
            _logger.LogWarning("请求路径: {Path}", context.Request.Path);
            _logger.LogWarning("请求方法: {Method}", context.Request.Method);
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
        _logger.LogInformation("RemoteIpAddress: {IP}", ip);

        // 如果是IPv6回环地址，转换为IPv4
        if (ip == "::1")
        {
            ip = "127.0.0.1";
            _logger.LogInformation("IPv6回环地址转换为IPv4: {IP}", ip);
        }
        // 如果是IPv4映射的IPv6地址（::ffff:x.x.x.x），提取IPv4部分
        else if (ip != null && ip.StartsWith("::ffff:"))
        {
            ip = ip.Substring(7); // 移除 "::ffff:" 前缀
            _logger.LogInformation("IPv6映射的IPv4地址转换为IPv4: {IP}", ip);
        }

        _logger.LogInformation("最终获取的客户端IP: {IP}", ip);
        return ip ?? "unknown";
    }

    private bool IsIpAllowed(string? clientIp)
    {
        if (string.IsNullOrEmpty(clientIp))
        {
            return false;
        }

        // 如果允许列表为空，则允许所有IP（开发环境）
        if (_proxySettings.AllowedIPs == null || _proxySettings.AllowedIPs.Count == 0)
        {
            _logger.LogDebug("IP白名单为空，允许所有IP访问");
            return true;
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