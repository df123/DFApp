-- ============================================
-- RSS模块实体迁移到自定义基类
-- 迁移日期: 2026-03-27
-- ============================================
-- 说明：
-- 本SQL文件记录了Rss模块5个实体从ABP基类迁移到自定义基类的变更
-- 由于所有字段名称保持不变，数据库结构无需修改
-- ============================================

-- 1. RssMirrorItems 表
-- 变更：基类从 Entity<long> 改为 AuditedEntity<long>
-- 影响：无，字段名称完全一致
-- 说明：CreationTime、LastModificationTime、ConcurrencyStamp 字段已由基类提供

-- 2. RssSources 表
-- 变更：基类从 Entity<long> 改为 CreationAuditedEntity<long>
-- 影响：无，字段名称完全一致
-- 说明：CreationTime、ConcurrencyStamp、CreatorId 字段已由基类提供

-- 3. RssSubscriptions 表
-- 变更：基类从 Entity<long> 改为 AuditedEntity<long>
-- 影响：无，字段名称完全一致
-- 说明：CreationTime、LastModificationTime、ConcurrencyStamp、CreatorId 字段已由基类提供

-- 4. RssSubscriptionDownloads 表
-- 变更：基类从 Entity<long> 改为 CreationAuditedEntity<long>
-- 影响：无，字段名称完全一致
-- 说明：CreationTime、CreatorId 字段已由基类提供

-- 5. RssWordSegments 表
-- 变更：基类从 Entity<long> 改为 CreationAuditedEntity<long>
-- 影响：无，字段名称完全一致
-- 说明：CreationTime、CreatorId 字段已由基类提供

-- ============================================
-- 验证脚本（可选）
-- ============================================

-- 验证所有表的存在
SELECT 
    'RssMirrorItems' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('RssMirrorItems')
UNION ALL
SELECT 
    'RssSources' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('RssSources')
UNION ALL
SELECT 
    'RssSubscriptions' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('RssSubscriptions')
UNION ALL
SELECT 
    'RssSubscriptionDownloads' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('RssSubscriptionDownloads')
UNION ALL
SELECT 
    'RssWordSegments' AS TableName,
    COUNT(*) AS ColumnCount
FROM pragma_table_info('RssWordSegments');

-- ============================================
-- 迁移完成
-- ============================================
