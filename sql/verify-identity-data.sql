-- ============================================================
-- Identity 数据完整性验证脚本
-- 用途：验证 ABP Framework 迁移后用户/角色/权限数据的完整性
-- ============================================================
--
-- 使用说明：
--   1. 确保后端应用已停止运行（避免数据库锁定）
--   2. 在项目根目录执行以下命令：
--      sqlite3 DFApp.db < sql/verify-identity-data.sql
--   3. 检查输出中是否有 ❌ 标记的问题
--
-- 背景：
--   项目已从 ABP Framework 迁移到轻量级 ASP.NET Core 架构。
--   本脚本用于验证迁移后的数据完整性，包括：
--     - 用户数据（软删除清理、表结构精简）
--     - 角色、权限、关联数据的一致性
--     - 多租户数据的清除
--
-- 前置脚本：
--   - cleanup-soft-deleted-users.sql（清理 3 个软删除用户）
--   - migrate-abpusers-table.sql（精简 AbpUsers 表结构，移除 21 列）
--
-- 注意事项：
--   本脚本为只读查询，不会修改任何数据，可安全重复执行。
--   部分检查会自适应判断：如果迁移脚本已执行（如 IsDeleted 列已移除），
--   对应的检查会自动跳过并提示。
-- ============================================================


-- ============================================================
-- 第一部分：用户表数据验证
-- ============================================================

SELECT '===== 第一部分：用户表数据验证 =====' AS step;

-- 1.1 用户基本统计
SELECT '--- 用户基本统计 ---' AS section;
SELECT COUNT(*) AS '用户总数' FROM AbpUsers;
SELECT COUNT(*) AS '活跃用户数' FROM AbpUsers WHERE IsActive = 1;
SELECT COUNT(*) AS '禁用用户数' FROM AbpUsers WHERE IsActive = 0;

-- 1.2 软删除检查
--    迁移脚本 migrate-abpusers-table.sql 会移除 IsDeleted 列，
--    如果该列仍存在则检查是否有残留的软删除数据
SELECT '--- 软删除检查 ---' AS section;
SELECT CASE
    WHEN COUNT(*) = 0 THEN '⚠️ IsDeleted 列不存在（已执行表结构迁移，此项跳过）'
    ELSE '检查软删除数据...'
END AS status
FROM pragma_table_info('AbpUsers') WHERE name = 'IsDeleted';

SELECT COUNT(*) AS '软删除用户数（IsDeleted=1）'
FROM AbpUsers
WHERE name = 'IsDeleted' AND (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'IsDeleted') > 0
AND IsDeleted = 1;

-- 使用子查询方式避免列不存在时报错
SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'IsDeleted') = 0
        THEN '✅ IsDeleted 列已移除，无软删除数据'
        WHEN (SELECT COUNT(*) FROM AbpUsers WHERE IsDeleted = 1) = 0
        THEN '✅ 无软删除用户'
        ELSE '❌ 仍有软删除用户残留，数量: ' || (SELECT COUNT(*) FROM AbpUsers WHERE IsDeleted = 1)
    END AS '软删除验证结果';

-- 1.3 多租户检查
--    迁移脚本会移除 TenantId 列，如果该列仍存在则检查是否有多租户数据残留
SELECT '--- 多租户检查 ---' AS section;
SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'TenantId') = 0
        THEN '✅ TenantId 列已移除，无多租户数据'
        WHEN (SELECT COUNT(*) FROM AbpUsers WHERE TenantId IS NOT NULL) = 0
        THEN '✅ 所有用户 TenantId 均为 NULL，无多租户数据'
        ELSE '❌ 存在多租户用户数据，数量: ' || (SELECT COUNT(*) FROM AbpUsers WHERE TenantId IS NOT NULL)
    END AS '多租户验证结果';

