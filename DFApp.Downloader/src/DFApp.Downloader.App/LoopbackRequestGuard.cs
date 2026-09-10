using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DFApp.Downloader.App;

/// <summary>
/// 回环请求同源校验：
/// 回环地址零配置放行仅面向本机同源调用，跨站 Origin/Referer 与非本地 Host（DNS rebinding）一律拒绝
/// </summary>
public static class LoopbackRequestGuard
{
    /// <summary>
    /// 允许的本地主机名（不含端口，比较时忽略端口与大小写）
    /// </summary>
    private static readonly HashSet<string> LocalHostNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "127.0.0.1",
        "[::1]",
        "::1"
    };

    /// <summary>
    /// 判断请求是否可按回环放行：
    /// Host 必须为本地主机名；携带 Origin/Referer 时其主机部分也必须是本地主机名
    /// </summary>
    public static bool IsSameOriginLocalRequest(HttpRequest request)
    {
        if (!IsLocalHostValue(request.Host.Host))
        {
            return false;
        }

        var origin = request.Headers.Origin.ToString();
        if (!string.IsNullOrWhiteSpace(origin) && !OriginHostIsLocal(origin))
        {
            return false;
        }

        var referer = request.Headers.Referer.ToString();
        if (string.IsNullOrWhiteSpace(origin) && !string.IsNullOrWhiteSpace(referer)
            && !OriginHostIsLocal(referer))
        {
            return false;
        }

        return true;
    }

    private static bool IsLocalHostValue(string? host)
    {
        return !string.IsNullOrWhiteSpace(host) && LocalHostNames.Contains(host.Trim());
    }

    /// <summary>
    /// 解析 Origin/Referer 值（可带 scheme 与路径）并判断主机部分是否为本地
    /// </summary>
    private static bool OriginHostIsLocal(string value)
    {
        try
        {
            var uri = new Uri(value, UriKind.Absolute);
            return IsLocalHostValue(uri.Host);
        }
        catch (UriFormatException)
        {
            // 非法头视为不可信
            return false;
        }
    }
}
