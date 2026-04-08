using System.Text.Json.Serialization;

namespace DFApp.Aria2.Response;

/// <summary>
/// Aria2 RPC 响应
/// </summary>
public class Aria2Response : ResponseBase
{
    [JsonPropertyName("jsonrpc")]
    public string JSONRPC { get; set; } = string.Empty;
}
