-- 添加磁盘空间暂存字段到订阅下载表
ALTER TABLE "AppRssSubscriptionDownloads" ADD COLUMN "IsPendingDueToLowDiskSpace" INTEGER NOT NULL DEFAULT 0;
