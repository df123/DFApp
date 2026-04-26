-- ============================================================
-- 软删除用户清理脚本
-- 用途：删除 ABP 软删除机制遗留的用户数据及关联记录
-- 前置条件：已备份 DFApp.db，应用已停止运行
-- 执行方式：sqlite3 DFApp.db < sql/cleanup-soft-deleted-users.sql
-- 注意：此操作不可逆！请务必先备份数据库！
-- ============================================================
--
-- 背景：
--   项目已从 ABP Framework 迁移到轻量级 ASP.NET Core 架构，废弃了软删除机制。
--   AbpUsers 表中有 3 个被软删除（IsDeleted=1）的用户不应出现在查询结果中，
--   需要彻底清理这些用户及其关联数据。
--
--   待清理用户：
--     - cms   (885D267D-A2D2-4ADD-162E-3A0FDC9097BD) - 删除于 2024-09-26
--     - down  (1B445486-69AC-A272-345E-3A1130B4CE8B) - 删除于 2024-09-26
--     - cms2  (E14913AD-9E83-E536-C43A-3A146BB96DE4) - 删除于 2024-09-26
--
-- 执行顺序：
--   1. 前置检查（确认表存在、软删除用户数据）
--   2. 人工确认阶段（展示即将删除的数据）
--   3. 在事务中删除关联数据和用户数据
--   4. 验证删除结果
--
-- 与 migrate-abpusers-table.sql 的关系：
--   - 两个脚本互不依赖，可按任意顺序执行
--   - 如果本脚本在迁移脚本之后执行，迁移脚本已通过 WHERE IsDeleted=0 过滤
--     排除了这些软删除用户，因此本脚本无需额外处理
-- ============================================================


-- ============================================================
-- 第一部分：前置检查
-- ============================================================

SELECT '===== 软删除用户清理脚本 - 前置检查 =====' AS step;

-- 确认相关表存在
SELECT '检查相关表是否存在...' AS step;
SELECT CASE
    WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AbpUsers') > 0
     AND (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AbpUserRoles') > 0
     AND (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AbpPermissionGrants') > 0
    THEN '✅ 所有相关表存在，可以继续'
    ELSE '❌ 部分表不存在，脚本终止！请检查数据库状态。'
END AS result;

-- 确认 AbpUsers 表中仍存在 IsDeleted 列
-- （如果在 migrate-abpusers-table.sql 之后执行，该列已被移除，此处会提示）
SELECT '检查 IsDeleted 列是否存在...' AS step;
SELECT CASE
    WHEN COUNT(*) > 0 THEN '✅ IsDeleted 列存在'
    ELSE '⚠️ IsDeleted 列不存在。如果已执行 migrate-abpusers-table.sql，这些用户已被排除，无需再清理。'
END AS result
FROM pragma_table_info('AbpUsers') WHERE name = 'IsDeleted';


-- ============================================================
-- 第二部分：数据展示（人工确认）
-- ============================================================

SELECT '' AS blank;
SELECT '===== 以下数据将被删除（请人工确认）=====' AS step;

-- 展示待删除的用户
SELECT '--- 待删除的用户（IsDeleted=1）---' AS section;
SELECT
    Id,
    UserName,
    Email,
    IsDeleted,
    DeletionTime
FROM AbpUsers
WHERE IsDeleted = 1;

-- 展示待删除的用户角色关联
SELECT '--- 待删除的用户角色关联 ---' AS section;
SELECT
    ur.UserId,
    u.UserName,
    ur.RoleId
FROM AbpUserRoles ur
JOIN AbpUsers u ON ur.UserId = u.Id
WHERE u.IsDeleted = 1;

-- 展示待删除的权限授予记录
SELECT '--- 待删除的权限授予记录（ProviderName=U）---' AS section;
SELECT
    g.Id,
    g.ProviderName,
    g.ProviderKey,
    g.Name,
    u.UserName
FROM AbpPermissionGrants g
JOIN AbpUsers u ON g.ProviderKey = u.Id
WHERE g.ProviderName = 'U' AND u.IsDeleted = 1;


-- ============================================================
-- 第三部分：执行删除（事务）
-- ============================================================

SELECT '' AS blank;
SELECT '===== 开始执行删除操作（事务）=====' AS step;

BEGIN TRANSACTION;

-- 3.1 删除 AbpUserRoles 中属于软删除用户的记录
DELETE FROM AbpUserRoles
WHERE UserId IN (
    SELECT Id FROM AbpUsers WHERE IsDeleted = 1
);
SELECT '✅ AbpUserRoles 关联记录已清理，影响行数: ' || changes() AS result;

-- 3.2 删除 AbpPermissionGrants 中 ProviderName='U' 且属于软删除用户的记录
DELETE FROM AbpPermissionGrants
WHERE ProviderName = 'U'
  AND ProviderKey IN (
    SELECT Id FROM AbpUsers WHERE IsDeleted = 1
);
SELECT '✅ AbpPermissionGrants 记录已清理，影响行数: ' || changes() AS result;

-- 3.3 删除 AbpUsers 中的软删除用户记录
DELETE FROM AbpUsers
WHERE IsDeleted = 1;
SELECT '✅ AbpUsers 软删除用户已清理，影响行数: ' || changes() AS result;

-- 提交事务
COMMIT;

SELECT '✅ 事务已提交，所有删除操作完成' AS step;


-- ============================================================
-- 第四部分：验证删除结果
-- ============================================================

SELECT '' AS blank;
SELECT '===== 验证删除结果 =====' AS step;

-- 确认软删除用户已不存在
SELECT '检查 IsDeleted=1 的用户是否已清除...' AS step;
SELECT CASE
    WHEN (SELECT COUNT(*) FROM AbpUsers WHERE IsDeleted = 1) = 0
    THEN '✅ 所有软删除用户已清除'
    ELSE '❌ 仍有软删除用户残留，数量: ' || (SELECT COUNT(*) FROM AbpUsers WHERE IsDeleted = 1)
END AS result;

-- 确认这些用户在关联表中也不存在
SELECT '检查关联表中是否还有残留记录...' AS step;
SELECT
    'AbpUserRoles 中属于已删除用户的残留: '
    || (SELECT COUNT(*) FROM AbpUserRoles
        WHERE UserId IN ('885D267D-A2D2-4ADD-162E-3A0FDC9097BD',
                          '1B445486-69AC-A272-345E-3A1130B4CE8B',
                          'E14913AD-9E83-E536-C43A-3A146BB96DE4'))
    || ' 条' AS result1,
    'AbpPermissionGrants 中属于已删除用户的残留: '
    || (SELECT COUNT(*) FROM AbpPermissionGrants
        WHERE ProviderName = 'U'
          AND ProviderKey IN ('885D267D-A2D2-4ADD-162E-3A0FDC9097BD',
                               '1B445486-69AC-A272-345E-3A1130B4CE8B',
                               'E14913AD-9E83-E536-C43A-3A146BB96DE4'))
    || ' 条' AS result2;

-- 展示清理后剩余的用户列表
SELECT '' AS blank;
SELECT '--- 清理后剩余的用户列表 ---' AS section;
SELECT
    Id,
    UserName,
    Email,
    IsDeleted
FROM AbpUsers;

SELECT '===== 清理脚本执行完毕 =====' AS step;
