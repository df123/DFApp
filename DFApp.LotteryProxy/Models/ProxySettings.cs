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
    /// 共享密钥：所有请求（/api/health 除外）必须携带匹配的 X-Proxy-Token 请求头。
    /// </summary>
    public string ProxyToken { get; set; } = string.Empty;

    /// <summary>
    /// 显式豁免令牌校验（仅限本地调试使用）。
    /// 默认 false 时 ProxyToken 为空将直接拒绝非 health 请求（fail-closed），
    /// 避免部署机忘记注入密钥导致代理裸奔。
    /// </summary>
    public bool AllowAnonymous { get; set; } = false;

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