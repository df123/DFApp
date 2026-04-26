using System.Text.Json.Serialization;

namespace DFApp.Aria2.Response.TellStatus;

/// <summary>
/// Aria2 TellStatus RPC 响应
/// </summary>
public class TellStatusResponse : Aria2Response
{
    [JsonPropertyName("result")]
    public TellStatusResult? Result { get; set; }
}
