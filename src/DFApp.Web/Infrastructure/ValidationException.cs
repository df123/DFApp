using System;
using System.Collections.Generic;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 验证异常类，用于表示数据验证失败
/// </summary>
public class ValidationException : BusinessException
{
    /// <summary>
    /// 验证错误字典
    /// </summary>
    public IDictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息</param>
    public ValidationException(string message) : base("ValidationError", message)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validationErrors">验证错误字典</param>
    public ValidationException(IDictionary<string, string[]> validationErrors)
        : base("ValidationError", "数据验证失败")
    {
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <param name="errorMessage">错误消息</param>
    public ValidationException(string propertyName, string errorMessage)
        : base("ValidationError", "数据验证失败")
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="validationErrors">验证错误字典</param>
    public ValidationException(string message, IDictionary<string, string[]> validationErrors)
        : base("ValidationError", message)
    {
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }
}
