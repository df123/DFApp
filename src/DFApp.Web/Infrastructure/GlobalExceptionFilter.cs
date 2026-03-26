using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 全局异常过滤器，用于捕获和处理所有异常
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    /// <summary>
    /// 异常发生时调用
    /// </summary>
    /// <param name="context">异常上下文</param>
    public void OnException(ExceptionContext context)
    {
        // 记录异常日志
        Log.Error(context.Exception, "发生未处理的异常: {Message}", context.Exception.Message);

        // 根据异常类型确定 HTTP 状态码
        var statusCode = context.Exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            ValidationException => HttpStatusCode.BadRequest,
            BusinessException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ArgumentException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        // 构建错误响应
        var errorResponse = new ErrorResponse
        {
            Code = GetErrorCode(context.Exception),
            Message = GetErrorMessage(context.Exception),
            Details = GetErrorDetails(context.Exception),
            Timestamp = DateTime.UtcNow
        };

        // 如果是开发环境，添加堆栈跟踪
        var env = context.HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        if (env.EnvironmentName == Microsoft.AspNetCore.Hosting.EnvironmentName.Development)
        {
            errorResponse.StackTrace = context.Exception.StackTrace;
        }

        // 设置响应
        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = (int)statusCode
        };

        context.ExceptionHandled = true;
    }

    /// <summary>
    /// 获取错误代码
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>错误代码</returns>
    private static string GetErrorCode(Exception exception)
    {
        return exception switch
        {
            BusinessException businessException => businessException.Code,
            _ => exception.GetType().Name
        };
    }

    /// <summary>
    /// 获取错误消息
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>错误消息</returns>
    private static string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            BusinessException businessException when !string.IsNullOrEmpty(businessException.Message) => businessException.Message,
            _ => "服务器内部错误，请稍后重试"
        };
    }

    /// <summary>
    /// 获取错误详细信息
    /// </summary>
    /// <param name="exception">异常</param>
    /// <returns>错误详细信息</returns>
    private static object? GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => validationException.ValidationErrors,
            BusinessException businessException => businessException.Details,
            _ => null
        };
    }
}

/// <summary>
/// 错误响应模型
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 详细信息
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 堆栈跟踪（仅开发环境）
    /// </summary>
    public string? StackTrace { get; set; }
}
