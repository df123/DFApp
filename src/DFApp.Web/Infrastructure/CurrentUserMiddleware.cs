using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DFApp.Web.Infrastructure;

/// <summary>
/// 当前用户中间件，用于从 JWT Token 中提取用户信息并设置到 CurrentUser
/// </summary>
public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next">下一个中间件</param>
    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <returns>异步任务</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var currentUser = context.RequestServices.GetRequiredService<Data.ICurrentUser>();
        var user = context.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            // 从 JWT Token 的 Claims 中提取用户信息
            // 优先使用自定义的 claim 类型，如果不存在则使用标准的 claim 类型
            var userIdClaim = user.FindFirst("sub") ?? user.FindFirst(ClaimTypes.NameIdentifier);
            var userNameClaim = user.FindFirst("unique_name") ?? user.FindFirst(ClaimTypes.Name);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                ((Data.CurrentUser)currentUser).Id = userId;
            }
            if (userNameClaim != null)
            {
                ((Data.CurrentUser)currentUser).UserName = userNameClaim.Value;
            }
        }

        await _next(context);
    }
}
