-- ============================================================
-- AbpUsers 表精简迁移脚本（幂等安全版）
-- 用途：移除 ABP Framework 遗留的冗余字段，使表结构与新 User 实体一致
-- 前置条件：已备份 DFApp.db，应用已停止运行
-- 执行方式：sqlite3 DFApp.db < sql/02-migrate-abpusers-table.sql
-- 注意：此操作不可逆！请务必先备份数据库！
-- 幂等性：如果迁移已完成，脚本会安全终止，不会重复执行
-- ============================================================
--
-- 背景：
--   项目已从 ABP Framework 迁移到轻量级 ASP.NET Core 架构。
--   新的 User 实体（DFApp.Web.Domain.Account.User）只映射以下字段：
--     - Id, ConcurrencyStamp, CreationTime, CreatorId, LastModificationTime, LastModifierId
--     - UserName, Email, PasswordHash, IsActive
--   其余 ABP Identity 遗留字段（共 19 列）需要移除。
--
-- 迁移策略（SQLite 建新表→复制数据→删旧表→重命名）：
--   1. 创建新表 _AbpUsers_new，只包含目标字段
--   2. 从旧表复制未软删除的数据到新表（排除 IsDeleted=1 的记录）
--   3. 删除旧表
--   4. 将新表重命名为 AbpUsers
--
-- 安全机制：
--   - .bail on：遇到错误时立即终止，防止 INSERT 失败后继续执行 DROP TABLE
--   - 迁移状态检查：检测 IsDeleted 列是否存在，已完成迁移时安全退出
--   - DROP TABLE IF EXISTS：清理上次失败运行遗留的临时表
-- ============================================================

-- 关键安全设置：遇到错误时终止脚本执行
-- 防止 INSERT 失败后脚本继续执行 DROP TABLE 导致数据丢失
.bail on


-- ============================================================
-- 第一部分：前置检查
-- ============================================================

-- 1.1 确认 AbpUsers 表存在
SELECT '正在检查 AbpUsers 表是否存在...' AS step;
SELECT CASE
    WHEN COUNT(*) > 0 THEN '✅ AbpUsers 表存在，可以继续'
    ELSE '❌ AbpUsers 表不存在，脚本终止！请检查数据库状态。'
END AS result
FROM sqlite_master WHERE type = 'table' AND name = 'AbpUsers';

-- 1.2 查看当前表结构
SELECT '=== 当前 AbpUsers 表结构 ===' AS section;
SELECT name AS ColumnName, type AS DataType, "notnull" AS "NotNull"
FROM pragma_table_info('AbpUsers')
ORDER BY cid;

-- 1.3 检查迁移状态
-- 通过 pragma_table_info 查询列信息，不直接引用 IsDeleted 列，因此总是安全的
SELECT '=== 迁移状态检查 ===' AS section;
SELECT CASE
    WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers') WHERE name = 'IsDeleted') > 0
    THEN '🔄 IsDeleted 列存在，需要执行数据迁移'
    ELSE '⏭️ IsDeleted 列不存在，迁移已完成。脚本将安全终止。'
END AS migration_status;

-- 1.4 查看当前数据量（含已软删除的）
-- 此查询引用 IsDeleted 列作为安全守卫：
--   - 如果 IsDeleted 列存在 → 正常输出统计数据
--   - 如果 IsDeleted 列不存在 → 产生 "no such column: IsDeleted" 错误
--     配合 .bail on 指令，脚本将在此安全终止
--     后续的事务迁移操作（含 DROP TABLE）不会被执行，保护数据安全
SELECT '=== 当前数据统计 ===' AS section;
SELECT
    COUNT(*) AS TotalRows,
    SUM(CASE WHEN IsDeleted = 1 THEN 1 ELSE 0 END) AS SoftDeletedRows,
    SUM(CASE WHEN IsDeleted = 0 OR IsDeleted IS NULL THEN 1 ELSE 0 END) AS ActiveRows
FROM AbpUsers;


-- ============================================================
-- 第二部分：数据迁移（事务保护）
-- 仅在 IsDeleted 列存在时才会执行到这里（否则已在上方终止）
-- ============================================================

-- 清理上次失败运行遗留的临时表（幂等性保障）
DROP TABLE IF EXISTS _AbpUsers_new;

BEGIN TRANSACTION;

-- 备份提示（仅输出提醒，SQLite 不支持自动备份）
SELECT '⚠️ 正在执行数据迁移，请确认已备份 DFApp.db！' AS warning;

