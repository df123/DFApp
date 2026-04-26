-- =============================================================
-- 修复 AuditedEntity 派生实体缺失的审计列（CreatorId, LastModifierId）
-- 幂等安全版：可安全地重复执行
--
-- 说明：部分 AuditedEntity 派生实体的数据库表在迁移时遗漏了审计列
--
-- 注意：AppPermissionGrants 表的审计列不在此脚本中添加。
--       该表由脚本 06 (06-migrate-to-app-permission-grants.sql) 创建，
--       创建时已包含 CreatorId、LastModificationTime、LastModifierId 列。
--       之前此脚本引用了尚未创建的 AppPermissionGrants 表导致报错。
--
-- 幂等性原理：
--   SQLite 不支持 ALTER TABLE ADD COLUMN IF NOT EXISTS 语法，
--   也不支持 SQL 中的 IF/ELSE 条件执行 DDL 语句。
--   本脚本使用以下技巧实现幂等性：
--   1. 通过 SELECT 'ALTER TABLE ...' WHERE (列不存在) 仅输出需要执行的语句
--   2. 将输出重定向到临时文件（.output）
--   3. 通过 .read 执行临时文件中的语句
--   已存在的列不会生成对应语句，从而避免 "duplicate column name" 错误。
-- =============================================================

.bail on
.headers off

-- ============================================================
-- 第一部分：检查当前状态
-- ============================================================

SELECT '=== 审计列迁移前状态检查 ===';

-- AppMediaInfo
SELECT 'AppMediaInfo:' || 
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppMediaInfo') WHERE name = 'CreatorId') > 0 THEN ' CreatorId=已存在' ELSE ' CreatorId=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppMediaInfo') WHERE name = 'LastModifierId') > 0 THEN ', LastModifierId=已存在' ELSE ', LastModifierId=缺失' END;

-- AppRssSubscriptions
SELECT 'AppRssSubscriptions:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptions') WHERE name = 'CreatorId') > 0 THEN ' CreatorId=已存在' ELSE ' CreatorId=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptions') WHERE name = 'LastModifierId') > 0 THEN ', LastModifierId=已存在' ELSE ', LastModifierId=缺失' END;

-- AppRssMirrorItem
SELECT 'AppRssMirrorItem:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssMirrorItem') WHERE name = 'CreatorId') > 0 THEN ' CreatorId=已存在' ELSE ' CreatorId=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssMirrorItem') WHERE name = 'LastModifierId') > 0 THEN ', LastModifierId=已存在' ELSE ', LastModifierId=缺失' END;

-- AbpRoleClaims
SELECT 'AbpRoleClaims:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'CreationTime') > 0 THEN ' CreationTime=已存在' ELSE ' CreationTime=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'CreatorId') > 0 THEN ', CreatorId=已存在' ELSE ', CreatorId=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'LastModificationTime') > 0 THEN ', LastModificationTime=已存在' ELSE ', LastModificationTime=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'LastModifierId') > 0 THEN ', LastModifierId=已存在' ELSE ', LastModifierId=缺失' END;

-- AbpRoles
SELECT 'AbpRoles:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'CreatorId') > 0 THEN ' CreatorId=已存在' ELSE ' CreatorId=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'LastModificationTime') > 0 THEN ', LastModificationTime=已存在' ELSE ', LastModificationTime=缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'LastModifierId') > 0 THEN ', LastModifierId=已存在' ELSE ', LastModifierId=缺失' END;


-- ============================================================
-- 第二部分：动态生成并执行 ALTER TABLE 语句
-- 仅对不存在的列生成 ALTER TABLE，已存在的列自动跳过
-- ============================================================

.output /tmp/_migration_04_steps.sql

-- AppMediaInfo（MediaInfo : AuditedEntity<long>）
SELECT 'ALTER TABLE "AppMediaInfo" ADD COLUMN "CreatorId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppMediaInfo') WHERE name = 'CreatorId') = 0;

SELECT 'ALTER TABLE "AppMediaInfo" ADD COLUMN "LastModifierId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppMediaInfo') WHERE name = 'LastModifierId') = 0;

-- AppRssSubscriptions（RssSubscription : AuditedEntity<long>）
-- CreatorId 已存在，仅补充 LastModifierId
SELECT 'ALTER TABLE "AppRssSubscriptions" ADD COLUMN "LastModifierId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptions') WHERE name = 'LastModifierId') = 0;

