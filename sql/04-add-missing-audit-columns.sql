-- =============================================================
-- 修复 AuditedEntity 派生实体缺失的审计列（CreatorId, LastModifierId）
-- 说明：部分 AuditedEntity 派生实体的数据库表在迁移时遗漏了审计列
--
-- 注意：AppPermissionGrants 表的审计列不在此脚本中添加。
--       该表由脚本 06 (06-migrate-to-app-permission-grants.sql) 创建，
--       创建时已包含 CreatorId、LastModificationTime、LastModifierId 列。
--       之前此脚本引用了尚未创建的 AppPermissionGrants 表导致报错。
-- =============================================================

.bail on

-- AppMediaInfo（MediaInfo : AuditedEntity<long>）
ALTER TABLE "AppMediaInfo" ADD COLUMN "CreatorId" TEXT NULL;
ALTER TABLE "AppMediaInfo" ADD COLUMN "LastModifierId" TEXT NULL;

-- AppRssSubscriptions（RssSubscription : AuditedEntity<long>）
-- CreatorId 已存在，仅补充 LastModifierId
ALTER TABLE "AppRssSubscriptions" ADD COLUMN "LastModifierId" TEXT NULL;

-- AppRssMirrorItem（RssMirrorItem : AuditedEntity<long>）
ALTER TABLE "AppRssMirrorItem" ADD COLUMN "CreatorId" TEXT NULL;
ALTER TABLE "AppRssMirrorItem" ADD COLUMN "LastModifierId" TEXT NULL;

-- AbpRoleClaims（RoleClaim : AuditedEntity<Guid>）
ALTER TABLE "AbpRoleClaims" ADD COLUMN "CreationTime" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';
ALTER TABLE "AbpRoleClaims" ADD COLUMN "CreatorId" TEXT NULL;
ALTER TABLE "AbpRoleClaims" ADD COLUMN "LastModificationTime" TEXT NULL;
ALTER TABLE "AbpRoleClaims" ADD COLUMN "LastModifierId" TEXT NULL;

-- AbpRoles（Role : AuditedEntity<Guid>）
-- CreationTime 已存在，仅补充 CreatorId 和修改审计列
ALTER TABLE "AbpRoles" ADD COLUMN "CreatorId" TEXT NULL;
ALTER TABLE "AbpRoles" ADD COLUMN "LastModificationTime" TEXT NULL;
ALTER TABLE "AbpRoles" ADD COLUMN "LastModifierId" TEXT NULL;
