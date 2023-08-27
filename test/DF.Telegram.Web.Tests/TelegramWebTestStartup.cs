using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace DF.Telegram;

public class TelegramWebTestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication<TelegramWebTestModule>();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.InitializeApplication();
    }
}
