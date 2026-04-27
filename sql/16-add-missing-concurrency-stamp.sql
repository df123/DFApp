-- =============================================================
-- 为缺失 ConcurrencyStamp 列的表补充该列
-- 幂等安全版：可安全地重复执行
--
-- 说明：
--   AppRssWordSegment、AppRssSubscriptionDownloads、AppMediaExternalLinkMediaIds
--   三个表的对应实体（RssWordSegment、RssSubscriptionDownload、
--   MediaExternalLinkMediaIds）继承自 EntityBase<long>，后者定义了
--   ConcurrencyStamp 列，但建表时遗漏了该列，导致查询时报错：
--   "no such column: ConcurrencyStamp"
--
-- 幂等性原理：
--   SQLite 不支持 ALTER TABLE ADD COLUMN IF NOT EXISTS 语法，
--   通过 pragma_table_info 检查列是否存在，仅对缺失的列生成
--   ALTER TABLE 语句，使用 .output 重定向 + .read 执行的方式。
-- =============================================================

.bail on
.headers off

-- ============================================================
-- 第一部分：检查当前状态
-- ============================================================

SELECT '=== ConcurrencyStamp 列迁移前状态检查 ===';

SELECT 'AppRssWordSegment:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssWordSegment') WHERE name = 'ConcurrencyStamp') > 0 THEN ' 已存在' ELSE ' 缺失' END;

SELECT 'AppRssSubscriptionDownloads:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptionDownloads') WHERE name = 'ConcurrencyStamp') > 0 THEN ' 已存在' ELSE ' 缺失' END;

SELECT 'AppMediaExternalLinkMediaIds:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppMediaExternalLinkMediaIds') WHERE name = 'ConcurrencyStamp') > 0 THEN ' 已存在' ELSE ' 缺失' END;


-- ============================================================
-- 第二部分：动态生成并执行 ALTER TABLE 语句
-- 仅对不存在的列生成 ALTER TABLE，已存在的列自动跳过
-- ============================================================

.output /tmp/_migration_16_steps.sql

-- AppRssWordSegment（RssWordSegment : CreationAuditedEntity<long> → EntityBase<long>）
SELECT 'ALTER TABLE "AppRssWordSegment" ADD COLUMN "ConcurrencyStamp" TEXT NOT NULL DEFAULT '''';'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppRssWordSegment') WHERE name = 'ConcurrencyStamp') = 0;

-- AppRssSubscriptionDownloads（RssSubscriptionDownload : CreationAuditedEntity<long> → EntityBase<long>）
SELECT 'ALTER TABLE "AppRssSubscriptionDownloads" ADD COLUMN "ConcurrencyStamp" TEXT NOT NULL DEFAULT '''';'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptionDownloads') WHERE name = 'ConcurrencyStamp') = 0;

-- AppMediaExternalLinkMediaIds（MediaExternalLinkMediaIds : EntityBase<long>）
SELECT 'ALTER TABLE "AppMediaExternalLinkMediaIds" ADD COLUMN "ConcurrencyStamp" TEXT NOT NULL DEFAULT '''';'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppMediaExternalLinkMediaIds') WHERE name = 'ConcurrencyStamp') = 0;

.output stdout

-- 执行动态生成的 ALTER TABLE 语句
-- 如果所有列都已存在，临时文件为空，.read 不会执行任何操作
.read /tmp/_migration_16_steps.sql

-- 清理临时文件
.shell rm -f /tmp/_migration_16_steps.sql


-- ============================================================
-- 第三部分：迁移后验证
-- ============================================================

SELECT '=== 迁移后验证 ===';

SELECT 'AppRssWordSegment:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssWordSegment') WHERE name = 'ConcurrencyStamp') > 0 THEN ' ✅ConcurrencyStamp' ELSE ' ❌ConcurrencyStamp缺失' END;

SELECT 'AppRssSubscriptionDownloads:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptionDownloads') WHERE name = 'ConcurrencyStamp') > 0 THEN ' ✅ConcurrencyStamp' ELSE ' ❌ConcurrencyStamp缺失' END;

SELECT 'AppMediaExternalLinkMediaIds:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppMediaExternalLinkMediaIds') WHERE name = 'ConcurrencyStamp') > 0 THEN ' ✅ConcurrencyStamp' ELSE ' ❌ConcurrencyStamp缺失' END;

SELECT '✅ ConcurrencyStamp 列迁移完成（幂等安全，可重复执行）';
