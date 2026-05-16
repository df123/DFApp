using System.Text.Json.Serialization;

namespace DFApp.Aria2.Notifications;

/// <summary>
/// Aria2 WebSocket 通知参数项
/// </summary>
public class ParamsItem
{
    [JsonPropertyName("gid")]
    public string GID { get; set; } = string.Empty;
}
