using System;

namespace DFApp.Downloader.Core.Configuration;

/// <summary>
/// 敏感配置解析：支持通过环境变量注入机密（DFAPP_DOWNLOADER_DFAPP_PASSWORD、
/// DFAPP_DOWNLOADER_APACHE_PASSWORD、DFAPP_DOWNLOADER_MANAGEMENT_TOKEN），
/// 设置了环境变量时优先于 settings.json，避免明文密码落盘
/// </summary>
public static class SettingsSecrets
{
    /// <summary>
    /// 解析机密值：环境变量优先，其次配置文件值
    /// </summary>
    /// <param name="envName">环境变量名</param>
    /// <param name="fileValue">settings.json 中的值</param>
    public static string Resolve(string envName, string? fileValue)
    {
        var envValue = Environment.GetEnvironmentVariable(envName);
        return string.IsNullOrWhiteSpace(envValue) ? (fileValue ?? string.Empty) : envValue;
    }

    /// <summary>DFApp 登录密码</summary>
    public static string DfAppPassword(DownloaderSettings settings)
        => Resolve("DFAPP_DOWNLOADER_DFAPP_PASSWORD", settings.DfAppPassword);

    /// <summary>Apache Basic Auth 密码</summary>
    public static string ApachePassword(DownloaderSettings settings)
        => Resolve("DFAPP_DOWNLOADER_APACHE_PASSWORD", settings.ApachePassword);

    /// <summary>管理令牌</summary>
    public static string ManagementToken(DownloaderSettings settings)
        => Resolve("DFAPP_DOWNLOADER_MANAGEMENT_TOKEN", settings.ManagementToken);
}
