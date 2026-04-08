using System.Text.Json.Serialization;

namespace DFApp.Aria2;

/// <summary>
/// Aria2 RPC 响应基类
/// </summary>
public class ResponseBase
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
