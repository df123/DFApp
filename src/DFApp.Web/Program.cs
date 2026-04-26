using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;
using Serilog;
using Serilog.Events;
using SqlSugar;
using DFApp.Web.Hubs;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using DFApp.Web.Queue;
using DFApp.Web.Background;
using DFApp.Web.Services.ElectricVehicle;
using DFApp.Aria2;

namespace DFApp.Web;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting web host.");
            var builder = WebApplication.CreateBuilder(args);

            // 使用 Serilog
            builder.Host.UseSerilog();

            // 配置 AppsettingsHelper（已废弃，暂注释）
            // builder.Services.AddSingleton(new AppsettingsHelper(builder.Configuration));

            // 配置 SqlSugar
            builder.Services.AddSingleton<SqlSugarConfig>();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped<ISqlSugarClient>(s =>
            {
                var config = s.GetRequiredService<SqlSugarConfig>();
                return config.CreateClient();
            });

            // 配置 HTTP 上下文访问器
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();

            // 配置权限系统
            builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            // 注册通用仓储
            builder.Services.AddScoped(typeof(ISqlSugarRepository<,>), typeof(SqlSugarRepository<,>));
            builder.Services.AddScoped(typeof(ISqlSugarReadOnlyRepository<,>), typeof(SqlSugarReadOnlyRepository<,>));

            // 注册自定义仓储
            builder.Services.AddScoped<DFApp.FileFilter.IKeywordFilterRuleRepository, DFApp.FileFilter.KeywordFilterRuleRepository>();
            builder.Services.AddScoped<DFApp.Web.Data.ElectricVehicle.IGasolinePriceRepository, DFApp.Web.Data.ElectricVehicle.GasolinePriceRepository>();
            builder.Services.AddScoped<DFApp.Web.Data.Configuration.IConfigurationInfoRepository, DFApp.Web.Data.Configuration.ConfigurationInfoRepository>();
            builder.Services.AddScoped<DFApp.Web.Data.Bookkeeping.IBookkeepingExpenditureRepository, DFApp.Web.Data.Bookkeeping.BookkeepingExpenditureRepository>();

            // 注册密码哈希服务（无状态，使用 Transient）
            builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();

            // 注册油价刷新器（无状态，使用 Transient）
            builder.Services.AddTransient<GasolinePriceRefresher>();

            // 配置 HttpClient
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<Aria2RpcClient>();

            // 注册 Aria2 管理器（单例，维护请求历史记录）
            builder.Services.AddSingleton<Aria2Manager>();

            // 配置后台任务队列
            builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddSingleton<IQueueManagement, QueueManagement>();
            builder.Services.AddHostedService<BackgroundQueueHostedService>();

            builder.Services.AddHostedService<Aria2BackgroundWorker>();
            builder.Services.AddHostedService<ListenTelegramService>();

            // 配置应用服务
            builder.Services.AddScoped<DFApp.Web.Services.TG.TGLoginService>();
            builder.Services.AddScoped<DFApp.Web.Services.Account.AccountAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Account.UserManagementAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.RssSourceAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.RssSubscriptionAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.RssSubscriptionDownloadAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.RssWordSegmentAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.RssMirrorItemAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.RssFetchService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.RssSubscriptionService>();
            builder.Services.AddScoped<DFApp.Web.Services.Rss.WordSegmentService>();
            builder.Services.AddScoped<DFApp.Web.Services.Lottery.LotteryService>();
            builder.Services.AddScoped<DFApp.Web.Services.Lottery.LotteryResultService>();
            builder.Services.AddScoped<DFApp.Web.Services.Lottery.LotteryDataFetchService>();
            builder.Services.AddScoped<DFApp.Web.Services.Lottery.Simulation.LotterySSQSimulationService>();
            builder.Services.AddScoped<DFApp.Web.Services.Lottery.Simulation.LotteryKL8SimulationService>();
            builder.Services.AddScoped<DFApp.Web.Services.Lottery.CompoundLotteryService>();
            builder.Services.AddScoped<DFApp.Web.Services.ElectricVehicle.ElectricVehicleService>();
            builder.Services.AddScoped<DFApp.Web.Services.ElectricVehicle.ElectricVehicleCostService>();
            builder.Services.AddScoped<DFApp.Web.Services.ElectricVehicle.ElectricVehicleChargingRecordService>();
            builder.Services.AddScoped<DFApp.Web.Services.ElectricVehicle.GasolinePriceService>();
            builder.Services.AddScoped<DFApp.Web.Services.Bookkeeping.BookkeepingCategoryService>();
            builder.Services.AddScoped<DFApp.Web.Services.Bookkeeping.BookkeepingExpenditureService>();
            builder.Services.AddScoped<DFApp.Web.Services.Media.MediaInfoService>();
            builder.Services.AddScoped<DFApp.Web.Services.Media.ExternalLinkService>();
            builder.Services.AddScoped<DFApp.Web.Services.Aria2.Aria2ManageService>();
            builder.Services.AddScoped<DFApp.Web.Services.Aria2.Aria2Service>();
            builder.Services.AddScoped<DFApp.Web.Services.FileUploadDownload.FileUploadInfoService>();
            builder.Services.AddScoped<DFApp.Web.Services.Configuration.ConfigurationInfoService>();
            builder.Services.AddScoped<DFApp.Web.Services.FileFilter.KeywordFilterRuleService>();
            builder.Services.AddScoped<DFApp.Web.Services.IP.DynamicIPService>();
            builder.Services.AddScoped<DFApp.Web.Services.Identity.RoleManagementAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Identity.PermissionGrantManagementAppService>();
            builder.Services.AddScoped<DFApp.Web.Services.Identity.UserRoleManagementAppService>();
            builder.Services.AddHostedService<Web.Background.Aria2MonitorWorker>();

            // 配置 CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(
                            "http://localhost:9949",
                            "https://localhost:9949"
                        )
                        .WithHeaders("Authorization", "Content-Type", "X-Requested-With", "X-SignalR-User-Agent")
                        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                        .AllowCredentials();
                });
            });

            // 配置 JWT 认证
            var secretKey = builder.Configuration["Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret Key 未配置，请设置环境变量 JWT_SECRET_KEY");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // 禁用 Claim 映射，保留短格式（如 "sub"、"unique_name"），与 Token 生成端一致
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        RoleClaimType = DFAppClaimTypes.Role,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            // 配置控制器
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            })
                .AddJsonOptions(options =>
                {
                    // 使用驼峰命名策略，与前端 JSON 约定保持一致
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            // 配置 SignalR
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            });

            // 配置 Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "DFApp API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);

                // 配置 JWT 认证支持
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            Quartz.Logging.LogProvider.IsDisabled = true;
            builder.Services.AddQuartz(q =>
            {
                // GasolinePriceRefreshJob — 每晚21:00执行
                q.ScheduleJob<Background.GasolinePriceRefreshJob>(trigger => trigger
                    .WithIdentity("GasolinePriceRefreshJob-trigger")
                    .WithCronSchedule("0 0 21 * * ?"));

                // DiskSpaceCheckJob — 每10分钟执行
                q.ScheduleJob<Background.DiskSpaceCheckJob>(trigger => trigger
                    .WithIdentity("DiskSpaceCheckJob-trigger")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(10)
                        .RepeatForever()));

                // LotteryResultJob — 每晚23:00执行
                q.ScheduleJob<Background.LotteryResultJob>(trigger => trigger
                    .WithIdentity("LotteryResultJob-trigger")
                    .WithCronSchedule("0 0 23 * * ?"));

                // RssMirrorFetchJob — 每5分钟执行
                q.ScheduleJob<Background.RssMirrorFetchJob>(trigger => trigger
                    .WithIdentity("RssMirrorFetchJob-trigger")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(5)
                        .RepeatForever()));
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            var app = builder.Build();

            var env = app.Environment;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            if (!env.IsDevelopment())
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseCors();

            app.UseAuthentication();
            app.UseMiddleware<CurrentUserMiddleware>();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DFApp API");
            });

            app.MapControllers();
            app.MapHub<Aria2Hub>(Aria2Hub.HubUrl);

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