-- 1.4 密码哈希检查
--    所有用户都应设置密码哈希
SELECT '--- 密码哈希检查 ---' AS section;
SELECT COUNT(*) AS '缺失密码哈希的用户数' FROM AbpUsers WHERE PasswordHash IS NULL OR PasswordHash = '';
SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM AbpUsers WHERE PasswordHash IS NULL OR PasswordHash = '') = 0
        THEN '✅ 所有用户均有密码哈希'
        ELSE '❌ 存在缺失密码哈希的用户'
    END AS '密码哈希验证结果';

-- 1.5 用户名重复检查
SELECT '--- 用户名重复检查 ---' AS section;
SELECT UserName AS '重复用户名', COUNT(*) AS '出现次数'
FROM AbpUsers
GROUP BY UserName
HAVING COUNT(*) > 1;

SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM (
            SELECT UserName FROM AbpUsers GROUP BY UserName HAVING COUNT(*) > 1
        )) = 0
        THEN '✅ 无重复用户名'
        ELSE '❌ 存在重复用户名'
    END AS '用户名唯一性验证结果';

-- 1.6 用户详情列表
SELECT '--- 用户详情列表 ---' AS section;
SELECT
    UserName AS '用户名',
    Email AS '邮箱',
    IsActive AS '是否激活',
    CreationTime AS '创建时间',
    CASE WHEN PasswordHash IS NULL OR PasswordHash = '' THEN '❌ 缺失' ELSE '✅ 已设置' END AS '密码状态'
FROM AbpUsers
ORDER BY CreationTime;


-- ============================================================
-- 第二部分：用户表结构验证
-- ============================================================

SELECT '' AS blank;
SELECT '===== 第二部分：用户表结构验证 =====' AS step;

-- 2.1 列出 AbpUsers 表所有列
SELECT '--- AbpUsers 表结构 ---' AS section;
SELECT name AS '列名', type AS '数据类型', `notnull` AS '非空约束', dflt_value AS '默认值'
FROM pragma_table_info('AbpUsers')
ORDER BY cid;

-- 2.2 验证列数
--    迁移后应为 10 列（Id, ConcurrencyStamp, CreationTime, CreatorId,
--    LastModificationTime, LastModifierId, UserName, Email, PasswordHash, IsActive）
SELECT '--- 列数验证 ---' AS section;
SELECT
    (SELECT COUNT(*) FROM pragma_table_info('AbpUsers')) AS '实际列数',
    10 AS '预期列数（迁移后）',
    CASE
        WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers')) = 10
        THEN '✅ 列数正确'
        ELSE '❌ 列数不正确'
    END AS '列数验证结果';

