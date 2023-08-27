using System.Threading.Tasks;

namespace DF.Telegram.Data;

public interface ITelegramDbSchemaMigrator
{
    Task MigrateAsync();
}
