using System.Net;
using DFApp.LotteryProxy.Middleware;
using DFApp.LotteryProxy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace DFApp.Web.Tests.LotteryProxy;

/// <summary>
/// IpWhitelistMiddleware 令牌校验测试：
/// 服务暴露公网时，X-Proxy-Token 共享密钥是 IP 白名单之外的第二道门。
/// 语义：令牌已配置时所有请求（/api/health 除外）必须携带匹配的 X-Proxy-Token；
/// 令牌未配置时不做令牌校验，行为与旧版一致。
/// </summary>
public class IpWhitelistMiddlewareTests
{
    private const string Token = "unit-test-secret-token";

    /// <summary>记录下游管道是否被调用（闭包布尔值返回的是拷贝，必须用可变对象）</summary>
    private sealed class NextRecorder
    {
        public bool Called;

        public RequestDelegate Delegate => _ => { Called = true; return Task.CompletedTask; };
    }

    /// <summary>构造中间件与请求上下文；recorder.Called 记录是否放行到下游</summary>
    private static (IpWhitelistMiddleware Middleware, DefaultHttpContext Context, NextRecorder Recorder) Create(
        string proxyToken,
        string? requestToken,
        string path = "/",
        string clientIp = "127.0.0.1")
    {
        var settings = new ProxySettings
        {
            ProxyToken = proxyToken,
            AllowedIPs = new List<string>(),
        };

        var context = new DefaultHttpContext();
        context.Request.Path = path;
        if (requestToken != null)
        {
            context.Request.Headers["X-Proxy-Token"] = requestToken;
        }
        context.Connection.RemoteIpAddress = IPAddress.Parse(clientIp);

        var recorder = new NextRecorder();
        var middleware = new IpWhitelistMiddleware(
            recorder.Delegate,
            NullLogger<IpWhitelistMiddleware>.Instance,
            settings);

        return (middleware, context, recorder);
    }

    [Fact]
    public async Task 令牌匹配_本机请求应放行()
    {
        var (middleware, context, recorder) = Create(Token, Token);
        await middleware.InvokeAsync(context);
        context.Response.StatusCode.Should().Be(200);
        recorder.Called.Should().BeTrue("令牌与白名单都通过时应放行到下游");
    }

    [Fact]
    public async Task 令牌缺失或错误_应返回401且不放行()
    {
        var (missing, ctx1, missingRecorder) = Create(Token, null);
        await missing.InvokeAsync(ctx1);
        ctx1.Response.StatusCode.Should().Be(401);
        missingRecorder.Called.Should().BeFalse("无令牌请求必须短路");

        var (wrong, ctx2, wrongRecorder) = Create(Token, "not-the-token");
        await wrong.InvokeAsync(ctx2);
        ctx2.Response.StatusCode.Should().Be(401);
        wrongRecorder.Called.Should().BeFalse("错误令牌请求必须短路");
    }

    [Fact]
    public async Task 未配置令牌_不做令牌校验保持旧行为()
    {
        var (middleware, context, recorder) = Create("", null);
        await middleware.InvokeAsync(context);
        recorder.Called.Should().BeTrue("令牌未配置时本机请求仍按白名单逻辑放行");
    }

    [Fact]
    public async Task 健康检查路径_免令牌校验()
    {
        var (middleware, context, recorder) = Create(Token, null, "/api/health");
        await middleware.InvokeAsync(context);
        recorder.Called.Should().BeTrue("容器探活的 /api/health 不携带令牌，必须放行");
    }

    [Fact]
    public async Task 公网IP即使令牌正确_仍要过白名单()
    {
        var (middleware, context, recorder) = Create(Token, Token, "/", "8.8.8.8");
        await middleware.InvokeAsync(context);
        context.Response.StatusCode.Should().Be(403, "令牌是第二道门，不能绕过 IP 白名单");
        recorder.Called.Should().BeFalse();
    }
}
