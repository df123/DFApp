using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DF.Telegram.EntityFrameworkCore;
using DF.Telegram.Localization;
using DF.Telegram.Web.Menus;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using DF.Telegram.Helper;
using Starksoft.Net.Proxy;
using DF.Telegram.TLConfig;
using Volo.Abp.BackgroundWorkers;
using Microsoft.AspNetCore.HttpOverrides;
using DF.Telegram.Queue;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DF.Telegram.Permissions;
using DF.Telegram.Background;
using Volo.Abp.Imaging;
using DF.Telegram.Media;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using System.Collections.Generic;
using Volo.CmsKit.Web;
using Volo.Abp.BackgroundWorkers.Quartz;

namespace DF.Telegram.Web;

[DependsOn(
    typeof(TelegramHttpApiModule),
    typeof(TelegramApplicationModule),
    typeof(TelegramEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpSettingManagementWebModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpImagingAbstractionsModule),
    typeof(AbpImagingImageSharpModule),
    typeof(AbpBackgroundWorkersQuartzModule)
    )]
[DependsOn(typeof(CmsKitWebModule))]
    public class TelegramWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(TelegramResource),
                typeof(TelegramDomainModule).Assembly,
                typeof(TelegramDomainSharedModule).Assembly,
                typeof(TelegramApplicationModule).Assembly,
                typeof(TelegramApplicationContractsModule).Assembly,
                typeof(TelegramWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("Telegram");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);
        ConfigureAuthorization();
        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.TokenCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
        });

        context.Services.AddSingleton<IQueueBase<string>, QueueBase<string>>();
        context.Services.AddSingleton<IQueueBase<DocumentQueueModel>, QueueBase<DocumentQueueModel>>();
        context.Services.AddSingleton<IQueueBase<PhotoQueueModel>, QueueBase<PhotoQueueModel>>();
        context.Services.AddSingleton<List<MediaInfoDto[]>>();
        context.Services.AddSingleton(new AppsettingsHelper(context.Services.GetConfiguration()));
        context.Services.AddSingleton(new HashHelper());

        context.Services.AddSingleton<WTelegram.Client>(m =>
        {
#nullable disable
            IQueueBase<string> queueBase = m.GetService<IQueueBase<string>>();
            WTelegram.Client client = new WTelegram.Client(what =>
            {
                m.GetService<TLConfigService>();
                string[] sections = new string[] { "Telegram", what };
                switch (what)
                {
                    case "session_pathname":
                    case "api_id":
                    case "api_hash":
                    case "phone_number": return AppsettingsHelper.app(sections);
                    case "verification_code":
                        return queueBase.GetItemAsync(default).Result;
                    default: return null;
                }
            });
#nullable restore
            if (bool.Parse(AppsettingsHelper.app(new string[] { "RunConfig", "Proxy", "EnableProxy" })))
            {
#pragma warning disable CS1998
                client.TcpHandler = async (address, port) =>
                {
                    var proxy = new Socks5ProxyClient(
                        AppsettingsHelper.app(new string[] { "RunConfig", "Proxy", "ProxyHost" }),
                    int.Parse(AppsettingsHelper.app(new string[] { "RunConfig", "Proxy", "ProxyPort" })));
                    return proxy.CreateConnection(address, port);
                };
#pragma warning restore CS1998
            }
            client.PingInterval = 300;
            client.MaxAutoReconnects = int.MaxValue;
            return client;
        });

        context.Services.AddHttpClient();
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.Mode = BundlingMode.Bundle;
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<TelegramWebModule>();
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<TelegramDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DF.Telegram.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<TelegramDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DF.Telegram.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<TelegramApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DF.Telegram.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<TelegramApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DF.Telegram.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<TelegramWebModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new TelegramMenuContributor());
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(TelegramApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Telegram API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    private void ConfigureAuthorization()
    {
        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AuthorizePage("/Media/Index", TelegramPermissions.Medias.Default);
            options.Conventions.AuthorizePage("/Media/EditModal", TelegramPermissions.Medias.Edit);
        });
    }

    public override async void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();

        if (!env.IsDevelopment())
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();
        app.UseUnitOfWork();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Telegram API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();

        //await context.AddBackgroundWorkerAsync<ListenTelegramService>();

    }
}
