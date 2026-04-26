-- =============================================================
-- 迁移脚本：从 AbpPermissionGrants 迁移到 AppPermissionGrants
-- 日期：2026-04-09
-- 说明：创建新的简化权限表，从旧表迁移数据，解决 GUID 大小写问题
-- =============================================================

-- 1. 创建新表 AppPermissionGrants（如果不存在）
-- SqlSugar CodeFirst 会自动创建，此处作为备用方案
CREATE TABLE IF NOT EXISTS AppPermissionGrants (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    PermissionName  TEXT NOT NULL,
    ProviderType    TEXT NOT NULL CHECK (ProviderType IN ('Role', 'User')),
    ProviderKey     TEXT NOT NULL,
    CreationTime    TEXT NOT NULL DEFAULT (datetime('now'))
);

-- 创建索引以提高查询性能
CREATE INDEX IF NOT EXISTS idx_apg_provider ON AppPermissionGrants (ProviderType, ProviderKey);
CREATE INDEX IF NOT EXISTS idx_apg_permission ON AppPermissionGrants (PermissionName);

-- 2. 清理可能已存在的数据（幂等操作）
DELETE FROM AppPermissionGrants;

-- 3. 迁移用户级权限
-- 旧表 ProviderName='U', ProviderKey 为用户 GUID（大小写不一致）
-- 新表 ProviderType='User', ProviderKey 为用户 ID 字符串（统一小写）
INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey, CreationTime)
SELECT Name, 'User', lower(ProviderKey), datetime('now')
FROM AbpPermissionGrants
WHERE ProviderName = 'U';

-- 4. 迁移角色级权限（先直接迁移，再转换 ProviderKey）
-- 旧表 ProviderName='R', ProviderKey 为角色 GUID（大写）
-- 新表 ProviderType='Role', ProviderKey 应为角色名称
INSERT INTO AppPermissionGrants (PermissionName, ProviderType, ProviderKey, CreationTime)
SELECT Name, 'Role', ProviderKey, datetime('now')
FROM AbpPermissionGrants
WHERE ProviderName = 'R';

-- 5. 将角色级权限的 ProviderKey 从 GUID 转换为角色名称
-- 需要处理 GUID 大小写不一致问题，使用 UPPER() 匹配
UPDATE AppPermissionGrants
SET ProviderKey = (
    SELECT r.Name FROM AbpRoles r
    WHERE UPPER(r.Id) = UPPER(AppPermissionGrants.ProviderKey)
    LIMIT 1
)
WHERE ProviderType = 'Role'
AND EXISTS (
    SELECT 1 FROM AbpRoles r
    WHERE UPPER(r.Id) = UPPER(AppPermissionGrants.ProviderKey)
);

-- 6. 验证迁移结果
SELECT '=== 迁移结果统计 ===' AS info;
SELECT '旧表总数' AS label, COUNT(*) AS cnt FROM AbpPermissionGrants
UNION ALL
SELECT '新表总数', COUNT(*) FROM AppPermissionGrants
UNION ALL
SELECT '用户级权限(旧)', COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'U'
UNION ALL
SELECT '用户级权限(新)', COUNT(*) FROM AppPermissionGrants WHERE ProviderType = 'User'
UNION ALL
SELECT '角色级权限(旧)', COUNT(*) FROM AbpPermissionGrants WHERE ProviderName = 'R'
UNION ALL
SELECT '角色级权限(新)', COUNT(*) FROM AppPermissionGrants WHERE ProviderType = 'Role';

-- 7. 检查是否有未能转换 ProviderKey 的角色级权限
SELECT '=== 未转换的角色级权限（ProviderKey 仍为 GUID 格式） ===' AS info;
SELECT * FROM AppPermissionGrants
WHERE ProviderType = 'Role'
AND ProviderKey LIKE '%-%-%-%';  -- GUID 格式包含连字符
