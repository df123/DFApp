using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace DFApp.Web.Data;

/// <summary>
/// Transient 独立数据库上下文（Transient.db）。
/// 存放可抛弃、可再生的非持久数据（当前为 RSS 镜像条目与分词，后续同类数据也放这里），
/// 与主库 DFApp.db 分离，避免主库膨胀影响远程备份。
/// </summary>
public class TransientDbContext
{
    private readonly SqlSugarConfig _sqlSugarConfig;
    private readonly string _connectionString;

    public TransientDbContext(SqlSugarConfig sqlSugarConfig, IConfiguration configuration)
    {
        _sqlSugarConfig = sqlSugarConfig;
        _connectionString = configuration.GetConnectionString("Transient")
            ?? "Data Source=./Transient.db;";
    }

    /// <summary>
    /// 创建指向独立库的客户端（与主库同样配置审计字段 AOP）
    /// </summary>
    public ISqlSugarClient CreateClient()
    {
        return _sqlSugarConfig.CreateClientFor(_connectionString);
    }

    /// <summary>
    /// 创建独立库表（启动时调用一次，新库自动建表）。
    /// 不用 CodeFirst：实体主键为 long（SqlSugar 映射 BIGINT），SQLite 的
    /// AUTOINCREMENT 仅允许 INTEGER PRIMARY KEY，因此直接使用与主库一致的 DDL。
    /// </summary>
    public void EnsureTablesCreated()
    {
        using var db = CreateClient();
        db.Ado.ExecuteCommand(@"
CREATE TABLE IF NOT EXISTS ""AppRssMirrorItem"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AppRssMirrorItem"" PRIMARY KEY AUTOINCREMENT,
    ""RssSourceId"" INTEGER NOT NULL,
    ""Title"" TEXT NOT NULL,
    ""Link"" TEXT NOT NULL,
    ""Description"" TEXT NULL,
    ""Author"" TEXT NULL,
    ""Category"" TEXT NULL,
    ""PublishDate"" TEXT NULL,
    ""Seeders"" INTEGER NULL,
    ""Leechers"" INTEGER NULL,
    ""Downloads"" INTEGER NULL,
    ""Extensions"" TEXT NULL,
    ""IsDownloaded"" INTEGER NOT NULL,
    ""DownloadTime"" TEXT NULL,
    ""CreationTime"" TEXT NOT NULL,
    ""LastModificationTime"" TEXT NULL,
    ""CreatorId"" TEXT NULL,
    ""LastModifierId"" TEXT NULL,
    ""ConcurrencyStamp"" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS ""AppRssWordSegment"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AppRssWordSegment"" PRIMARY KEY AUTOINCREMENT,
    ""RssMirrorItemId"" INTEGER NOT NULL,
    ""Word"" TEXT NOT NULL,
    ""LanguageType"" INTEGER NOT NULL,
    ""Count"" INTEGER NOT NULL,
    ""PartOfSpeech"" TEXT NULL,
    ""CreationTime"" TEXT NOT NULL,
    ""CreatorId"" TEXT NULL,
    ""ConcurrencyStamp"" TEXT NOT NULL DEFAULT ''
);
CREATE INDEX IF NOT EXISTS ""IX_AppRssMirrorItem_CreationTime"" ON ""AppRssMirrorItem"" (""CreationTime"");
CREATE INDEX IF NOT EXISTS ""IX_AppRssMirrorItem_IsDownloaded"" ON ""AppRssMirrorItem"" (""IsDownloaded"");
CREATE INDEX IF NOT EXISTS ""IX_AppRssMirrorItem_PublishDate"" ON ""AppRssMirrorItem"" (""PublishDate"");
CREATE INDEX IF NOT EXISTS ""IX_AppRssMirrorItem_RssSourceId"" ON ""AppRssMirrorItem"" (""RssSourceId"");
CREATE INDEX IF NOT EXISTS ""IX_AppRssWordSegment_Count"" ON ""AppRssWordSegment"" (""Count"");
CREATE INDEX IF NOT EXISTS ""IX_AppRssWordSegment_LanguageType"" ON ""AppRssWordSegment"" (""LanguageType"");
CREATE INDEX IF NOT EXISTS ""IX_AppRssWordSegment_RssMirrorItemId"" ON ""AppRssWordSegment"" (""RssMirrorItemId"");
CREATE INDEX IF NOT EXISTS ""IX_AppRssWordSegment_Word"" ON ""AppRssWordSegment"" (""Word"");
");
    }
}
