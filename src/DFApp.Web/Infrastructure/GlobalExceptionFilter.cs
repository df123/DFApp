using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        // 使用统一响应格式构建错误响应
        var errorResponse = new ApiResponse<object>
        {
            Success = false,
            Code = ((int)statusCode).ToString(),
            Message = GetErrorMessage(context.Exception),
            Data = null
        };

        // 设置响应
        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = (int)statusCode
        };

        context.ExceptionHandled = true;
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
            ValidationException validationException when validationException.ValidationErrors.Count > 0
                => FormatValidationErrors(validationException.Message, validationException.ValidationErrors),
            _ => "服务器内部错误，请稍后重试"
        };
    }

    /// <summary>
    /// 格式化验证错误信息
    /// </summary>
    /// <param name="message">主消息</param>
    /// <param name="validationErrors">验证错误字典</param>
    /// <returns>格式化后的错误消息</returns>
    private static string FormatValidationErrors(string message, IDictionary<string, string[]> validationErrors)
    {
        var details = string.Join("; ", validationErrors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"));
        return string.IsNullOrEmpty(details) ? message : $"{message} - {details}";
    }
}
