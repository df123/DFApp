using System.Text.Json;
using DFApp.Downloader.Core;
using DFApp.Downloader.Core.Configuration;
using DFApp.Downloader.Core.Data;
using DFApp.Downloader.Core.Engine;
using DFApp.Downloader.Core.Entities;
using DFApp.Downloader.Core.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DFApp.Downloader.App;

public class Program
{
    public static async Task Main(string[] args)
    {
        // 配置 Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/downloader-.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("DFApp.Downloader 启动中...");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            // 加载配置
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
            var settings = LoadSettings(settingsPath);

            // 注册服务
            builder.Services.AddSingleton(settings);
            builder.Services.AddSingleton<DownloaderDbContext>(sp =>
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "downloader.db");
                return new DownloaderDbContext(dbPath);
            });

            builder.Services.AddHttpClient<DownloadNotificationClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseProxy = false });

            // 默认 HttpClient（DownloadManager / DownloadEngine 注入，空名 options）：同样禁用代理
            builder.Services.AddHttpClient(string.Empty)
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseProxy = false });

            builder.Services.AddSingleton<DownloadEngine>();
            builder.Services.AddSingleton<DownloadNotificationClient>();
            builder.Services.AddSingleton<DownloadManager>();
            builder.Services.AddSingleton<Core.Security.AccountService>();

            // 配置控制器
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            var app = builder.Build();

            // 初始化数据库表并种子默认管理员账号（旧库自动升级：Accounts 表不存在时由 CodeFirst 创建）
            var dbContext = app.Services.GetRequiredService<DownloaderDbContext>();
            dbContext.EnsureTablesCreated();
            app.Services.GetRequiredService<Core.Security.AccountService>().EnsureAdminInitialized();

            // 登录认证：Web 界面使用账号密码登录（HttpOnly 会话 Cookie）；
            // 保留 X-Management-Token 作为脚本/程序化调用的兼容凭据；
            // 登录接口与静态页面资源匿名放行，其余 /api/* 与 /media/* 一律要求认证（含本机访问）
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                var isLoginApi = HttpMethods.IsPost(context.Request.Method)
                    && path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase);
                var isProtected = path.StartsWithSegments("/api") || path.StartsWithSegments("/media");

                if (isLoginApi || !isProtected)
                {
                    await next();
                    return;
                }

                var accountService = context.RequestServices.GetRequiredService<Core.Security.AccountService>();
                var authorized = accountService.ValidateSession(context.Request.Cookies[Controllers.AuthController.SessionCookieName]);

                if (!authorized)
                {
                    // 兼容令牌凭据：非浏览器调用可继续携带 X-Management-Token
                    var settings = context.RequestServices.GetRequiredService<DownloaderSettings>();
                    var expected = Core.Configuration.SettingsSecrets.ManagementToken(settings);
                    var provided = context.Request.Headers["X-Management-Token"].ToString();
                    authorized = expected.Length > 0 &&
                        System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                            System.Text.Encoding.UTF8.GetBytes(provided),
                            System.Text.Encoding.UTF8.GetBytes(expected));
                }

                if (!authorized)
                {
                    Log.Warning("拒绝未认证的管理请求: {Remote} {Method} {Path}",
                        context.Connection.RemoteIpAddress, context.Request.Method, context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("401 Unauthorized: 请先登录");
                    return;
                }

                await next();
            });

            // 静态文件（Web 管理界面）
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // 下载目录静态映射：图片大图预览等通过 /media/{文件名} 访问
            var downloadPath = Environment.ExpandEnvironmentVariables(settings.DownloadPath);
            if (Directory.Exists(downloadPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(downloadPath),
                    RequestPath = "/media"
                });
            }

            app.MapControllers();

            // SPA fallback：非文件请求回退到 index.html
            app.MapFallbackToFile("index.html");

            // 启动下载管理器
            var manager = app.Services.GetRequiredService<DownloadManager>();
            await manager.StartAsync();

            // 打开浏览器（首次运行）
            if (args.Contains("--open-browser"))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = $"http://localhost:{settings.WebServerPort}",
                        UseShellExecute = true
                    });
                }
                catch { }
            }

            Log.Information("DFApp.Downloader 已启动，监听: {Bind}:{Port}（登录认证已启用，默认账号 admin/admin，请尽快修改密码）",
                settings.WebServerBind, settings.WebServerPort);
            await app.RunAsync($"http://{settings.WebServerBind}:{settings.WebServerPort}");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "DFApp.Downloader 启动失败");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static DownloaderSettings LoadSettings(string path)
    {
        if (File.Exists(path))
        {
            var settingsJson = File.ReadAllText(path);
            return JsonSerializer.Deserialize<DownloaderSettings>(settingsJson) ?? new DownloaderSettings();
        }

        var defaultSettings = new DownloaderSettings();
        var defaultJson = JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, defaultJson);
        return defaultSettings;
    }
}
