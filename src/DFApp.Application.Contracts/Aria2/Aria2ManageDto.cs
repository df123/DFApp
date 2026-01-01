using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFApp.Aria2
{
    /// <summary>
    /// Aria2 全局状态
    /// </summary>
    public class Aria2GlobalStatDto
    {
        [JsonPropertyName("downloadSpeed")]
        public string DownloadSpeed { get; set; } = string.Empty;

        [JsonPropertyName("uploadSpeed")]
        public string UploadSpeed { get; set; } = string.Empty;

        [JsonPropertyName("numActive")]
        public string ActiveCount { get; set; } = string.Empty;

        [JsonPropertyName("numWaiting")]
        public string WaitingCount { get; set; } = string.Empty;

        [JsonPropertyName("numStopped")]
        public string StoppedCount { get; set; } = string.Empty;

        [JsonPropertyName("numStoppedTotal")]
        public string StoppedTotal { get; set; } = string.Empty;
    }

    /// <summary>
    /// Aria2 任务信息
    /// </summary>
    public class Aria2TaskDto
    {
        public string Gid { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long TotalLength { get; set; }
        public long CompletedLength { get; set; }
        public long DownloadSpeed { get; set; }
        public long UploadSpeed { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Aria2FileDto>? Files { get; set; }
        public string? Dir { get; set; }
        public int? Connections { get; set; }

        /// <summary>
        /// 分享率（上传量/下载量）
        /// </summary>
        public decimal ShareRatio { get; set; }

        /// <summary>
        /// 已上传字节数
        /// </summary>
        public long UploadedLength { get; set; }

        /// <summary>
        /// 种子下载的Peer信息列表
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
        public string Index { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Length { get; set; }
        public long CompletedLength { get; set; }
        public bool Selected { get; set; }
        public List<Aria2UriDto>? Uris { get; set; }
    }

    /// <summary>
    /// Aria2 URI 信息
    /// </summary>
    public class Aria2UriDto
    {
        public string Uri { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 暂停任务请求
    /// </summary>
    public class PauseTasksRequestDto
    {
        public List<string> Gids { get; set; } = new List<string>();
    }

    /// <summary>
    /// 停止任务请求
    /// </summary>
    public class StopTasksRequestDto
    {
        public List<string> Gids { get; set; } = new List<string>();
    }

    /// <summary>
    /// 删除任务请求
    /// </summary>
    public class RemoveTasksRequestDto
    {
        public List<string> Gids { get; set; } = new List<string>();
    }

    /// <summary>
    /// Aria2 连接状态
    /// </summary>
    public class Aria2ConnectionStatusDto
    {
        public bool IsConnected { get; set; }
        public string? Version { get; set; }
        public string? SessionInfo { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Aria2 版本信息
    /// </summary>
    public class Aria2VersionDto
    {
        public string Version { get; set; } = string.Empty;
        public List<string> EnabledFeatures { get; set; } = new List<string>();
    }

    /// <summary>
    /// Aria2 会话信息
    /// </summary>
    public class Aria2SessionDto
    {
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// 添加种子文件下载请求
    /// </summary>
    public class AddTorrentRequestDto
    {
        /// <summary>
        /// 种子文件的 Base64 编码内容
        /// </summary>
        public string TorrentData { get; set; } = string.Empty;

        /// <summary>
        /// 保存路径（可选）
        /// </summary>
        public string? SavePath { get; set; }

        /// <summary>
        /// 下载选项（可选）
        /// </summary>
        public Dictionary<string, object>? Options { get; set; }
    }

    /// <summary>
    /// 批量添加种子文件下载请求
    /// </summary>
    public class BatchAddTorrentRequestDto
    {
        /// <summary>
        /// 种子文件列表
        /// </summary>
        public List<TorrentFileItemDto> Torrents { get; set; } = new List<TorrentFileItemDto>();

        /// <summary>
        /// 保存路径（可选，应用于所有种子）
        /// </summary>
        public string? SavePath { get; set; }
    }

    /// <summary>
    /// 种子文件项
    /// </summary>
    public class TorrentFileItemDto
    {
        /// <summary>
        /// 种子文件的 Base64 编码内容
        /// </summary>
        public string TorrentData { get; set; } = string.Empty;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Aria2 Peer信息（BitTorrent对等连接）
    /// </summary>
    public class Aria2PeerDto
    {
        /// <summary>
        /// Peer ID
        /// </summary>
        public string PeerId { get; set; } = string.Empty;

        /// <summary>
        /// IP地址
        /// </summary>
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 客户端标识（如：uTorrent/2.0.4）
        /// </summary>
        public string? Client { get; set; } = string.Empty;

        /// <summary>
        /// 该Peer正在下载（从我们这里）
        /// </summary>
        public bool AmChoking { get; set; }

        /// <summary>
        /// 该Peer被我们阻塞
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
        /// Seeder标记（是否拥有完整文件）
        /// </summary>
        public bool Seeder { get; set; }

        /// <summary>
        /// 国家（通过IP查询获取）
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 城市（通过IP查询获取）
        /// </summary>
        public string? City { get; set; }
    }

    /// <summary>
    /// 任务详情（包含完整信息）
    /// </summary>
    public class Aria2TaskDetailDto
    {
        public string Gid { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? BtName { get; set; }
        public long TotalLength { get; set; }
        public long CompletedLength { get; set; }
        public long UploadedLength { get; set; }
        public decimal ShareRatio { get; set; }
        public long DownloadSpeed { get; set; }
        public long UploadSpeed { get; set; }
        public string? Dir { get; set; }
        public List<Aria2FileDto> Files { get; set; } = new List<Aria2FileDto>();
        public List<Aria2PeerDto> Peers { get; set; } = new List<Aria2PeerDto>();
        public int? Connections { get; set; }
    }
}
