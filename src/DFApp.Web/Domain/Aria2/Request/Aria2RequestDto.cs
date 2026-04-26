using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFApp.Aria2.Request;

/// <summary>
/// Aria2 RPC 请求 DTO（用于队列传递）
/// </summary>
public class Aria2RequestDto
{
    [JsonPropertyName("jsonrpc")]
    public string JSONRPC { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public IList<object> Params { get; set; } = new List<object>();
}
