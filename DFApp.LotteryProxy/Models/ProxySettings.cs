namespace DFApp.LotteryProxy.Models;

/// <summary>
/// 代理服务配置
/// </summary>
public class ProxySettings
{
    /// <summary>
    /// 允许访问的IP地址列表
    /// </summary>
    public List<string> AllowedIPs { get; set; } = new();

    /// <summary>
    /// 共享密钥：非空时所有请求（/api/health 除外）必须携带匹配的 X-Proxy-Token 请求头。
    /// 服务暴露公网时必配，为空则不校验（仅 IP 白名单）。
    /// </summary>
    public string ProxyToken { get; set; } = string.Empty;

    /// <summary>
    /// 目标基础URL
    /// </summary>
    public string TargetBaseUrl { get; set; } = "https://www.cwl.gov.cn";

    /// <summary>
    /// 请求超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// 重试延迟时间（秒）
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
}