-- 2.3 验证必要列存在
SELECT '--- 必要列存在性检查 ---' AS section;
SELECT
    CASE WHEN MAX(CASE WHEN name = 'Id' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'Id',
    CASE WHEN MAX(CASE WHEN name = 'UserName' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'UserName',
    CASE WHEN MAX(CASE WHEN name = 'Email' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'Email',
    CASE WHEN MAX(CASE WHEN name = 'PasswordHash' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'PasswordHash',
    CASE WHEN MAX(CASE WHEN name = 'IsActive' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'IsActive',
    CASE WHEN MAX(CASE WHEN name = 'ConcurrencyStamp' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'ConcurrencyStamp',
    CASE WHEN MAX(CASE WHEN name = 'CreationTime' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'CreationTime',
    CASE WHEN MAX(CASE WHEN name = 'CreatorId' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'CreatorId',
    CASE WHEN MAX(CASE WHEN name = 'LastModificationTime' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'LastModificationTime',
    CASE WHEN MAX(CASE WHEN name = 'LastModifierId' THEN 1 ELSE 0 END) = 1 THEN '✅' ELSE '❌' END AS 'LastModifierId'
FROM pragma_table_info('AbpUsers');


-- ============================================================
-- 第三部分：角色表数据验证
-- ============================================================

SELECT '' AS blank;
SELECT '===== 第三部分：角色表数据验证 =====' AS step;

-- 3.1 角色基本统计
SELECT '--- 角色基本统计 ---' AS section;
SELECT COUNT(*) AS '角色总数' FROM AbpRoles;

-- 3.2 角色详情列表
SELECT '--- 角色详情列表 ---' AS section;
SELECT
    Name AS '角色名称',
    CASE WHEN IsDefault = 1 THEN '是' ELSE '否' END AS '是否默认角色',
    CASE WHEN IsStatic = 1 THEN '是' ELSE '否' END AS '是否静态角色',
    CASE WHEN IsPublic = 1 THEN '是' ELSE '否' END AS '是否公开角色'
FROM AbpRoles
ORDER BY Name;


-- ============================================================
-- 第四部分：用户-角色关联验证
-- ============================================================

SELECT '' AS blank;
SELECT '===== 第四部分：用户-角色关联验证 =====' AS step;

-- 4.1 关联基本统计
SELECT '--- 关联基本统计 ---' AS section;
SELECT COUNT(*) AS '用户角色关联总数' FROM AbpUserRoles;

-- 4.2 孤儿关联检查（指向不存在的用户或角色）
SELECT '--- 孤儿关联检查 ---' AS section;
SELECT COUNT(*) AS '孤儿关联（用户不存在）' FROM AbpUserRoles WHERE UserId NOT IN (SELECT Id FROM AbpUsers);
SELECT COUNT(*) AS '孤儿关联（角色不存在）' FROM AbpUserRoles WHERE RoleId NOT IN (SELECT Id FROM AbpRoles);

SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM AbpUserRoles WHERE UserId NOT IN (SELECT Id FROM AbpUsers)) = 0
         AND (SELECT COUNT(*) FROM AbpUserRoles WHERE RoleId NOT IN (SELECT Id FROM AbpRoles)) = 0
        THEN '✅ 无孤儿关联'
        ELSE '❌ 存在孤儿关联'
    END AS '孤儿关联验证结果';

-- 4.3 每个用户的角色分配明细
SELECT '--- 用户角色分配明细 ---' AS section;
SELECT u.UserName AS '用户名', r.Name AS '角色名'
FROM AbpUserRoles ur
JOIN AbpUsers u ON ur.UserId = u.Id
JOIN AbpRoles r ON ur.RoleId = r.Id
ORDER BY u.UserName, r.Name;

-- 4.4 检查是否有用户未被分配任何角色
SELECT '--- 未分配角色的用户 ---' AS section;
SELECT u.UserName AS '未分配角色的用户'
FROM AbpUsers u
LEFT JOIN AbpUserRoles ur ON u.Id = ur.UserId
WHERE ur.UserId IS NULL;

SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM AbpUsers u LEFT JOIN AbpUserRoles ur ON u.Id = ur.UserId WHERE ur.UserId IS NULL) = 0
        THEN '✅ 所有用户均已分配角色'
        ELSE '❌ 存在未分配角色的用户'
    END AS '角色分配验证结果';


-- ============================================================
-- 第五部分：权限授予验证
-- ============================================================

SELECT '' AS blank;
SELECT '===== 第五部分：权限授予验证 =====' AS step;

-- 5.1 权限授予基本统计
SELECT '--- 权限授予基本统计 ---' AS section;
SELECT COUNT(*) AS '权限授予总数' FROM AbpPermissionGrants;
SELECT ProviderName AS '授予类型', COUNT(*) AS '数量'
FROM AbpPermissionGrants
GROUP BY ProviderName;

-- 5.2 用户级权限授予有效性检查
--    ProviderName='U' 的记录，ProviderKey 应引用有效的用户 Id
SELECT '--- 用户级权限授予有效性（ProviderName=U）---' AS section;
SELECT COUNT(*) AS '无效用户权限（用户不存在）'
FROM AbpPermissionGrants
WHERE ProviderName = 'U'
  AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpUsers);

SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM AbpPermissionGrants
              WHERE ProviderName = 'U'
                AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpUsers)) = 0
        THEN '✅ 所有用户级权限授予均引用有效用户'
        ELSE '❌ 存在引用无效用户的权限授予'
    END AS '用户权限验证结果';

-- 5.3 角色级权限授予有效性检查
--    ProviderName='R' 的记录，ProviderKey 应引用有效的角色 Id
SELECT '--- 角色级权限授予有效性（ProviderName=R）---' AS section;
SELECT COUNT(*) AS '无效角色权限（角色不存在）'
FROM AbpPermissionGrants
WHERE ProviderName = 'R'
  AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpRoles);

SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM AbpPermissionGrants
              WHERE ProviderName = 'R'
                AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpRoles)) = 0
        THEN '✅ 所有角色级权限授予均引用有效角色'
        ELSE '❌ 存在引用无效角色的权限授予'
    END AS '角色权限验证结果';

-- 5.4 权限名称有效性检查
--    权限授予引用的权限名称应存在于已启用的权限定义中
SELECT '--- 权限名称有效性 ---' AS section;
SELECT COUNT(*) AS '无效权限授予（权限定义不存在或未启用）'
FROM AbpPermissionGrants
WHERE Name NOT IN (SELECT Name FROM AbpPermissions WHERE IsEnabled = 1);

SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM AbpPermissionGrants
              WHERE Name NOT IN (SELECT Name FROM AbpPermissions WHERE IsEnabled = 1)) = 0
        THEN '✅ 所有权限授予均引用有效权限定义'
        ELSE '❌ 存在引用无效权限定义的权限授予'
    END AS '权限定义验证结果';


