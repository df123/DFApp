using System.Threading.Tasks;

namespace DFApp.Data;

public interface IDFAppDbSchemaMigrator
{
    Task MigrateAsync();
}
