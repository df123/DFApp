using System;
using System.Threading.Tasks;
using System.Text;
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
using DFApp.Queue;
using DFApp.Helper;
using DFApp.Background;

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

            // 配置 AppsettingsHelper
            builder.Services.AddSingleton(new AppsettingsHelper(builder.Configuration));

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

            // 注册密码哈希服务（无状态，使用 Transient）
            builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();

            // 配置 HttpClient
            builder.Services.AddHttpClient();

            // 配置后台任务队列
            builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddHostedService<BackgroundQueueHostedService>();

            // 配置后台服务
            builder.Services.AddHostedService<Aria2BackgroundWorker>();
            builder.Services.AddHostedService<ListenTelegramService>();
            builder.Services.AddHostedService<Web.Background.Aria2MonitorWorker>();

            // 配置 CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(
                            "http://localhost:8848",
                            "https://localhost:8848"
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
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
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
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
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

            // 配置 Quartz.NET
            Quartz.Logging.LogProvider.IsDisabled = true;
            builder.Services.AddQuartz(q =>
            {
                // 这里可以配置 Quartz 作业
                // q.AddJob<YourJob>(opts => opts.WithIdentity("job1"));
                // q.AddTrigger(opts => opts
                //     .ForJob("job1")
                //     .WithIdentity("trigger1")
                //     .StartNow()
                //     .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever()));
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

            app.UseMiddleware<CurrentUserMiddleware>();
            app.UseAuthentication();
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
