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
    /// 创建数据库表（如果不存在）
    /// </summary>
    public void EnsureTablesCreated()
    {
        using var db = CreateClient();
        db.CodeFirst.InitTables(
            typeof(Entities.DownloadItem),
            typeof(Entities.DownloadSegment)
        );

        // 兼容旧库：补充新增的失败重试计数字段
        var columns = db.DbMaintenance.GetColumnInfosByTableName("DownloadItems");
        if (columns.All(c => c.DbColumnName != nameof(Entities.DownloadItem.RetryCount)))
        {
            db.DbMaintenance.AddColumn("DownloadItems", new DbColumnInfo
            {
                DbColumnName = nameof(Entities.DownloadItem.RetryCount),
                DataType = "int",
                IsNullable = false,
                DefaultValue = "0"
            });
        }

        // 兼容旧库：补充聊天消息字段（可空，历史记录留空）
        if (columns.All(c => c.DbColumnName != nameof(Entities.DownloadItem.Message)))
        {
            db.DbMaintenance.AddColumn("DownloadItems", new DbColumnInfo
            {
                DbColumnName = nameof(Entities.DownloadItem.Message),
                DataType = "text",
                IsNullable = true
            });
        }
    }
}
