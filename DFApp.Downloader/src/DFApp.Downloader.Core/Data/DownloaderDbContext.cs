using SqlSugar;

namespace DFApp.Downloader.Core.Data;

/// <summary>
/// SqlSugar 数据库客户端工厂
/// </summary>
public class DownloaderDbContext
{
    private readonly string _connectionString;

    public DownloaderDbContext(string dbPath)
    {
        _connectionString = $"DataSource={dbPath}";
    }

    /// <summary>
    /// 创建 SqlSugar 客户端实例
    /// </summary>
    public ISqlSugarClient CreateClient()
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = _connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
    }

    /// <summary>
    /// 初始化数据库表
    /// </summary>
    public void InitDatabase()
    {
        using var db = CreateClient();
        db.CodeFirst.InitTables(
            typeof(Entities.DownloadItem),
            typeof(Entities.DownloadSegment)
        );
    }
}
