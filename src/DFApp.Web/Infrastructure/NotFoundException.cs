namespace DFApp.Web.Infrastructure;

/// <summary>
/// 未找到异常类，用于表示资源未找到
/// </summary>
public class NotFoundException : BusinessException
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// 资源 ID
    /// </summary>
    public object? ResourceId { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息</param>
    public NotFoundException(string message) : base("NotFound", message)
    {
        ResourceType = string.Empty;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="resourceType">资源类型</param>
    /// <param name="resourceId">资源 ID</param>
    public NotFoundException(string resourceType, object resourceId)
        : base("NotFound", $"资源 '{resourceType}' (ID: {resourceId}) 未找到")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="resourceType">资源类型</param>
    /// <param name="resourceId">资源 ID</param>
    /// <param name="message">错误消息</param>
    public NotFoundException(string resourceType, object resourceId, string message)
        : base("NotFound", message)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
