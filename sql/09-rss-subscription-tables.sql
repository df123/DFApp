-- RSS订阅功能 - 数据库表创建脚本
-- 创建日期: 2026-02-24
-- 数据库: SQLite

-- ============================================
-- 表: AppRssSubscriptions (RSS订阅规则表)
-- ============================================
CREATE TABLE IF NOT EXISTS "AppRssSubscriptions" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Keywords" TEXT NOT NULL,
    "IsEnabled" INTEGER NOT NULL DEFAULT 1,
    "MinSeeders" INTEGER,
    "MaxSeeders" INTEGER,
    "MinLeechers" INTEGER,
    "MaxLeechers" INTEGER,
    "MinDownloads" INTEGER,
    "MaxDownloads" INTEGER,
    "QualityFilter" TEXT,
    "SubtitleGroupFilter" TEXT,
    "AutoDownload" INTEGER NOT NULL DEFAULT 1,
    "VideoOnly" INTEGER NOT NULL DEFAULT 0,
    "EnableKeywordFilter" INTEGER NOT NULL DEFAULT 0,
    "SavePath" TEXT,
    "RssSourceId" INTEGER,
    "StartDate" TEXT,
    "EndDate" TEXT,
    "Remark" TEXT,
    "CreationTime" TEXT NOT NULL,
    "LastModificationTime" TEXT,
    "ConcurrencyStamp" TEXT NOT NULL DEFAULT '',
    "CreatorId" TEXT,
    FOREIGN KEY ("RssSourceId") REFERENCES "AppRssSource" ("Id")
);

-- 索引
CREATE INDEX IF NOT EXISTS "IX_AppRssSubscriptions_IsEnabled" ON "AppRssSubscriptions" ("IsEnabled");
CREATE INDEX IF NOT EXISTS "IX_AppRssSubscriptions_RssSourceId" ON "AppRssSubscriptions" ("RssSourceId");
CREATE INDEX IF NOT EXISTS "IX_AppRssSubscriptions_CreationTime" ON "AppRssSubscriptions" ("CreationTime" DESC);

-- ============================================
-- 表: AppRssSubscriptionDownloads (RSS订阅下载记录表)
-- ============================================
CREATE TABLE IF NOT EXISTS "AppRssSubscriptionDownloads" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "SubscriptionId" INTEGER NOT NULL,
    "RssMirrorItemId" INTEGER NOT NULL,
    "Aria2Gid" TEXT NOT NULL,
    "DownloadStatus" INTEGER NOT NULL DEFAULT 0,
    "ErrorMessage" TEXT,
    "DownloadStartTime" TEXT,
    "DownloadCompleteTime" TEXT,
    "CreationTime" TEXT NOT NULL,
    "CreatorId" TEXT,
    FOREIGN KEY ("SubscriptionId") REFERENCES "AppRssSubscriptions" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RssMirrorItemId") REFERENCES "AppRssMirrorItem" ("Id") ON DELETE CASCADE
);

-- 索引
CREATE INDEX IF NOT EXISTS "IX_AppRssSubscriptionDownloads_SubscriptionId" ON "AppRssSubscriptionDownloads" ("SubscriptionId");
CREATE INDEX IF NOT EXISTS "IX_AppRssSubscriptionDownloads_RssMirrorItemId" ON "AppRssSubscriptionDownloads" ("RssMirrorItemId");
CREATE INDEX IF NOT EXISTS "IX_AppRssSubscriptionDownloads_DownloadStatus" ON "AppRssSubscriptionDownloads" ("DownloadStatus");
CREATE INDEX IF NOT EXISTS "IX_AppRssSubscriptionDownloads_Aria2Gid" ON "AppRssSubscriptionDownloads" ("Aria2Gid");

-- ============================================
-- 说明
-- ============================================
-- 下载状态 (DownloadStatus):
--   0 = 待下载
--   1 = 下载中
--   2 = 下载完成
--   3 = 下载失败
--
-- 级联删除:
--   - 删除订阅规则时，自动删除相关的下载记录
--   - 删除RSS镜像条目时，自动删除相关的下载记录
-- ============================================
