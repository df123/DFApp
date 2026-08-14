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

            // 配置 CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 配置控制器
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            var app = builder.Build();

            app.UseCors();

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

            Log.Information("DFApp.Downloader 已启动，端口: {Port}", settings.WebServerPort);
            await app.RunAsync($"http://0.0.0.0:{settings.WebServerPort}");
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
