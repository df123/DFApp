using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace DFApp.Data;

/* This is used if database provider does't define
 * IDFAppDbSchemaMigrator implementation.
 */
public class NullDFAppDbSchemaMigrator : IDFAppDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
