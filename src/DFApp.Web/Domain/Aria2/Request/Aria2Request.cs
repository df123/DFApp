using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DFApp.Aria2.Request;

/// <summary>
/// Aria2 RPC 请求
/// </summary>
public class Aria2Request
{
    [JsonPropertyName("jsonrpc")]
    public string JSONRPC { get; set; } = "2.0";

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public IList<object> Params { get; set; } = new List<object>();

    /// <summary>
    /// 构造函数，自动添加 token 到 Params[0]
    /// </summary>
    /// <param name="id">请求 ID</param>
    /// <param name="secret">RPC 密钥</param>
    public Aria2Request(string id, string? secret = null)
    {
        Id = id;
        JSONRPC = "2.0";
        Params = new List<object>();
        if (!string.IsNullOrEmpty(secret))
        {
            Params.Add($"token:{secret}");
        }
    }
}
