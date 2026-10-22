-- 下载器管理界面登录账号表（账号密码登录，替代原回环免认证 + X-Management-Token 方案）
-- 数据库：DFApp.Downloader 独立 SQLite（downloader.db，非主库）
-- 注：实际建表由 DownloaderDbContext.EnsureTablesCreated() 的 SqlSugar CodeFirst 自动完成（2026-10-22），
--     并在首次启动时种子默认管理员 admin/admin（可用环境变量 DFAPP_DOWNLOADER_ADMIN_PASSWORD 指定初始密码），
--     本文件仅作变更记录

CREATE TABLE IF NOT EXISTS Accounts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL,                  -- 用户名（当前仅 admin）
    PasswordHash TEXT NOT NULL,              -- PBKDF2-SHA256 口令哈希（Base64）
    PasswordSalt TEXT NOT NULL,              -- PBKDF2 盐（Base64）
    Iterations INTEGER NOT NULL,             -- 哈希迭代次数（100000）
    CreatedAt TEXT NOT NULL,                 -- 创建时间（UTC）
    UpdatedAt TEXT NOT NULL                  -- 更新时间（UTC）
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Accounts_Username
    ON Accounts (Username ASC);
