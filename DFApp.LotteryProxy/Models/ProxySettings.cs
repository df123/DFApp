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