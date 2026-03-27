-- ============================================
-- Identity模块实体迁移到自定义基类
-- Phase 2.3
-- 迁移日期: 2026-03-27
-- ============================================
-- 说明：
-- 本SQL文件记录了Identity模块6个实体从ABP Identity基类迁移到自定义基类的变更
-- 由于所有字段名称保持不变，数据库结构无需修改
-- ============================================

-- 1. AbpRoles 表
-- 实体：Role
-- 变更：基类从 AbpRole<Guid> 改为 AuditedEntity<Guid>
-- 影响：无，字段名称完全一致
-- 说明：
--   - 基类提供字段：Id, CreationTime, LastModificationTime, ConcurrencyStamp
--   - 业务字段：TenantId, Name, NormalizedName, IsDefault, IsStatic, IsPublic, EntityVersion, ExtraProperties
--   - 注意：CreatorId, LastModificationTime, LastModifierId 字段在实体中被忽略（IsIgnore = true）
--   - 这些字段在数据库中可能不存在，需要通过代码逻辑处理

-- 2. AbpUserRoles 表
-- 实体：UserRole
-- 变更：从 IdentityUserRole<Guid> 改为普通类（无基类）
-- 影响：无，字段名称完全一致
-- 说明：
--   - 复合主键：UserId, RoleId
--   - 业务字段：TenantId
--   - 无审计字段

-- 3. AbpPermissionGrants 表
-- 实体：PermissionGrant
-- 变更：基类从 PermissionGrant 改为 AuditedEntity<Guid>
-- 影响：无，字段名称完全一致
-- 说明：
--   - 基类提供字段：Id, CreationTime, LastModificationTime, ConcurrencyStamp
--   - 业务字段：TenantId, Name, ProviderName, ProviderKey
--   - 注意：所有审计字段在实体中被忽略（IsIgnore = true）
--   - 这些字段在数据库中可能不存在，需要通过代码逻辑处理

-- 4. AbpPermissions 表
-- 实体：Permission
-- 变更：基类从 Permission 改为 CreationAuditedEntity<Guid>
-- 影响：无，字段名称完全一致
-- 说明：
--   - 基类提供字段：Id, CreationTime, CreatorId, ConcurrencyStamp
--   - 业务字段：GroupName, Name, ParentName, DisplayName, IsEnabled, MultiTenancySide, Providers, StateCheckers, ExtraProperties
--   - 注意：所有审计字段在实体中被忽略（IsIgnore = true）
--   - TenantId 字段在实体中被忽略（IsIgnore = true）

-- 5. AbpPermissionGroups 表
-- 实体：PermissionGroup
-- 变更：基类从 PermissionGroup 改为 Entity
-- 影响：无，字段名称完全一致
-- 说明：
--   - 基类提供字段：Id, ConcurrencyStamp
--   - 业务字段：Name, DisplayName, ExtraProperties
--   - 注意：ConcurrencyStamp 字段在实体中被忽略（IsIgnore = true）

-- 6. AbpRoleClaims 表
-- 实体：RoleClaim
-- 变更：基类从 IdentityRoleClaim<Guid> 改为 AuditedEntity<Guid>
-- 影响：无，字段名称完全一致
-- 说明：
--   - 基类提供字段：Id, CreationTime, LastModificationTime, ConcurrencyStamp
--   - 业务字段：RoleId, TenantId, ClaimType, ClaimValue
--   - 注意：所有审计字段在实体中被忽略（IsIgnore = true）
--   - Role 导航属性在实体中被忽略（IsIgnore = true）

-- ============================================
-- 验证脚本（可选）
-- ============================================

-- 验证所有表的存在
SELECT 
    'AbpRoles' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('AbpRoles')
UNION ALL
SELECT 
    'AbpUserRoles' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('AbpUserRoles')
UNION ALL
SELECT 
    'AbpPermissionGrants' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('AbpPermissionGrants')
UNION ALL
SELECT 
    'AbpPermissions' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('AbpPermissions')
UNION ALL
SELECT 
    'AbpPermissionGroups' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('AbpPermissionGroups')
UNION ALL
SELECT 
    'AbpRoleClaims' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('AbpRoleClaims');

