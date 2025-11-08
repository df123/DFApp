using DFApp.LotteryProxy.Models;
using DFApp.LotteryProxy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.Extensions.Options;
using DFApp.LotteryProxy.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 配置服务
ConfigureServices(builder);

var app = builder.Build();

// 配置中间件管道
ConfigureMiddleware(app);

// 配置API端点
ConfigureEndpoints(app);

// 运行应用
app.Run();

/// <summary>
/// 配置依赖注入服务
/// </summary>
static void ConfigureServices(WebApplicationBuilder builder)
{
    // 添加配置
    builder.Services.Configure<ProxySettings>(
        builder.Configuration.GetSection("ProxySettings"));

    // 注册ProxySettings为单例
    builder.Services.AddSingleton(sp => 
        sp.GetRequiredService<IOptions<ProxySettings>>().Value);

    // 添加HTTP客户端
    builder.Services.AddHttpClient();

    // 添加代理服务
    builder.Services.AddScoped<LotteryProxyService>();

    // 添加CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // 添加Swagger（仅开发环境）
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
            { 
                Title = "彩票数据代理API", 
                Version = "v1",
                Description = "用于代理中国福利彩票网站数据的API服务"
            });
            
            // 包含XML注释（如果有的话）
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
    }
}

/// <summary>
/// 配置中间件管道
/// </summary>
static void ConfigureMiddleware(WebApplication app)
{
    // 开发环境配置
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "彩票数据代理API v1");
            c.RoutePrefix = string.Empty; // 设置Swagger UI为根路径
        });
    }

    // 使用CORS
    app.UseCors();

    // 使用IP白名单中间件
    app.UseMiddleware<IpWhitelistMiddleware>();

    // 使用异常处理中间件
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
            var response = new
            {
                error = "服务器内部错误",
                message = exception?.Message ?? "未知错误",
                timestamp = DateTime.UtcNow
            };
            
            await context.Response.WriteAsJsonAsync(response);
        });
    });
}

/// <summary>
/// 配置API端点
/// </summary>
static void ConfigureEndpoints(WebApplication app)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var proxyService = app.Services.GetRequiredService<LotteryProxyService>();

    // 健康检查端点
    app.MapGet("/api/health", () => 
    {
        return Results.Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    })
    .WithName("HealthCheck")
    .WithTags("健康检查")
    .WithSummary("健康检查")
    .WithDescription("检查API服务是否正常运行");

    // 彩票数据代理端点
    app.MapGet("/api/proxy/lottery/findDrawNotice", async (HttpContext context) =>
    {
        logger.LogInformation("收到彩票数据代理请求");
        
        // 获取查询字符串
        var queryString = context.Request.QueryString.ToString();
        
        // 移除开头的'?'
        if (queryString.StartsWith('?'))
        {
            queryString = queryString.Substring(1);
        }
        
        logger.LogInformation("查询字符串: {QueryString}", queryString);
        
        // 调用代理服务
        var result = await proxyService.ProxyRequestAsync(queryString);
        
        logger.LogInformation("代理请求完成");
        
        return result;
    })
    .WithName("ProxyLotteryData")
    .WithTags("彩票代理")
    .WithSummary("代理彩票数据请求")
    .WithDescription("代理请求到中国福利彩票网站获取开奖数据");

    // 根路径重定向到Swagger（仅开发环境）
    if (app.Environment.IsDevelopment())
    {
        app.MapGet("/", () => Results.Redirect("/swagger"));
    }
    else
    {
        // 生产环境根路径返回基本信息
        app.MapGet("/", () => 
        {
            return Results.Ok(new 
            { 
                service = "彩票数据代理API",
                status = "running",
                timestamp = DateTime.UtcNow
            });
        });
    }

    logger.LogInformation("API端点配置完成");
}