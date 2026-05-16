using System.Collections.Generic;

namespace DFApp.Aria2;

/// <summary>
/// Aria2 全局统计
/// </summary>
public class Aria2GlobalStatDto
{
    /// <summary>
    /// 下载速度
    /// </summary>
    public string DownloadSpeed { get; set; } = string.Empty;

    /// <summary>
    /// 上传速度
    /// </summary>
    public string UploadSpeed { get; set; } = string.Empty;

    /// <summary>
    /// 活跃任务数
    /// </summary>
    public string ActiveCount { get; set; } = string.Empty;

    /// <summary>
    /// 等待任务数
    /// </summary>
    public string WaitingCount { get; set; } = string.Empty;

    /// <summary>
    /// 停止任务数
    /// </summary>
    public string StoppedCount { get; set; } = string.Empty;

    /// <summary>
    /// 停止任务总数
    /// </summary>
    public string StoppedTotal { get; set; } = string.Empty;
}

/// <summary>
/// Aria2 任务信息
/// </summary>
public class Aria2TaskDto
{
    /// <summary>
    /// GID
    /// </summary>
    public string Gid { get; set; } = string.Empty;

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 总长度
    /// </summary>
    public long TotalLength { get; set; }

    /// <summary>
    /// 已完成长度
    /// </summary>
    public long CompletedLength { get; set; }

    /// <summary>
    /// 下载速度
    /// </summary>
    public long DownloadSpeed { get; set; }

    /// <summary>
    /// 上传速度
    /// </summary>
    public long UploadSpeed { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 文件列表
    /// </summary>
    public List<Aria2FileDto>? Files { get; set; }

    /// <summary>
    /// 下载目录
    /// </summary>
    public string? Dir { get; set; }

    /// <summary>
    /// 连接数
    /// </summary>
    public int? Connections { get; set; }

    /// <summary>
    /// 分享率
    /// </summary>
    public decimal ShareRatio { get; set; }

    /// <summary>
    /// 已上传字节数
    /// </summary>
    public long UploadedLength { get; set; }

    /// <summary>
    /// Peer 信息列表
    /// </summary>
    public List<Aria2PeerDto>? Peers { get; set; }

    /// <summary>
    /// 种子文件名
    /// </summary>
    public string? BtName { get; set; }
}

/// <summary>
/// Aria2 文件信息
/// </summary>
public class Aria2FileDto
{
    /// <summary>
    /// 索引
    /// </summary>
    public string Index { get; set; } = string.Empty;

    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 长度
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// 已完成长度
    /// </summary>
    public long CompletedLength { get; set; }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// URI 列表
    /// </summary>
    public List<Aria2UriDto>? Uris { get; set; }
}

/// <summary>
/// Aria2 URI 信息
/// </summary>
public class Aria2UriDto
{
    /// <summary>
    /// URI
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Aria2 Peer 信息（BitTorrent 对等连接）
/// </summary>
public class Aria2PeerDto
{
    /// <summary>
    /// Peer ID
    /// </summary>
    public string PeerId { get; set; } = string.Empty;

    /// <summary>
    /// IP 地址
    /// </summary>
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 客户端标识
    /// </summary>
    public string? Client { get; set; }

    /// <summary>
    /// 该 Peer 正在下载（从我们这里）
    /// </summary>
    public bool AmChoking { get; set; }

    /// <summary>
    /// 该 Peer 被我们阻塞
    /// </summary>
    public bool PeerChoking { get; set; }

    /// <summary>
    /// 下载速度（字节/秒）
    /// </summary>
    public long DownloadSpeed { get; set; }

    /// <summary>
    /// 上传速度（字节/秒）
    /// </summary>
    public long UploadSpeed { get; set; }

    /// <summary>
    /// 完成进度（0-1）
    /// </summary>
    public decimal Progress { get; set; }

    /// <summary>
    /// Seeder 标记
    /// </summary>
    public bool Seeder { get; set; }

    /// <summary>
    /// 国家（通过 IP 查询获取）
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 城市（通过 IP 查询获取）
    /// </summary>
    public string? City { get; set; }
}

/// <summary>
/// 任务详情（包含完整信息）
/// </summary>
public class Aria2TaskDetailDto
{
    /// <summary>
    /// GID
    /// </summary>
    public string Gid { get; set; } = string.Empty;

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 种子文件名
    /// </summary>
    public string? BtName { get; set; }

    /// <summary>
    /// 总长度
    /// </summary>
    public long TotalLength { get; set; }

    /// <summary>
    /// 已完成长度
    /// </summary>
    public long CompletedLength { get; set; }

    /// <summary>
    /// 已上传字节数
    /// </summary>
    public long UploadedLength { get; set; }

    /// <summary>
    /// 分享率
    /// </summary>
    public decimal ShareRatio { get; set; }

    /// <summary>
    /// 下载速度
    /// </summary>
    public long DownloadSpeed { get; set; }

    /// <summary>
    /// 上传速度
    /// </summary>
    public long UploadSpeed { get; set; }

    /// <summary>
    /// 下载目录
    /// </summary>
    public string? Dir { get; set; }

    /// <summary>
    /// 文件列表
    /// </summary>
    public List<Aria2FileDto> Files { get; set; } = new();

    /// <summary>
    /// Peer 列表
    /// </summary>
    public List<Aria2PeerDto> Peers { get; set; } = new();

    /// <summary>
    /// 连接数
    /// </summary>
    public int? Connections { get; set; }
}

/// <summary>
/// Aria2 连接状态
/// </summary>
public class Aria2ConnectionStatusDto
{
    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 会话信息
    /// </summary>
    public string? SessionInfo { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Aria2 版本信息
/// </summary>
public class Aria2VersionDto
{
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 已启用的功能列表
    /// </summary>
    public List<string> EnabledFeatures { get; set; } = new();
}

/// <summary>
/// Aria2 会话信息
/// </summary>
public class Aria2SessionDto
{
    /// <summary>
    /// 会话 ID
    /// </summary>
    public string? SessionId { get; set; }
}

/// <summary>
/// IP 地理位置 DTO
/// </summary>
public class IpGeolocationDto
{
    /// <summary>
    /// 状态 (success/fail)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 查询的 IP 地址
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// 国家
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 国家代码
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? Message { get; set; }
}
