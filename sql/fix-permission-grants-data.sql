-- ============================================================================
-- 修复 AbpPermissionGrants 表数据格式问题
-- 创建时间: 2026-04-02
-- ============================================================================

-- ============================================================================
-- 背景说明
-- ============================================================================
-- AbpPermissionGrants 表存在两个数据格式问题：
--
-- 问题 1：角色级权限授予（ProviderName='R'）的 ProviderKey 存储的是角色名称
--          而非角色 GUID。ABP 框架原设计使用角色名作为 ProviderKey，
--          但新系统使用角色 ID 进行权限查询，因此需要修改。
--
-- 问题 2：用户级权限授予（ProviderName='U'）的 ProviderKey 是小写 GUID，
--          而 AbpUsers.Id 是大写 GUID。SQLite 字符串比较大小写敏感，
--          导致 JOIN 匹配失败。
--
-- 关于 ABP ProviderKey 设计的说明：
--   ABP 框架中，PermissionGrant 的 ProviderKey 对于角色（ProviderName='R'）
--   存储的确实是角色名（Name）而非角色 ID。这是 ABP Identity 的设计规范。
--
--   方案 A（已选择）：将角色名改为角色 ID
--     - 适用于：新系统使用角色 ID 查询权限
--     - 优点：与用户级权限保持一致，均使用 GUID 作为 ProviderKey
--     - 缺点：与 ABP 原生设计不兼容
--
--   方案 B（备选）：保持原样，修改代码适配角色名
--     - 适用于：保持 ABP 兼容性
--     - 优点：不需要修改数据库数据
--     - 缺点：需要代码中做额外的角色名到角色 ID 的转换
-- ============================================================================

.headers on
.mode column

-- ============================================================================
-- 第一阶段：修复前的数据检查
-- ============================================================================

SELECT '===== 修复前：角色级权限授予数据 =====';
SELECT ProviderKey AS '当前ProviderKey(角色名)', COUNT(*) AS '记录数'
FROM AbpPermissionGrants
WHERE ProviderName = 'R'
GROUP BY ProviderKey
ORDER BY ProviderKey;

SELECT '===== 修复前：用户级权限授予数据 =====';
SELECT ProviderKey AS '当前ProviderKey(用户GUID)', COUNT(*) AS '记录数'
FROM AbpPermissionGrants
WHERE ProviderName = 'U'
GROUP BY ProviderKey;

SELECT '===== 修复前：角色表参考数据 =====';
SELECT Id AS '角色ID(GUID)', Name AS '角色名' FROM AbpRoles ORDER BY Name;

SELECT '===== 修复前：用户表参考数据 =====';
SELECT Id AS '用户ID(GUID)', UserName AS '用户名' FROM AbpUsers ORDER BY UserName;

SELECT '===== 修复前：用户级 ProviderKey 与用户表 ID 的匹配检查 =====';
SELECT pg.ProviderKey AS '权限表ProviderKey', u.Id AS '用户表Id',
       CASE WHEN pg.ProviderKey = u.Id THEN '匹配' ELSE '不匹配(大小写问题)' END AS '匹配状态'
FROM AbpPermissionGrants pg
JOIN AbpUsers u ON LOWER(pg.ProviderKey) = LOWER(u.Id)
WHERE pg.ProviderName = 'U';

-- ============================================================================
-- 第二阶段：执行修复（使用事务）
-- ============================================================================

BEGIN TRANSACTION;

