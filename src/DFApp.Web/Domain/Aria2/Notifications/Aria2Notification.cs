using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFApp.Aria2.Notifications;

/// <summary>
/// Aria2 WebSocket 通知
/// </summary>
public class Aria2Notification : ResponseBase
{
    [JsonPropertyName("jsonrpc")]
    public string JSONRPC { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public List<ParamsItem> Params { get; set; } = new();
}