-- AppRssMirrorItem（RssMirrorItem : AuditedEntity<long>）
SELECT 'ALTER TABLE "AppRssMirrorItem" ADD COLUMN "CreatorId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppRssMirrorItem') WHERE name = 'CreatorId') = 0;

SELECT 'ALTER TABLE "AppRssMirrorItem" ADD COLUMN "LastModifierId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AppRssMirrorItem') WHERE name = 'LastModifierId') = 0;

-- AbpRoleClaims（RoleClaim : AuditedEntity<Guid>）
SELECT 'ALTER TABLE "AbpRoleClaims" ADD COLUMN "CreationTime" TEXT NOT NULL DEFAULT ''0001-01-01 00:00:00'';'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'CreationTime') = 0;

SELECT 'ALTER TABLE "AbpRoleClaims" ADD COLUMN "CreatorId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'CreatorId') = 0;

SELECT 'ALTER TABLE "AbpRoleClaims" ADD COLUMN "LastModificationTime" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'LastModificationTime') = 0;

SELECT 'ALTER TABLE "AbpRoleClaims" ADD COLUMN "LastModifierId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'LastModifierId') = 0;

-- AbpRoles（Role : AuditedEntity<Guid>）
-- CreationTime 已存在，仅补充 CreatorId 和修改审计列
SELECT 'ALTER TABLE "AbpRoles" ADD COLUMN "CreatorId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'CreatorId') = 0;

SELECT 'ALTER TABLE "AbpRoles" ADD COLUMN "LastModificationTime" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'LastModificationTime') = 0;

SELECT 'ALTER TABLE "AbpRoles" ADD COLUMN "LastModifierId" TEXT NULL;'
WHERE (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'LastModifierId') = 0;

.output stdout

-- 执行动态生成的 ALTER TABLE 语句
-- 如果所有列都已存在，临时文件为空，.read 不会执行任何操作
.read /tmp/_migration_04_steps.sql

-- 清理临时文件
.shell rm -f /tmp/_migration_04_steps.sql


-- ============================================================
-- 第三部分：迁移后验证
-- ============================================================

SELECT '=== 迁移后验证 ===';

SELECT 'AppMediaInfo:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppMediaInfo') WHERE name = 'CreatorId') > 0 THEN ' ✅CreatorId' ELSE ' ❌CreatorId缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppMediaInfo') WHERE name = 'LastModifierId') > 0 THEN ' ✅LastModifierId' ELSE ' ❌LastModifierId缺失' END;

SELECT 'AppRssSubscriptions:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptions') WHERE name = 'CreatorId') > 0 THEN ' ✅CreatorId' ELSE ' ❌CreatorId缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssSubscriptions') WHERE name = 'LastModifierId') > 0 THEN ' ✅LastModifierId' ELSE ' ❌LastModifierId缺失' END;

SELECT 'AppRssMirrorItem:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssMirrorItem') WHERE name = 'CreatorId') > 0 THEN ' ✅CreatorId' ELSE ' ❌CreatorId缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AppRssMirrorItem') WHERE name = 'LastModifierId') > 0 THEN ' ✅LastModifierId' ELSE ' ❌LastModifierId缺失' END;

SELECT 'AbpRoleClaims:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'CreationTime') > 0 THEN ' ✅CreationTime' ELSE ' ❌CreationTime缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'CreatorId') > 0 THEN ' ✅CreatorId' ELSE ' ❌CreatorId缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'LastModificationTime') > 0 THEN ' ✅LastModificationTime' ELSE ' ❌LastModificationTime缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoleClaims') WHERE name = 'LastModifierId') > 0 THEN ' ✅LastModifierId' ELSE ' ❌LastModifierId缺失' END;

SELECT 'AbpRoles:' ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'CreatorId') > 0 THEN ' ✅CreatorId' ELSE ' ❌CreatorId缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'LastModificationTime') > 0 THEN ' ✅LastModificationTime' ELSE ' ❌LastModificationTime缺失' END ||
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpRoles') WHERE name = 'LastModifierId') > 0 THEN ' ✅LastModifierId' ELSE ' ❌LastModifierId缺失' END;

SELECT '✅ 审计列迁移完成（幂等安全，可重复执行）';
