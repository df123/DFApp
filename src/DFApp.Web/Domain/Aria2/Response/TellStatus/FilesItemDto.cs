using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFApp.Aria2.Response.TellStatus;

/// <summary>
/// 文件项 DTO（字符串属性版本，用于 RPC 响应反序列化）
/// </summary>
public class FilesItemDto
{
    [JsonPropertyName("completedLength")]
    public string CompletedLength { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public string Index { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public string Length { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("selected")]
    public string Selected { get; set; } = string.Empty;
}