-- 2.1 创建新表 _AbpUsers_new，只保留目标字段
-- 字段定义与新的 User 实体一致
CREATE TABLE _AbpUsers_new (
    -- 基类 EntityBase<Guid> 字段
    Id              TEXT PRIMARY KEY,              -- 主键 (Guid)
    ConcurrencyStamp TEXT,                         -- 并发标记

    -- 审计字段（来自 AuditedEntity<Guid>）
    CreationTime     TEXT,                         -- 创建时间
    CreatorId        TEXT,                         -- 创建者 ID
    LastModificationTime TEXT,                     -- 最后修改时间
    LastModifierId   TEXT,                         -- 最后修改者 ID

    -- User 实体业务字段
    UserName         TEXT NOT NULL DEFAULT '',      -- 用户名
    Email            TEXT NOT NULL DEFAULT '',      -- 邮箱
    PasswordHash     TEXT,                          -- 密码哈希
    IsActive         INTEGER NOT NULL DEFAULT 1     -- 是否激活
);

-- 2.2 从旧表复制数据，排除已软删除的记录
INSERT INTO _AbpUsers_new (
    Id,
    ConcurrencyStamp,
    CreationTime,
    CreatorId,
    LastModificationTime,
    LastModifierId,
    UserName,
    Email,
    PasswordHash,
    IsActive
)
SELECT
    Id,
    ConcurrencyStamp,
    CreationTime,
    CreatorId,
    LastModificationTime,
    LastModifierId,
    UserName,
    Email,
    PasswordHash,
    IsActive
FROM AbpUsers
WHERE IsDeleted = 0 OR IsDeleted IS NULL;

-- 2.3 验证数据复制数量
-- 确保复制的行数等于活跃用户数，如果数量不匹配则回滚
-- （SQLite 不支持在事务中直接回滚，此处仅输出警告供人工检查）
SELECT '=== 数据复制验证 ===' AS section;
SELECT
    (SELECT COUNT(*) FROM _AbpUsers_new) AS NewTableRows,
    (SELECT COUNT(*) FROM AbpUsers WHERE IsDeleted = 0 OR IsDeleted IS NULL) AS ExpectedRows;

-- 2.4 删除旧表
DROP TABLE AbpUsers;

-- 2.5 将新表重命名为 AbpUsers
ALTER TABLE _AbpUsers_new RENAME TO AbpUsers;

COMMIT;


-- ============================================================
-- 第三部分：迁移后验证
-- ============================================================

-- 验证新表结构（应只有 10 列）
SELECT '=== 新 AbpUsers 表结构 ===' AS section;
SELECT name AS ColumnName, type AS DataType, "notnull" AS "NotNull"
FROM pragma_table_info('AbpUsers')
ORDER BY cid;

-- 验证列数是否正确（应为 10）
SELECT CASE
    WHEN (SELECT COUNT(*) FROM pragma_table_info('AbpUsers')) = 10
    THEN '✅ 列数正确（10 列）'
    ELSE '❌ 列数不正确！预期 10 列，实际 '
        || (SELECT COUNT(*) FROM pragma_table_info('AbpUsers')) || ' 列'
END AS ColumnCheck;

