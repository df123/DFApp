namespace DFApp.Web.Infrastructure;

/// <summary>
/// 统一 API 响应模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 状态码或错误代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }
}
