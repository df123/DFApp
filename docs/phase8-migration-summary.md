# Phase 8 迁移总结：用户/角色/权限数据迁移与 ABP 系统表清理

**完成时间**：2026-04-02 | **状态**：已完成 | **迁移范围**：身份数据精简、软删除清理、权限数据修复、ABP 遗留表统一清理、密码哈希兼容性确认

## 概述

Phase 8 是数据层迁移的收尾阶段，共 3 个子任务。重点是用户表精简、权限数据格式修复、以及一次性清除所有 ABP 遗留系统表。同时确认了密码哈希方案的兼容性。

## 8.1 用户/角色/权限数据迁移 SQL

### 8.1a — AbpUsers 表字段精简迁移

- **新建文件**：`sql/migrate-abpusers-table.sql`
- **操作**：将 AbpUsers 表从 31 列精简为 10 列
- **移除的 21 个 ABP 冗余列**：TenantId, NormalizedUserName, Name, Surname, NormalizedEmail, EmailConfirmed, SecurityStamp, IsExternal, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, ShouldChangePasswordOnNextLogin, EntityVersion, LastPasswordChangeTime, ExtraProperties, IsDeleted, DeletionTime, DeleterId
- **保留的 10 列**：Id, UserName, Email, PasswordHash, IsActive, ConcurrencyStamp, CreationTime, CreatorId, LastModificationTime, LastModifierId
- **数据迁移**：2 个活跃用户保留，3 个软删除用户不复制到新表
- **执行状态**：✅ 已执行

### 8.1b — 软删除用户物理删除

- **新建文件**：`sql/cleanup-soft-deleted-users.sql`
- **操作**：物理删除 3 个已软删除用户及其关联数据
- **删除用户**：cms（旧）、down、cms2
- **关联清理**：AbpUserRoles 0 条、AbpPermissionGrants 0 条
- **执行状态**：✅ 已执行

### 8.1c — 身份数据验证脚本增强

- **修改文件**：`sql/verify-identity-data.sql`（71行 → 421行）
- **新增检查项**：软删除残留、多租户残留、用户/角色级权限有效性、密码哈希完整性、用户名唯一性、表结构验证、权限分组一致性、未分配角色用户
- **执行状态**：✅ 已执行

### 额外发现 — 权限授予数据格式修复

- **新建文件**：`sql/fix-permission-grants-data.sql`
- **问题 1**：角色级权限授予的 ProviderKey 存储角色名而非角色 GUID（123条记录）
- **问题 2**：用户级权限授予的 GUID 大小写不一致（25条记录）
- **修复**：角色名→角色GUID，GUID统一大写
- **执行状态**：✅ 已执行

## 8.2 ABP 系统表清理 SQL

- **新建文件**：`sql/cleanup-all-abp-tables.sql`
- **整合**：原 `cleanup-abp-obsolete-tables.sql`（25张表）+ `remove-openiddict-tables.sql`（4张表）+ `__EFMigrationsHistory`（1张表）
- **清理范围**：共 31 张 ABP 遗留表
- **分类**：Identity废弃(9)、安全日志(2)、审计日志(5)、多租户(2)、功能/设置(6)、后台任务(1)、BLOB存储(2)、OpenIddict(4)、EF迁移(1)
- **执行状态**：✅ 已执行

## 8.3 密码哈希兼容性

- **结论**：✅ 完全兼容，无需迁移
- **原因**：项目始终使用自定义 PBKDF2-HMAC-SHA256 实现（16字节盐、10000次迭代、32字节哈希），从未使用过 ABP Identity 内置的 PasswordHasher V3 格式
- **格式**：`Base64(salt[16B] + hash[32B])` = 64字符

## 迁移后数据库状态

| 指标 | 迁移前 | 迁移后 | 说明 |
|------|--------|--------|------|
| 总表数 | 73 | 32 | 删除 31 张 ABP 遗留表，10 张无数据表自然消失 |
| 用户 | 5 | 2 | 保留 admin + cms新，删除 3 个软删除用户 |
| 角色 | 13 | 13 | 不变 |
| 用户角色关联 | 10 | 10 | 不变，全属于 admin |
| 权限授予 | 148 | 148 | 不变，已修复数据格式 |
| 权限定义 | 126 | 126 | 不变 |
| 权限分组 | 23 | 23 | 不变 |

## 文件变更清单

### 新建文件（4 个）

| 文件 | 用途 |
|------|------|
| `sql/migrate-abpusers-table.sql` | AbpUsers 表精简迁移（31列→10列） |
| `sql/cleanup-soft-deleted-users.sql` | 软删除用户物理删除及关联清理 |
| `sql/cleanup-all-abp-tables.sql` | ABP 遗留表统一清理（31张表） |
| `sql/fix-permission-grants-data.sql` | 权限授予数据格式修复 |

### 修改文件（2 个）

| 文件 | 用途 |
|------|------|
| `sql/verify-identity-data.sql` | 增强验证脚本（71行→421行），新增 8 项检查 |
| `sql/README.md` | 更新 SQL 脚本使用说明 |

### 废弃文件（2 个）

| 文件 | 原因 |
|------|------|
| `sql/cleanup-abp-obsolete-tables.sql` | 被 `cleanup-all-abp-tables.sql` 替代 |
| `sql/remove-openiddict-tables.sql` | 被 `cleanup-all-abp-tables.sql` 替代 |

## 统计

| 指标 | 数量 |
|------|------|
| 新建 SQL 文件 | 4 |
| 修改 SQL 文件 | 2 |
| 废弃 SQL 文件 | 2 |
| 清理的 ABP 表 | 31 |
| 精简的用户表列数 | 31 → 10（移除 21 列） |
| 修复的权限授予记录 | 148（123 角色级 + 25 用户级） |
| 物理删除的用户 | 3 |
| 验证脚本新增检查项 | 8 |

## 遗留问题

1. **cms 用户无角色分配**：cms 用户没有任何角色绑定，需要确认是否为预期行为
2. **表名前缀未重命名**：AbpUsers、AbpRoles 等业务表仍保持 Abp 前缀，未来可考虑重命名（但非必须，代码中 `[SugarTable]` 已明确映射）
