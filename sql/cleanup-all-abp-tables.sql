-- ============================================================
-- ABP 框架遗留表统一清理脚本（Phase 8）
-- 用途：删除从 ABP Framework 迁移后所有不再使用的系统表
--
-- 【前置条件】
--   1. 已备份 DFApp.db（重要！此操作不可逆！）
--   2. 应用已停止运行，确保无活动连接占用数据库
--
-- 【执行方式】
--   sqlite3 DFApp.db < sql/cleanup-all-abp-tables.sql
--
-- 【保留的业务表】
--   以下业务表不会被删除，脚本会验证它们是否存在：
--   - AbpUsers          （用户表）
--   - AbpRoles          （角色表）
--   - AbpUserRoles      （用户-角色关联表）
--   - AbpPermissionGrants   （权限授予表）
--   - AbpPermissions        （权限定义表）
--   - AbpPermissionGroups   （权限分组表）
--   - AbpRoleClaims         （角色声明表）
--
-- 【即将删除的表（共 31 张）】
--   详见各部分注释
--
-- 注意：此操作不可逆！
-- ============================================================

-- ============================================================
-- 前置条件检查
-- ============================================================

SELECT '=== 第一步：前置条件检查 ===' AS section;

-- 检查业务表是否存在
SELECT
  CASE
    WHEN COUNT(*) = 7 THEN '✅ 所有业务表均存在，可以安全执行清理'
    ELSE '❌ 业务表缺失！请检查以下列表：'
  END AS check_result
FROM (
  SELECT name FROM sqlite_master WHERE type='table' AND name IN (
    'AbpUsers', 'AbpRoles', 'AbpUserRoles',
    'AbpPermissionGrants', 'AbpPermissions', 'AbpPermissionGroups',
    'AbpRoleClaims'
  )
);

-- 列出缺失的业务表（如果有的话）
SELECT '以下业务表不存在：' AS warning
WHERE NOT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='AbpUsers');

SELECT name || ' 不存在' AS warning
FROM (
  SELECT 'AbpUsers' AS name UNION ALL SELECT 'AbpRoles' UNION ALL SELECT 'AbpUserRoles'
  UNION ALL SELECT 'AbpPermissionGrants' UNION ALL SELECT 'AbpPermissions'
  UNION ALL SELECT 'AbpPermissionGroups' UNION ALL SELECT 'AbpRoleClaims'
)
WHERE name NOT IN (SELECT name FROM sqlite_master WHERE type='table');

-- ============================================================
-- 即将删除的表统计（行数检查）
-- ============================================================

SELECT '' AS '';
SELECT '=== 第二步：即将删除的表统计 ===' AS section;
SELECT '以下表将被删除，请确认数据情况：' AS info;

-- 创建一个临时视图来统计（SQLite 不支持 WITH RECURSIVE 做这种事，用子查询）

-- 第一部分：ABP Identity 废弃表
SELECT '【第一部分】ABP Identity 废弃表（无业务数据）' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'AbpClaimTypes' AS name UNION ALL SELECT 'AbpOrganizationUnits'
  UNION ALL SELECT 'AbpOrganizationUnitRoles' UNION ALL SELECT 'AbpUserClaims'
  UNION ALL SELECT 'AbpUserLogins' UNION ALL SELECT 'AbpUserOrganizationUnits'
  UNION ALL SELECT 'AbpUserTokens' UNION ALL SELECT 'AbpLinkUsers'
  UNION ALL SELECT 'AbpUserDelegations'
);

SELECT '' AS '';

-- 第二部分：ABP 安全日志相关表
SELECT '【第二部分】ABP 安全日志相关表' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'AbpSecurityLogs' AS name UNION ALL SELECT 'AbpSessions'
);

SELECT '' AS '';

-- 第三部分：ABP 审计日志相关表
SELECT '【第三部分】ABP 审计日志相关表' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'AbpAuditLogActions' AS name UNION ALL SELECT 'AbpEntityPropertyChanges'
  UNION ALL SELECT 'AbpEntityChanges' UNION ALL SELECT 'AbpAuditLogs'
  UNION ALL SELECT 'AbpAuditLogExcelFiles'
);

SELECT '' AS '';

-- 第四部分：ABP 多租户相关表
SELECT '【第四部分】ABP 多租户相关表' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'AbpTenantConnectionStrings' AS name UNION ALL SELECT 'AbpTenants'
);

SELECT '' AS '';

-- 第五部分：ABP 功能管理和设置相关表
SELECT '【第五部分】ABP 功能管理和设置相关表' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'AbpFeatureValues' AS name UNION ALL SELECT 'AbpFeatures'
  UNION ALL SELECT 'AbpFeatureGroups' UNION ALL SELECT 'AbpSettingValues'
  UNION ALL SELECT 'AbpSettings' UNION ALL SELECT 'AbpSettingDefinitions'
);

SELECT '' AS '';

-- 第六部分：ABP 后台任务相关表
SELECT '【第六部分】ABP 后台任务相关表' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'AbpBackgroundJobs' AS name
);

SELECT '' AS '';