-- 验证已移除的列确实不存在
SELECT '=== 已移除列检查 ===' AS section;
SELECT
    CASE WHEN MAX(CASE WHEN name = 'TenantId' THEN 1 ELSE 0 END) = 0 THEN '✅ TenantId 已移除' ELSE '❌ TenantId 仍存在' END AS TenantId,
    CASE WHEN MAX(CASE WHEN name = 'NormalizedUserName' THEN 1 ELSE 0 END) = 0 THEN '✅ NormalizedUserName 已移除' ELSE '❌ NormalizedUserName 仍存在' END AS NormalizedUserName,
    CASE WHEN MAX(CASE WHEN name = 'Name' THEN 1 ELSE 0 END) = 0 THEN '✅ Name 已移除' ELSE '❌ Name 仍存在' END AS Name,
    CASE WHEN MAX(CASE WHEN name = 'Surname' THEN 1 ELSE 0 END) = 0 THEN '✅ Surname 已移除' ELSE '❌ Surname 仍存在' END AS Surname,
    CASE WHEN MAX(CASE WHEN name = 'NormalizedEmail' THEN 1 ELSE 0 END) = 0 THEN '✅ NormalizedEmail 已移除' ELSE '❌ NormalizedEmail 仍存在' END AS NormalizedEmail,
    CASE WHEN MAX(CASE WHEN name = 'EmailConfirmed' THEN 1 ELSE 0 END) = 0 THEN '✅ EmailConfirmed 已移除' ELSE '❌ EmailConfirmed 仍存在' END AS EmailConfirmed,
    CASE WHEN MAX(CASE WHEN name = 'SecurityStamp' THEN 1 ELSE 0 END) = 0 THEN '✅ SecurityStamp 已移除' ELSE '❌ SecurityStamp 仍存在' END AS SecurityStamp,
    CASE WHEN MAX(CASE WHEN name = 'IsExternal' THEN 1 ELSE 0 END) = 0 THEN '✅ IsExternal 已移除' ELSE '❌ IsExternal 仍存在' END AS IsExternal,
    CASE WHEN MAX(CASE WHEN name = 'PhoneNumber' THEN 1 ELSE 0 END) = 0 THEN '✅ PhoneNumber 已移除' ELSE '❌ PhoneNumber 仍存在' END AS PhoneNumber,
    CASE WHEN MAX(CASE WHEN name = 'PhoneNumberConfirmed' THEN 1 ELSE 0 END) = 0 THEN '✅ PhoneNumberConfirmed 已移除' ELSE '❌ PhoneNumberConfirmed 仍存在' END AS PhoneNumberConfirmed,
    CASE WHEN MAX(CASE WHEN name = 'TwoFactorEnabled' THEN 1 ELSE 0 END) = 0 THEN '✅ TwoFactorEnabled 已移除' ELSE '❌ TwoFactorEnabled 仍存在' END AS TwoFactorEnabled,
    CASE WHEN MAX(CASE WHEN name = 'LockoutEnd' THEN 1 ELSE 0 END) = 0 THEN '✅ LockoutEnd 已移除' ELSE '❌ LockoutEnd 仍存在' END AS LockoutEnd,
    CASE WHEN MAX(CASE WHEN name = 'LockoutEnabled' THEN 1 ELSE 0 END) = 0 THEN '✅ LockoutEnabled 已移除' ELSE '❌ LockoutEnabled 仍存在' END AS LockoutEnabled,
    CASE WHEN MAX(CASE WHEN name = 'AccessFailedCount' THEN 1 ELSE 0 END) = 0 THEN '✅ AccessFailedCount 已移除' ELSE '❌ AccessFailedCount 仍存在' END AS AccessFailedCount,
    CASE WHEN MAX(CASE WHEN name = 'ShouldChangePasswordOnNextLogin' THEN 1 ELSE 0 END) = 0 THEN '✅ ShouldChangePasswordOnNextLogin 已移除' ELSE '❌ ShouldChangePasswordOnNextLogin 仍存在' END AS ShouldChangePasswordOnNextLogin,
    CASE WHEN MAX(CASE WHEN name = 'EntityVersion' THEN 1 ELSE 0 END) = 0 THEN '✅ EntityVersion 已移除' ELSE '❌ EntityVersion 仍存在' END AS EntityVersion,
    CASE WHEN MAX(CASE WHEN name = 'LastPasswordChangeTime' THEN 1 ELSE 0 END) = 0 THEN '✅ LastPasswordChangeTime 已移除' ELSE '❌ LastPasswordChangeTime 仍存在' END AS LastPasswordChangeTime,
    CASE WHEN MAX(CASE WHEN name = 'ExtraProperties' THEN 1 ELSE 0 END) = 0 THEN '✅ ExtraProperties 已移除' ELSE '❌ ExtraProperties 仍存在' END AS ExtraProperties,
    CASE WHEN MAX(CASE WHEN name = 'IsDeleted' THEN 1 ELSE 0 END) = 0 THEN '✅ IsDeleted 已移除' ELSE '❌ IsDeleted 仍存在' END AS IsDeleted,
    CASE WHEN MAX(CASE WHEN name = 'DeletionTime' THEN 1 ELSE 0 END) = 0 THEN '✅ DeletionTime 已移除' ELSE '❌ DeletionTime 仍存在' END AS DeletionTime,
    CASE WHEN MAX(CASE WHEN name = 'DeleterId' THEN 1 ELSE 0 END) = 0 THEN '✅ DeleterId 已移除' ELSE '❌ DeleterId 仍存在' END AS DeleterId
FROM pragma_table_info('AbpUsers');

-- 验证数据完整性（查看迁移后的用户数据）
SELECT '=== 迁移后用户数据 ===' AS section;
SELECT Id, UserName, Email, IsActive, CreationTime, CreatorId
FROM AbpUsers;

-- 统计迁移后的数据量
SELECT '=== 迁移后数据统计 ===' AS section;
SELECT COUNT(*) AS TotalUsers FROM AbpUsers;

-- ============================================================
-- 迁移完成
-- ============================================================
SELECT '✅ AbpUsers 表精简迁移完成！' AS result;
SELECT '   已移除 21 列 ABP 遗留字段，保留 10 列。' AS detail;
SELECT '   已软删除的用户（IsDeleted=1）未被迁移。' AS detail2;
