-- ============================================================
-- ABP 废弃表清理脚本
-- 用途：删除迁移后不再使用的 ABP 框架系统表
-- 前置条件：已备份 DFApp.db，应用已停止运行
-- 执行方式：sqlite3 DFApp.db < sql/cleanup-abp-obsolete-tables.sql
-- 注意：此操作不可逆！
-- ============================================================

-- ============================================================
-- 第一部分：安全的 ABP Identity 废弃表（无业务数据）
-- 这些表在当前系统中从未使用，可以安全删除
-- ============================================================

-- ABP 声明类型表
DROP TABLE IF EXISTS AbpClaimTypes;
-- ABP 组织单元相关表
DROP TABLE IF EXISTS AbpOrganizationUnits;
DROP TABLE IF EXISTS AbpOrganizationUnitRoles;
-- ABP 用户声明表
DROP TABLE IF EXISTS AbpUserClaims;
-- ABP 用户第三方登录表
DROP TABLE IF EXISTS AbpUserLogins;
-- ABP 用户-组织单元关联表
DROP TABLE IF EXISTS AbpUserOrganizationUnits;
-- ABP 用户令牌表
DROP TABLE IF EXISTS AbpUserTokens;
-- ABP 用户关联表
DROP TABLE IF EXISTS AbpLinkUsers;
-- ABP 用户委托表
DROP TABLE IF EXISTS AbpUserDelegations;

-- ============================================================
-- 第二部分：ABP 安全日志相关表（日志数据）
-- 新系统使用 Serilog 文件日志替代 ABP 审计日志
-- ============================================================

-- ABP 安全日志表
DROP TABLE IF EXISTS AbpSecurityLogs;
-- ABP 会话表
DROP TABLE IF EXISTS AbpSessions;

-- ============================================================
-- 第三部分：ABP 审计日志相关表
-- 新系统改用 Serilog 文件日志，不再使用 ABP 审计日志
-- ============================================================

DROP TABLE IF EXISTS AbpAuditLogActions;
DROP TABLE IF EXISTS AbpEntityPropertyChanges;
DROP TABLE IF EXISTS AbpEntityChanges;
DROP TABLE IF EXISTS AbpAuditLogs;
-- ABP 审计日志 Excel 文件表（如果有）
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
-- 清理完成
-- ============================================================

SELECT '✅ ABP 废弃表清理完成' AS result;

-- 验证：列出剩余的所有表
SELECT '=== 剩余表列表 ===' AS section;
SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;
