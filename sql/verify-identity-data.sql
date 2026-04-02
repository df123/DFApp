-- ============================================================
-- Identity 数据验证脚本
-- 用途：验证 ABP 框架迁移后用户/角色/权限数据完整性
-- 执行方式：sqlite3 DFApp.db < sql/verify-identity-data.sql
-- ============================================================

-- 1. 验证用户表数据
-- 检查用户总数
SELECT '=== 用户数据验证 ===' AS section;
SELECT COUNT(*) AS '用户总数' FROM AbpUsers;
SELECT COUNT(*) AS '活跃用户数' FROM AbpUsers WHERE IsActive = 1;
SELECT COUNT(*) AS '禁用用户数' FROM AbpUsers WHERE IsActive = 0;

-- 检查是否有用户缺失密码哈希（管理员除外）
SELECT COUNT(*) AS '缺失密码哈希的用户数' FROM AbpUsers WHERE PasswordHash IS NULL;

-- 2. 验证角色表数据
SELECT '=== 角色数据验证 ===' AS section;
SELECT COUNT(*) AS '角色总数' FROM AbpRoles;
SELECT Name AS '角色名称', IsDefault AS '是否默认角色', IsStatic AS '是否静态角色', IsPublic AS '是否公开角色' FROM AbpRoles;

-- 3. 验证用户-角色关联
SELECT '=== 用户-角色关联验证 ===' AS section;
SELECT COUNT(*) AS '用户角色关联总数' FROM AbpUserRoles;

-- 检查是否有孤儿关联（指向不存在的用户或角色）
SELECT COUNT(*) AS '孤儿关联（用户不存在）' FROM AbpUserRoles WHERE UserId NOT IN (SELECT Id FROM AbpUsers);
SELECT COUNT(*) AS '孤儿关联（角色不存在）' FROM AbpUserRoles WHERE RoleId NOT IN (SELECT Id FROM AbpRoles);

-- 每个用户的角色列表
SELECT u.UserName AS '用户名', r.Name AS '角色名'
FROM AbpUserRoles ur
JOIN AbpUsers u ON ur.UserId = u.Id
JOIN AbpRoles r ON ur.RoleId = r.Id
ORDER BY u.UserName, r.Name;

-- 4. 验证权限授予
SELECT '=== 权限授予验证 ===' AS section;
SELECT COUNT(*) AS '权限授予总数' FROM AbpPermissionGrants;
SELECT ProviderName AS '授予类型', COUNT(*) AS '数量' FROM AbpPermissionGrants GROUP BY ProviderName;
-- U = 用户直接授权, R = 角色授权

-- 检查用户级权限授予的有效性
SELECT COUNT(*) AS '无效用户权限（用户不存在）' FROM AbpPermissionGrants WHERE ProviderName = 'U' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpUsers);

-- 检查角色级权限授予的有效性
SELECT COUNT(*) AS '无效角色权限（角色不存在）' FROM AbpPermissionGrants WHERE ProviderName = 'R' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpRoles);

-- 5. 验证权限定义
SELECT '=== 权限定义验证 ===' AS section;
SELECT COUNT(*) AS '权限定义总数' FROM AbpPermissions;
SELECT GroupName AS '权限组', COUNT(*) AS '权限数', SUM(CASE WHEN IsEnabled = 1 THEN 1 ELSE 0 END) AS '已启用数' FROM AbpPermissions GROUP BY GroupName ORDER BY GroupName;

-- 检查是否有权限授予引用了不存在的权限定义
SELECT COUNT(*) AS '无效权限授予（权限不存在）' FROM AbpPermissionGrants WHERE Name NOT IN (SELECT Name FROM AbpPermissions WHERE IsEnabled = 1);

-- 6. 验证权限分组
SELECT '=== 权限分组验证 ===' AS section;
SELECT Name AS '分组名称', DisplayName AS '显示名称' FROM AbpPermissionGroups;

-- 7. 综合验证摘要
SELECT '=== 验证摘要 ===' AS section;
SELECT
    CASE
        WHEN (SELECT COUNT(*) FROM AbpUserRoles WHERE UserId NOT IN (SELECT Id FROM AbpUsers)) = 0
         AND (SELECT COUNT(*) FROM AbpUserRoles WHERE RoleId NOT IN (SELECT Id FROM AbpRoles)) = 0
         AND (SELECT COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'U' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpUsers)) = 0
         AND (SELECT COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'R' AND ProviderKey NOT IN (SELECT CAST(Id AS TEXT) FROM AbpRoles)) = 0
        THEN '✅ 所有数据验证通过'
        ELSE '❌ 存在数据完整性问题，请检查上方详细信息'
    END AS '验证结果';