-- 方案 A：将角色名改为角色 ID
-- 使用 CASE 语句将每个角色名映射到对应的角色 GUID
UPDATE AbpPermissionGrants
SET ProviderKey = CASE
    WHEN ProviderKey = 'admin'                THEN '77BC54A8-D3C3-2FC7-9894-3A0FDC4DF8E9'
    WHEN ProviderKey = 'log'                  THEN 'ECBEA0A5-701A-B2AC-CDA5-3A0FE65B249B'
    WHEN ProviderKey = 'bookkeeping'          THEN 'C861A565-FDFC-78CF-67C4-3A100AF27165'
    WHEN ProviderKey = 'file'                 THEN 'C587B3E3-3184-4AF5-A35C-3A100B087923'
    WHEN ProviderKey = 'telegram'             THEN '77D85089-23FF-50EF-55DF-3A10121F414D'
    WHEN ProviderKey = 'cms'                  THEN '3C6F3B72-FC71-152A-9981-3A103ED82109'
    WHEN ProviderKey = 'Lottery'              THEN '7DF6DE33-2FC0-F64C-35AC-3A103ED95A9B'
    WHEN ProviderKey = 'IP'                   THEN 'C21E3C2B-667B-03AC-564D-3A103ED96DBE'
    WHEN ProviderKey = 'management_background' THEN 'A09D711B-1921-E748-EE7D-3A10D3F1B6F8'
    WHEN ProviderKey = 'down-ex'              THEN 'CB410581-3D19-62D8-7635-3A1130B3C76C'
    WHEN ProviderKey = 'aria2'                THEN '12294449-FC9E-C64A-28E9-3A1265DED8EB'
    WHEN ProviderKey = 'rss'                  THEN '03951B85-E4BA-93D7-AC0E-3A1ED466334F'
    WHEN ProviderKey = 'ElectricVehicle'      THEN 'CA14ECA5-AE0D-CF0C-A750-3A1F82344C6B'
    ELSE ProviderKey
END
WHERE ProviderName = 'R';

-- 修复用户级权限授予的 GUID 大小写问题
-- 将所有 ProviderName='U' 的 ProviderKey 统一转为大写
UPDATE AbpPermissionGrants
SET ProviderKey = UPPER(ProviderKey)
WHERE ProviderName = 'U' AND ProviderKey != UPPER(ProviderKey);

COMMIT;

-- ============================================================================
-- 第三阶段：修复后验证
-- ============================================================================

SELECT '===== 修复后：角色级权限授予数据 =====';
SELECT pg.ProviderKey AS 'ProviderKey(角色GUID)', r.Name AS '角色名', COUNT(*) AS '记录数'
FROM AbpPermissionGrants pg
LEFT JOIN AbpRoles r ON pg.ProviderKey = r.Id
WHERE pg.ProviderName = 'R'
GROUP BY pg.ProviderKey, r.Name
ORDER BY r.Name;

SELECT '===== 修复后：用户级权限授予数据 =====';
SELECT pg.ProviderKey AS 'ProviderKey(用户GUID)', u.UserName AS '用户名', COUNT(*) AS '记录数'
FROM AbpPermissionGrants pg
LEFT JOIN AbpUsers u ON pg.ProviderKey = u.Id
WHERE pg.ProviderName = 'U'
GROUP BY pg.ProviderKey, u.UserName
ORDER BY u.UserName;

SELECT '===== 修复后：所有角色权限的 JOIN 验证 =====';
SELECT
    CASE WHEN r.Id IS NOT NULL THEN '通过' ELSE '失败' END AS 'JOIN结果',
    COUNT(*) AS '记录数'
FROM AbpPermissionGrants pg
LEFT JOIN AbpRoles r ON pg.ProviderKey = r.Id
WHERE pg.ProviderName = 'R'
GROUP BY CASE WHEN r.Id IS NOT NULL THEN '通过' ELSE '失败' END;

SELECT '===== 修复后：所有用户权限的 JOIN 验证 =====';
SELECT
    CASE WHEN u.Id IS NOT NULL THEN '通过' ELSE '失败' END AS 'JOIN结果',
    COUNT(*) AS '记录数'
FROM AbpPermissionGrants pg
LEFT JOIN AbpUsers u ON pg.ProviderKey = u.Id
WHERE pg.ProviderName = 'U'
GROUP BY CASE WHEN u.Id IS NOT NULL THEN '通过' ELSE '失败' END;

SELECT '===== 修复完成 =====';
