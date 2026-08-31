namespace DFApp.Downloader.Core.Configuration;

/// <summary>
/// 下载器配置模型
/// </summary>
public class DownloaderSettings
{
    /// <summary>DFApp 后端地址</summary>
    public string DfAppUrl { get; set; } = "http://localhost:44369";

    /// <summary>DFApp 登录用户名</summary>
    public string DfAppUsername { get; set; } = string.Empty;

    /// <summary>DFApp 登录密码</summary>
    public string DfAppPassword { get; set; } = string.Empty;

    /// <summary>Apache Basic Auth 用户名</summary>
    public string ApacheUsername { get; set; } = string.Empty;

    /// <summary>Apache Basic Auth 密码</summary>
    public string ApachePassword { get; set; } = string.Empty;

    /// <summary>文件下载保存路径</summary>
    public string DownloadPath { get; set; } = @"%USERPROFILE%\Downloads\DFApp";

    /// <summary>最大并发下载数</summary>
    public int MaxConcurrentDownloads { get; set; } = 3;

    /// <summary>每个文件最大分片数</summary>
    public int MaxSegmentsPerFile { get; set; } = 4;

    /// <summary>分片大小（字节）</summary>
    public long SegmentSize { get; set; } = 4 * 1024 * 1024; // 4MB

    /// <summary>Web 管理界面端口</summary>
    public int WebServerPort { get; set; } = 9550;

    /// <summary>
    /// Web 管理界面绑定地址，默认仅回环（127.0.0.1）。
    /// 需要局域网访问时显式配置为 0.0.0.0，并务必同时配置管理令牌
    /// </summary>
    public string WebServerBind { get; set; } = "127.0.0.1";

    /// <summary>
    /// 管理令牌：非回环来源的请求必须携带匹配的 X-Management-Token 请求头；
    /// 为空时非回环请求一律拒绝（fail-closed），仅本机可访问
    /// </summary>
    public string ManagementToken { get; set; } = string.Empty;

    /// <summary>是否开机自启</summary>
    public bool AutoStart { get; set; } = false;
}
