-- =============================================================
-- 添加磁盘空间暂存字段到订阅下载表
-- 幂等安全版：可安全地重复执行
--
-- 说明：为 AppRssSubscriptionDownloads 表添加 IsPendingDueToLowDiskSpace 列，
--       用于标记因磁盘空间不足而暂缓下载的任务。
--
-- 幂等性原理：
--   SQLite 不支持 ALTER TABLE ADD COLUMN IF NOT EXISTS 语法，
--   本脚本使用与 04-add-missing-audit-columns.sql 相同的技巧：
--   1. 通过 pragma_table_info 检查列是否存在
--   2. 仅在列不存在时生成 ALTER TABLE 语句到临时文件
--   3. 通过 .read 执行临时文件中的语句
-- =============================================================

.bail on
.headers off

-- ============================================================
-- 第一部分：检查当前状态
-- ============================================================

SELECT '=== 磁盘空间暂存字段迁移前状态检查 ===';

SELECT 'AppRssSubscriptionDownloads:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptionDownloads') WHERE name = 'IsPendingDueToLowDiskSpace') > 0 THEN ' IsPendingDueToLowDiskSpace=已存在' ELSE ' IsPendingDueToLowDiskSpace=缺失' END;


-- ============================================================
-- 第二部分：动态生成并执行 ALTER TABLE 语句
-- 仅对不存在的列生成 ALTER TABLE，已存在的列自动跳过
-- ============================================================

.output /tmp/_migration_10_steps.sql

SELECT 'ALTER TABLE "AppRssSubscriptionDownloads" ADD COLUMN "IsPendingDueToLowDiskSpace" INTEGER NOT NULL DEFAULT 0;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptionDownloads') WHERE name = 'IsPendingDueToLowDiskSpace') = 0;

.output stdout

-- 执行动态生成的 ALTER TABLE 语句
-- 如果列已存在，临时文件为空，.read 不会执行任何操作
.read /tmp/_migration_10_steps.sql

-- 清理临时文件
.shell rm -f /tmp/_migration_10_steps.sql


-- ============================================================
-- 第三部分：迁移后验证
-- ============================================================

SELECT '=== 迁移后验证 ===';

SELECT 'AppRssSubscriptionDownloads:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptionDownloads') WHERE name = 'IsPendingDueToLowDiskSpace') > 0 THEN ' ✅IsPendingDueToLowDiskSpace' ELSE ' ❌IsPendingDueToLowDiskSpace缺失' END;

SELECT '✅ 磁盘空间暂存字段迁移完成（幂等安全，可重复执行）';
