using System;
using System.Collections.Generic;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Services.Aria2;

/// <summary>
/// Aria2 下载请求防护：出站 URL 经 SSRF 校验，保存路径限制为下载目录内的相对子目录，
/// 自定义 RPC 选项仅放行白名单（防止 dir/out/allow-overwrite 等选项实现任意路径写入）
/// </summary>
public static class Aria2OptionGuard
{
    /// <summary>
    /// 允许透传给 aria2 RPC 的用户自定义选项白名单（select-file 用于种子内文件选择）
    /// </summary>
    private static readonly HashSet<string> AllowedOptionKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "select-file"
    };

    /// <summary>
    /// 校验下载 URL 列表：磁力链接走 DHT 不做 HTTP 直连，跳过校验，其余 URL 必须通过 SSRF 防护
    /// </summary>
    public static void EnsureUrlsAllowed(IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            if (url.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            SsrfGuard.EnsureAllowed(url);
        }
    }

    /// <summary>
    /// 校验保存路径：仅允许下载目录内的相对子目录，禁止绝对路径、盘符前缀与上跳
    /// </summary>
    public static void EnsureSavePathAllowed(string? savePath)
    {
        if (string.IsNullOrWhiteSpace(savePath))
        {
            return;
        }

        if (savePath.IndexOfAny(new[] { ':', '\0' }) >= 0
            || savePath.Contains("..", StringComparison.Ordinal)
            || savePath.StartsWith("/", StringComparison.Ordinal)
            || savePath.StartsWith("\\", StringComparison.Ordinal))
        {
            throw new BusinessException("保存路径仅允许下载目录内的相对子目录");
        }
    }

    /// <summary>
    /// 构建安全的 aria2 下载选项：dir 限制为相对子目录，自定义选项仅保留白名单键
    /// </summary>
    public static Dictionary<string, object>? BuildSafeOptions(
        string? savePath,
        Dictionary<string, object>? customOptions)
    {
        var options = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(savePath))
        {
            EnsureSavePathAllowed(savePath);
            options["dir"] = savePath;
        }

        if (customOptions != null)
        {
            foreach (var kvp in customOptions)
            {
                if (AllowedOptionKeys.Contains(kvp.Key))
                {
                    options[kvp.Key] = kvp.Value;
                }
            }
        }

        return options.Count > 0 ? options : null;
    }
}
