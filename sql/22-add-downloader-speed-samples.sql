-- 新增下载器全局速度采样表（仪表盘"速度记录"图表的数据源）
-- 数据库：DFApp.Downloader 独立 SQLite（downloader.db，非主库）
-- 注：实际建表由 DownloaderDbContext.EnsureTablesCreated() 的 SqlSugar CodeFirst 自动完成（2026-08-20），本文件仅作变更记录

CREATE TABLE IF NOT EXISTS DownloadSpeedSamples (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RecordedAt TEXT NOT NULL,               -- 采样时间（UTC）
    SpeedBytesPerSecond REAL NOT NULL       -- 采样时刻的全局总下载速度（字节/秒）
);

CREATE INDEX IF NOT EXISTS IX_DownloadSpeedSamples_RecordedAt
    ON DownloadSpeedSamples (RecordedAt ASC);