-- ============================================================
-- 第六部分：权限定义验证
-- ============================================================

SELECT '' AS blank;
SELECT '===== 第六部分：权限定义验证 =====' AS step;

-- 6.1 权限定义基本统计
SELECT '--- 权限定义基本统计 ---' AS section;
SELECT COUNT(*) AS '权限定义总数' FROM AbpPermissions;

-- 6.2 按权限分组统计
SELECT '--- 按权限分组统计 ---' AS section;
SELECT
    GroupName AS '权限组',
    COUNT(*) AS '权限总数',
    SUM(CASE WHEN IsEnabled = 1 THEN 1 ELSE 0 END) AS '已启用数',
    SUM(CASE WHEN IsEnabled = 0 THEN 1 ELSE 0 END) AS '已禁用数'
FROM AbpPermissions
GROUP BY GroupName
ORDER BY GroupName;


-- ============================================================
-- 第七部分：权限分组验证
-- ============================================================

SELECT '' AS blank;
SELECT '===== 第七部分：权限分组验证 =====' AS step;

-- 7.1 权限分组列表
SELECT '--- 权限分组列表 ---' AS section;
SELECT Name AS '分组名称', DisplayName AS '显示名称' FROM AbpPermissionGroups ORDER BY Name;

-- 7.2 检查权限定义中的 GroupName 是否都有对应的分组
SELECT '--- 权限分组一致性检查 ---' AS section;
SELECT DISTINCT p.GroupName AS '未在 AbpPermissionGroups 中定义的分组'
FROM AbpPermissions p
LEFT JOIN AbpPermissionGroups g ON p.GroupName = g.Name
WHERE g.Name IS NULL;

SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM (
            SELECT DISTINCT p.GroupName
            FROM AbpPermissions p
            LEFT JOIN AbpPermissionGroups g ON p.GroupName = g.Name
            WHERE g.Name IS NULL
        )) = 0
        THEN '✅ 所有权限分组均有对应定义'
        ELSE '❌ 存在未定义的权限分组'
    END AS '权限分组一致性验证结果';


-- ============================================================
-- 第八部分：综合验证摘要
-- ============================================================

SELECT '' AS blank;
SELECT '===== 第八部分：综合验证摘要 =====' AS step;

-- 8.1 孤儿关联汇总
SELECT '--- 孤儿关联汇总 ---' AS section;
SELECT
    (SELECT COUNT(*) FROM AbpUserRoles WHERE UserId NOT IN (SELECT Id FROM AbpUsers)) AS '用户角色孤儿关联数',
    (SELECT COUNT(*) FROM AbpUserRoles WHERE RoleId NOT IN (SELECT Id FROM AbpRoles)) AS '角色关联孤儿数',
    (SELECT COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'U' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpUsers)) AS '无效用户权限数',
    (SELECT COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'R' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpRoles)) AS '无效角色权限数',
    (SELECT COUNT(*) FROM AbpPermissionGrants WHERE Name NOT IN (SELECT Name FROM AbpPermissions WHERE IsEnabled = 1)) AS '无效权限定义数';