-- 验证 AbpRoles 表的关键字段
SELECT 
    name AS ColumnName,
    type AS DataType,
    'notnull' AS NotNull
FROM pragma_table_info('AbpRoles')
WHERE name IN ('Id', 'TenantId', 'Name', 'NormalizedName', 'IsDefault', 'IsStatic', 'IsPublic', 'EntityVersion', 'ExtraProperties', 'CreationTime', 'LastModificationTime', 'ConcurrencyStamp')
ORDER BY cid;

-- 验证 AbpUserRoles 表的关键字段
SELECT 
    name AS ColumnName,
    type AS DataType,
    pk AS PrimaryKey,
    'notnull' AS NotNull
FROM pragma_table_info('AbpUserRoles')
WHERE name IN ('UserId', 'RoleId', 'TenantId')
ORDER BY cid;

-- 验证 AbpPermissionGrants 表的关键字段
SELECT 
    name AS ColumnName,
    type AS DataType,
    'notnull' AS NotNull
FROM pragma_table_info('AbpPermissionGrants')
WHERE name IN ('Id', 'TenantId', 'Name', 'ProviderName', 'ProviderKey', 'CreationTime', 'LastModificationTime', 'ConcurrencyStamp')
ORDER BY cid;

-- 验证 AbpPermissions 表的关键字段
SELECT 
    name AS ColumnName,
    type AS DataType,
    'notnull' AS NotNull
FROM pragma_table_info('AbpPermissions')
WHERE name IN ('Id', 'GroupName', 'Name', 'ParentName', 'DisplayName', 'IsEnabled', 'MultiTenancySide', 'Providers', 'StateCheckers', 'ExtraProperties', 'CreationTime', 'CreatorId', 'ConcurrencyStamp')
ORDER BY cid;

-- 验证 AbpPermissionGroups 表的关键字段
SELECT 
    name AS ColumnName,
    type AS DataType,
    'notnull' AS NotNull
FROM pragma_table_info('AbpPermissionGroups')
WHERE name IN ('Id', 'Name', 'DisplayName', 'ExtraProperties', 'ConcurrencyStamp')
ORDER BY cid;

-- 验证 AbpRoleClaims 表的关键字段
SELECT 
    name AS ColumnName,
    type AS DataType,
    'notnull' AS NotNull
FROM pragma_table_info('AbpRoleClaims')
WHERE name IN ('Id', 'RoleId', 'TenantId', 'ClaimType', 'ClaimValue', 'CreationTime', 'LastModificationTime', 'ConcurrencyStamp')
ORDER BY cid;

-- 统计各表的数据量
SELECT 
    'AbpRoles' AS TableName,
    COUNT(*) AS RecordCount
FROM AbpRoles
UNION ALL
SELECT 
    'AbpUserRoles' AS TableName,
    COUNT(*) AS RecordCount
FROM AbpUserRoles
UNION ALL
SELECT 
    'AbpPermissionGrants' AS TableName,
    COUNT(*) AS RecordCount
FROM AbpPermissionGrants
UNION ALL
SELECT 
    'AbpPermissions' AS TableName,
    COUNT(*) AS RecordCount
FROM AbpPermissions
UNION ALL
SELECT 
    'AbpPermissionGroups' AS TableName,
    COUNT(*) AS RecordCount
FROM AbpPermissionGroups
UNION ALL
SELECT 
    'AbpRoleClaims' AS TableName,
    COUNT(*) AS RecordCount
FROM AbpRoleClaims;

-- ============================================
-- 注意事项
-- ============================================
-- 1. 所有表名必须保持为 AbpXXX 格式，以确保与现有数据库兼容
-- 2. 所有列名必须保持不变，以确保与现有数据兼容
-- 3. 部分实体的审计字段在代码中被标记为忽略（IsIgnore = true），这意味着：
--    - 这些字段在数据库中可能不存在
--    - 需要通过代码逻辑处理审计信息的保存
-- 4. UserRole 实体使用复合主键（UserId, RoleId），需要在 SqlSugar 配置中正确设置
-- 5. 数据库结构无需修改，因为所有字段名称保持不变
-- 6. 迁移后需要确保应用程序能够正确使用这些实体

-- ============================================
-- 迁移完成
-- ============================================
