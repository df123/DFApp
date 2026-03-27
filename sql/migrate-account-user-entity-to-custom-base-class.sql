-- ====================================================================
-- 迁移Account模块的User实体到自定义基类
-- Phase 2.2 - 子任务10
-- ====================================================================
-- 说明：
-- 1. 将User实体从FullAuditedAggregateRoot<Guid>迁移到AuditedEntity<Guid>
-- 2. 不再使用软删除功能
-- 3. 添加SqlSugar属性
-- 4. 保持数据库表名和列名不变
-- ====================================================================

-- User实体对应的表是AbpUsers
-- 由于只是基类迁移，数据库结构不需要修改
-- 表名：AbpUsers
-- 主键：Id (Guid)
-- 审计字段：CreationTime, CreatorId, LastModificationTime, LastModifierId
-- 业务字段：UserName, Email, PasswordHash, IsActive

-- 验证表结构
SELECT 
    'AbpUsers' AS TableName,
    name AS ColumnName,
    type_name(system_type_id) AS DataType,
    max_length,
    is_nullable
FROM sys.columns 
WHERE object_id = OBJECT_ID('AbpUsers')
ORDER BY ordinal_position;

-- 验证审计字段是否存在
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AbpUsers'
AND COLUMN_NAME IN ('Id', 'CreationTime', 'CreatorId', 'LastModificationTime', 'LastModifierId', 'UserName', 'Email', 'PasswordHash', 'IsActive')
ORDER BY ORDINAL_POSITION;

-- 说明：
-- 1. AbpUsers表结构保持不变
-- 2. 审计字段（CreationTime, CreatorId, LastModificationTime, LastModifierId）已存在
-- 3. 业务字段（UserName, Email, PasswordHash, IsActive）保持不变
-- 4. 不再需要IsDeleted字段（软删除字段），因为不再使用软删除功能

-- 注意事项：
-- - User实体是特殊的实体，它替代了ABP Identity的IdentityUser表
-- - 表名必须保持为AbpUsers以确保与现有数据库兼容
-- - 所有列名必须保持不变以确保与现有数据兼容
-- - 不再使用软删除，所以IsDeleted字段不再使用（如果存在的话）
