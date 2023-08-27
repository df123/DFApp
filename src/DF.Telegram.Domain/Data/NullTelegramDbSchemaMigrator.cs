using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace DF.Telegram.Data;

/* This is used if database provider does't define
 * ITelegramDbSchemaMigrator implementation.
 */
public class NullTelegramDbSchemaMigrator : ITelegramDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