-- 第七部分：ABP BLOB 存储相关表
SELECT '【第七部分】ABP BLOB 存储相关表' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'AbpBlobs' AS name UNION ALL SELECT 'AbpBlobContainers'
);

SELECT '' AS '';

-- 第八部分：OpenIddict 表
SELECT '【第八部分】OpenIddict 表（JWT 认证已改用自定义实现）' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT 'OpenIddictApplications' AS name UNION ALL SELECT 'OpenIddictAuthorizations'
  UNION ALL SELECT 'OpenIddictScopes' UNION ALL SELECT 'OpenIddictTokens'
);

SELECT '' AS '';

-- 第九部分：EF Core 迁移历史表
SELECT '【第九部分】EF Core 迁移历史表（已改用 SqlSugar）' AS category;
SELECT name AS table_name,
       CASE WHEN name IN (SELECT name FROM sqlite_master WHERE type='table') THEN '✓ 存在'
            ELSE '✗ 不存在' END AS status
FROM (
  SELECT '__EFMigrationsHistory' AS name
);

-- ============================================================
-- 开始事务：执行清理
-- ============================================================

BEGIN TRANSACTION;

SELECT '' AS '';
SELECT '=== 第三步：开始删除表（事务中）===' AS section;

-- ============================================================
-- 第一部分：ABP Identity 废弃表（无业务数据）
-- 这些表在当前系统中从未使用，可以安全删除
-- ============================================================

DROP TABLE IF EXISTS AbpClaimTypes;
DROP TABLE IF EXISTS AbpOrganizationUnits;
DROP TABLE IF EXISTS AbpOrganizationUnitRoles;
DROP TABLE IF EXISTS AbpUserClaims;
DROP TABLE IF EXISTS AbpUserLogins;
DROP TABLE IF EXISTS AbpUserOrganizationUnits;
DROP TABLE IF EXISTS AbpUserTokens;
DROP TABLE IF EXISTS AbpLinkUsers;
DROP TABLE IF EXISTS AbpUserDelegations;

-- ============================================================
-- 第二部分：ABP 安全日志相关表
-- 新系统使用 Serilog 文件日志替代 ABP 安全日志
-- ============================================================

DROP TABLE IF EXISTS AbpSecurityLogs;
DROP TABLE IF EXISTS AbpSessions;

-- ============================================================
-- 第三部分：ABP 审计日志相关表
-- 新系统改用 Serilog 文件日志，不再使用 ABP 审计日志
-- ============================================================

DROP TABLE IF EXISTS AbpAuditLogActions;
DROP TABLE IF EXISTS AbpEntityPropertyChanges;
DROP TABLE IF EXISTS AbpEntityChanges;
DROP TABLE IF EXISTS AbpAuditLogs;
DROP TABLE IF EXISTS AbpAuditLogExcelFiles;

-- ============================================================
-- 第四部分：ABP 多租户相关表
-- 新系统不再使用多租户功能
-- ============================================================

DROP TABLE IF EXISTS AbpTenantConnectionStrings;
DROP TABLE IF EXISTS AbpTenants;

-- ============================================================
-- 第五部分：ABP 功能管理和设置相关表
-- 新系统使用自定义 ConfigurationInfos 替代 ABP 设置系统
-- ============================================================

DROP TABLE IF EXISTS AbpFeatureValues;
DROP TABLE IF EXISTS AbpFeatures;
DROP TABLE IF EXISTS AbpFeatureGroups;
DROP TABLE IF EXISTS AbpSettingValues;
DROP TABLE IF EXISTS AbpSettings;
DROP TABLE IF EXISTS AbpSettingDefinitions;

-- ============================================================
-- 第六部分：ABP 后台任务相关表
-- 新系统使用 Quartz.NET 替代 ABP 后台任务
-- ============================================================

DROP TABLE IF EXISTS AbpBackgroundJobs;

-- ============================================================
-- 第七部分：ABP BLOB 存储相关表
-- ============================================================

DROP TABLE IF EXISTS AbpBlobs;
DROP TABLE IF EXISTS AbpBlobContainers;

-- ============================================================
-- 第八部分：OpenIddict 表
-- JWT 认证已改用自定义实现，不再依赖 OpenIddict
-- ============================================================

DROP TABLE IF EXISTS OpenIddictApplications;
DROP TABLE IF EXISTS OpenIddictAuthorizations;
DROP TABLE IF EXISTS OpenIddictScopes;
DROP TABLE IF EXISTS OpenIddictTokens;

-- ============================================================
-- 第九部分：EF Core 迁移历史表
-- 数据库 ORM 已从 EF Core 切换为 SqlSugar，迁移历史不再需要
-- ============================================================

DROP TABLE IF EXISTS __EFMigrationsHistory;

-- ============================================================
-- 提交事务
-- ============================================================

COMMIT;

-- ============================================================
-- 验证结果
-- ============================================================

SELECT '' AS '';
SELECT '=== 第四步：清理完成，验证结果 ===' AS section;
SELECT '✅ 所有 ABP 遗留表已删除' AS result;

-- 列出剩余的所有表
SELECT '' AS '';
SELECT '=== 当前剩余表列表 ===' AS section;
SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;
