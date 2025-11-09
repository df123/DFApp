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
        
        _logger.LogDebug("客户端IP: {ClientIP}", clientIp);

        if (!IsIpAllowed(clientIp))
        {
            _logger.LogWarning("未授权的IP访问尝试: {ClientIP}", clientIp);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("403 Forbidden: IP地址不在允许列表中");
            return;
        }

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // 尝试从各种头部获取真实IP
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        
        if (!string.IsNullOrEmpty(ip))
        {
            // X-Forwarded-For可能包含多个IP，取第一个
            var ips = ip.Split(',');
            if (ips.Length > 0)
            {
                ip = ips[0].Trim();
            }
        }
        
        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        
        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Connection.RemoteIpAddress?.ToString();
        }

        // 如果是IPv6回环地址，转换为IPv4
        if (ip == "::1")
        {
            ip = "127.0.0.1";
        }

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