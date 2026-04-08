using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFApp.Aria2.Response.TellStatus;

/// <summary>
/// TellStatus 结果 DTO（字符串属性版本，用于 RPC 响应反序列化）
/// </summary>
public class TellStatusResultDto
{
    [JsonPropertyName("bitfield")]
    public string Bitfield { get; set; } = string.Empty;

    [JsonPropertyName("completedLength")]
    public string CompletedLength { get; set; } = string.Empty;

    [JsonPropertyName("connections")]
    public string Connections { get; set; } = string.Empty;

    [JsonPropertyName("dir")]
    public string Dir { get; set; } = string.Empty;

    [JsonPropertyName("downloadSpeed")]
    public string DownloadSpeed { get; set; } = string.Empty;

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public List<FilesItemDto> Files { get; set; } = new();

    [JsonPropertyName("gid")]
    public string Gid { get; set; } = string.Empty;

    [JsonPropertyName("numPieces")]
    public string NumPieces { get; set; } = string.Empty;

    [JsonPropertyName("pieceLength")]
    public string PieceLength { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("totalLength")]
    public string TotalLength { get; set; } = string.Empty;

    [JsonPropertyName("uploadLength")]
    public string UploadLength { get; set; } = string.Empty;

    [JsonPropertyName("uploadSpeed")]
    public string UploadSpeed { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 创建者 ID
    /// </summary>
    public Guid? CreatorId { get; set; }
}
