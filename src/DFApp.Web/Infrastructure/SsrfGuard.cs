using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 出站请求 SSRF 防护：
/// - 仅允许 http/https，禁止 URL 携带 userinfo
/// - 禁止访问回环/内网/链路本地（含云元数据 169.254.169.254）/唯一本地地址
/// - 校验发生在建立连接时（对实际连接 IP 判定），防 DNS 重绑定绕过
/// - 重定向由调用方逐跳校验后手动跟随
/// </summary>
public static class SsrfGuard
{
    private static readonly ConcurrentDictionary<string, (DateTime ExpiresAt, bool Blocked)> DnsCache = new();
    private static readonly TimeSpan DnsCacheTtl = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 校验用户提供的 URL 并返回解析结果，不满足则抛出异常
    /// </summary>
    public static Uri EnsureAllowed(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            throw new InvalidOperationException("仅允许 http/https 地址");
        }

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException("地址中不允许携带用户凭据");
        }

        if (!string.IsNullOrEmpty(uri.Host))
        {
            if (uri.HostNameType == UriHostNameType.IPv4 ||
                uri.HostNameType == UriHostNameType.IPv6)
            {
                if (IPAddress.TryParse(uri.Host, out var literal) && IsBlockedAddress(literal))
                {
                    throw new InvalidOperationException($"禁止访问内网地址: {uri.Host}");
                }
            }
            else if (ResolveBlocked(uri.Host))
            {
                throw new InvalidOperationException($"目标主机解析到内网地址，已拒绝: {uri.Host}");
            }
        }

        return uri;
    }

    /// <summary>
    /// 创建受防护的 HTTP 处理器：关闭自动重定向（由 SafeGetAsync 逐跳校验后跟随），
    /// 连接建立时对实际 IP 做最终判定。
    /// </summary>
    public static SocketsHttpHandler CreateGuardedHandler()
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            ConnectCallback = (context, ct) => ConnectValidatedAsync(context, ct)
        };
    }

    /// <summary>
    /// 逐跳校验并跟随重定向的 GET；非重定向响应原样返回（含 4xx/5xx，由调用方决定处理）
    /// </summary>
    public static async Task<HttpResponseMessage> SafeGetAsync(HttpClient client, Uri uri, int maxRedirects = 5)
    {
        var current = uri;
        for (var hop = 0; ; hop++)
        {
            var response = await client.GetAsync(current, HttpCompletionOption.ResponseHeadersRead);
            if ((int)response.StatusCode is < 300 or >= 400)
            {
                return response;
            }

            var location = response.Headers.Location;
            if (location is null || hop >= maxRedirects)
            {
                return response;
            }

            response.Dispose();
            current = location.IsAbsoluteUri ? location : new Uri(current, location);
            current = EnsureAllowed(current.ToString());
        }
    }

    /// <summary>
    /// 逐跳校验并跟随重定向的 GET，返回响应体字节
    /// </summary>
    public static async Task<byte[]> SafeGetByteArrayAsync(HttpClient client, string url, int maxRedirects = 5)
    {
        var uri = EnsureAllowed(url);
        using var response = await SafeGetAsync(client, uri, maxRedirects);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// 建立连接前对解析出的地址做最终判定；DNS 结果短暂缓存，避免双重解析开销
    /// </summary>
    private static async ValueTask<Stream> ConnectValidatedAsync(
        SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var endpoint = context.DnsEndPoint;
        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(endpoint.Host, cancellationToken);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException($"目标主机无法解析: {endpoint.Host}", ex);
        }

        var candidates = addresses
            .Where(a => !IsBlockedAddress(a))
            .Distinct()
            .ToArray();

        if (candidates.Length == 0)
        {
            throw new InvalidOperationException($"禁止访问内网地址: {endpoint.Host}");
        }

        Exception? lastError = null;
        foreach (var address in candidates)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(address, endpoint.Port, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception ex)
            {
                lastError = ex;
                socket.Dispose();
            }
        }

        throw lastError ?? new InvalidOperationException($"无法连接目标主机: {endpoint.Host}");
    }

    private static bool ResolveBlocked(string host)
    {
        var now = DateTime.UtcNow;
        if (DnsCache.TryGetValue(host, out var cached) && cached.ExpiresAt > now)
        {
            return cached.Blocked;
        }

        var blocked = false;
        try
        {
            var addresses = Dns.GetHostAddresses(host);
            blocked = addresses.Length == 0 || addresses.Any(IsBlockedAddress);
        }
        catch (SocketException)
        {
            blocked = true;
        }

        DnsCache[host] = (now.Add(DnsCacheTtl), blocked);
        return blocked;
    }

    /// <summary>
    /// 内网/保留地址判定（回环整段、私网、链路本地、唯一本地、CGNAT、未指定）
    /// </summary>
    public static bool IsBlockedAddress(IPAddress address)
    {
        var ip = address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var v6Bytes = ip.GetAddressBytes();
            return ip.IsIPv6LinkLocal ||
                   ip.IsIPv6SiteLocal ||
                   ip.Equals(IPAddress.IPv6Loopback) ||
                   ip.Equals(IPAddress.IPv6Any) ||
                   (v6Bytes[0] & 0xFE) == 0xFC;   // fc00::/7 唯一本地地址（ULA）
        }

        if (ip.AddressFamily != AddressFamily.InterNetwork)
        {
            return true;
        }

        var bytes = ip.GetAddressBytes();
        return bytes[0] == 0 ||                      // 0.0.0.0/8
               bytes[0] == 10 ||                     // 10.0.0.0/8
               bytes[0] == 127 ||                    // 127.0.0.0/8 回环整段（Linux 上全部路由到本机）
               (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) || // 172.16.0.0/12
               (bytes[0] == 192 && bytes[1] == 168) || // 192.168.0.0/16
               (bytes[0] == 169 && bytes[1] == 254) || // 169.254.0.0/16 链路本地（含云元数据）
               (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127); // 100.64.0.0/10 CGNAT
    }
}
