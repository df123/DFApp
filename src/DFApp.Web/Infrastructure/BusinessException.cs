using System;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 业务异常类，用于处理业务逻辑中的错误
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 详细信息
    /// </summary>
    public object? Details { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息</param>
    public BusinessException(string message) : base(message)
    {
        Code = "BusinessError";
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
        Code = "BusinessError";
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="message">错误消息</param>
    public BusinessException(string code, string message) : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="message">错误消息</param>
    /// <param name="details">详细信息</param>
    public BusinessException(string code, string message, object? details) : base(message)
    {
        Code = code;
        Details = details;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常</param>
    public BusinessException(string code, string message, Exception innerException) : base(message, innerException)
    {
        Code = code;
    }
}