-- 8.2 用户数据完整性汇总
SELECT '--- 用户数据完整性汇总 ---' AS section;
SELECT
    (SELECT COUNT(*) FROM AbpUsers) AS '用户总数',
    (SELECT COUNT(*) FROM AbpUsers WHERE IsActive = 0) AS '禁用用户数',
    (SELECT COUNT(*) FROM AbpUsers WHERE PasswordHash IS NULL OR PasswordHash = '') AS '缺失密码用户数',
    (SELECT COUNT(*) FROM (
        SELECT UserName FROM AbpUsers GROUP BY UserName HAVING COUNT(*) > 1
    )) AS '重复用户名数';

-- 8.3 迁移状态检查
SELECT '--- 迁移状态检查 ---' AS section;
SELECT
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'IsDeleted') = 0
        THEN '✅ 已迁移'
        ELSE '❌ 未迁移（IsDeleted 列仍存在）'
    END AS '表结构迁移状态',
    CASE WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'TenantId') = 0
        THEN '✅ 已清理'
        ELSE CASE WHEN (SELECT COUNT(*) FROM AbpUsers WHERE TenantId IS NOT NULL) = 0
            THEN '✅ 已清理（列存在但值为空）'
            ELSE '❌ 存在多租户数据'
        END
    END AS '多租户数据清理状态';

-- 8.4 最终验证结论
SELECT '' AS blank;
SELECT '===== 最终验证结论 =====' AS step;
SELECT CASE
    -- 孤儿关联全部为零
    WHEN (SELECT COUNT(*) FROM AbpUserRoles WHERE UserId NOT IN (SELECT Id FROM AbpUsers)) > 0
    THEN '❌ 数据完整性存在问题：存在用户角色孤儿关联'
    WHEN (SELECT COUNT(*) FROM AbpUserRoles WHERE RoleId NOT IN (SELECT Id FROM AbpRoles)) > 0
    THEN '❌ 数据完整性存在问题：存在角色关联孤儿'
    WHEN (SELECT COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'U' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpUsers)) > 0
    THEN '❌ 数据完整性存在问题：存在引用无效用户的权限授予'
    WHEN (SELECT COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'R' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpRoles)) > 0
    THEN '❌ 数据完整性存在问题：存在引用无效角色的权限授予'
    WHEN (SELECT COUNT(*) FROM AbpPermissionGrants WHERE Name NOT IN (SELECT Name FROM AbpPermissions WHERE IsEnabled = 1)) > 0
    THEN '❌ 数据完整性存在问题：存在引用无效权限定义的权限授予'
    -- 用户数据完整性
    WHEN (SELECT COUNT(*) FROM AbpUsers WHERE PasswordHash IS NULL OR PasswordHash = '') > 0
    THEN '❌ 数据完整性存在问题：存在缺失密码哈希的用户'
    WHEN (SELECT COUNT(*) FROM (
        SELECT UserName FROM AbpUsers GROUP BY UserName HAVING COUNT(*) > 1
    )) > 0
    THEN '❌ 数据完整性存在问题：存在重复用户名'
    -- 迁移残留检查（仅在列仍存在时才报错）
    WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'IsDeleted') > 0
     AND (SELECT COUNT(*) FROM AbpUsers WHERE IsDeleted = 1) > 0
    THEN '❌ 数据完整性存在问题：存在软删除用户残留'
    WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'TenantId') > 0
     AND (SELECT COUNT(*) FROM AbpUsers WHERE TenantId IS NOT NULL) > 0
    THEN '❌ 数据完整性存在问题：存在多租户数据残留'
    -- 所有检查通过
    ELSE '✅ 所有数据验证通过，数据完整性良好'
END AS '验证结论';

SELECT '===== 验证脚本执行完毕 =====' AS step;
