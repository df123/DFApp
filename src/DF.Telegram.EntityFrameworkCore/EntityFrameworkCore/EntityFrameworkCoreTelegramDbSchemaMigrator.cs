using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DF.Telegram.Data;
using Volo.Abp.DependencyInjection;

namespace DF.Telegram.EntityFrameworkCore;

public class EntityFrameworkCoreTelegramDbSchemaMigrator
    : ITelegramDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreTelegramDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the TelegramDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<TelegramDbContext>()
            .Database
            .MigrateAsync();
    }
}
