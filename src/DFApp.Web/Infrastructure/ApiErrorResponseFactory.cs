using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 统一构建 API 错误响应，避免泄露 traceId、堆栈、System.Text.Json 等框架细节。
/// 模型校验失败在生产环境仅返回通用提示，开发环境保留详细信息以便排查。
/// </summary>
public static class ApiErrorResponseFactory
{
    /// <summary>
    /// 替换 [ApiController] 默认的 ValidationProblemDetails 响应。
    /// 生产环境不暴露字段级校验错误与 traceId。
    /// </summary>
    public static IActionResult CreateModelStateResponse(ActionContext context)
    {
        var env = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

        // 生产环境关闭详细模型校验信息，仅返回通用提示
        var message = env.IsProduction()
            ? "请求参数无效，请检查后重试"
            : BuildModelStateMessage(context.ModelState);

        var response = new ApiResponse<object>
        {
            Success = false,
            Code = StatusCodes.Status400BadRequest.ToString(),
            Message = message,
            Data = null
        };

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    /// <summary>
    /// 全局异常处理中间件回调（配合 UseExceptionHandler 使用）。
    /// 捕获 MVC 过滤器之外的异常，统一返回 ApiResponse，不泄露任何框架细节。
    /// </summary>
    public static async Task WriteExceptionResponse(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Code = StatusCodes.Status500InternalServerError.ToString(),
            Message = "服务器内部错误，请稍后重试",
            Data = null
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// 将模型状态错误汇总为人类可读的消息（仅非生产环境使用）
    /// </summary>
    private static string BuildModelStateMessage(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value!.Errors.Select(e => e.ErrorMessage))}");

        var detail = string.Join("; ", errors);
        return string.IsNullOrEmpty(detail) ? "请求参数无效" : $"请求参数无效 - {detail}";
    }
}
