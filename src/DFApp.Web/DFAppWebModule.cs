using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DFApp.EntityFrameworkCore;
using DFApp.Localization;
using DFApp.Web.Menus;
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
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.OpenIddict;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.UI;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using DFApp.Queue;
using System.Collections.Generic;
using DFApp.Media;
using DFApp.Helper;
using DFApp.TLConfig;
using Starksoft.Net.Proxy;
using Volo.Abp.BackgroundWorkers;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DFApp.Permissions;
using Volo.CmsKit.Web;
using Volo.Abp.Imaging;
using Volo.Abp.BackgroundWorkers.Quartz;
using DFApp.Background;
using Volo.Abp.AspNetCore.SignalR;
using DFApp.Web.SignalRHub;

namespace DFApp.Web;

[DependsOn(
    typeof(DFAppHttpApiModule),
    typeof(DFAppApplicationModule),
    typeof(DFAppEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpSettingManagementWebModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpBackgroundWorkersQuartzModule)
    )]
[DependsOn(typeof(CmsKitWebModule))]
[DependsOn(typeof(AbpImagingAbstractionsModule))]
[DependsOn(typeof(AbpImagingImageSharpModule))]
[DependsOn(typeof(AbpAspNetCoreSignalRModule))]
    public class DFAppWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(DFAppResource),
                typeof(DFAppDomainModule).Assembly,
                typeof(DFAppDomainSharedModule).Assembly,
                typeof(DFAppApplicationModule).Assembly,
                typeof(DFAppApplicationContractsModule).Assembly,
                typeof(DFAppWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("DFApp");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", "6a19a84a-f89a-466c-861b-37c3ddf30da2");
            });
        }
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
        context.Services.AddSingleton<SinkHub>();

    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });

        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AuthorizePage("/Media/Index", DFAppPermissions.Medias.Default);
            options.Conventions.AuthorizePage("/Media/EditModal", DFAppPermissions.Medias.Edit);
            options.Conventions.AuthorizePage("/DynamicIP/Index", DFAppPermissions.DynamicIP.Default);
            options.Conventions.AuthorizePage("/LogSink/QueueSink/Index", DFAppPermissions.LogSink.Default);
            options.Conventions.AuthorizePage("/LogSink/SignalRSink/Index", DFAppPermissions.LogSink.Default);
            options.Conventions.AuthorizePage("/Lottery/Index", DFAppPermissions.Lottery.Default);
            options.Conventions.AuthorizePage("/Lottery/Result/Index", DFAppPermissions.Lottery.Default);
            options.Conventions.AuthorizePage("/Lottery/SpecifyPeriod/Index", DFAppPermissions.Lottery.Default);
            options.Conventions.AuthorizePage("/Lottery/Statistics/Index", DFAppPermissions.Lottery.Default);
            options.Conventions.AuthorizePage("/Lottery/StatisticsItem/Index", DFAppPermissions.Lottery.Default);
            options.Conventions.AuthorizePage("/Lottery/BatchCreate/Index", DFAppPermissions.Lottery.Default);

            options.Conventions.AuthorizeFolder("/Bookkeeping/Category", DFAppPermissions.BookkeepingCategory.Default);
            options.Conventions.AuthorizeFolder("/Bookkeeping/Expenditure", DFAppPermissions.BookkeepingExpenditure.Default);
            options.Conventions.AuthorizeFolder("/FileUploadDownload", DFAppPermissions.FileUploadDownload.Default);

        });

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
            options.AddMaps<DFAppWebModule>();
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<DFAppDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DFApp.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<DFAppDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DFApp.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<DFAppApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DFApp.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<DFAppApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}DFApp.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<DFAppWebModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new DFAppMenuContributor());
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(DFAppApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "DFApp API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
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
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "DFApp API");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();


        await context.AddBackgroundWorkerAsync<ListenTelegramService>();

    }
}
