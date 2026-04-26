# Phase 6 迁移总结：权限与认证系统（压缩版）

**完成时间**：2026-04-02 | **状态**：已完成 | **迁移范围**：权限系统、JWT 认证、数据迁移脚本

## 概述
Phase 6 完成权限与认证系统的迁移和优化。主要工作包括：修复权限特性集成、修复中间件注册顺序、完善 JWT Claims 链路、创建数据迁移脚本。

## 6.1 自定义权限系统

### 修复的问题

| 问题 | 严重度 | 修复方案 |
|------|--------|---------|
| `[Permission]` 特性未实现 `IAuthorizeData`，权限检查完全不生效 | P0 | 实现 `IAuthorizeData` 接口，Policy 自动生成 `"Permission:{Name}"` |
| `CurrentUserMiddleware` 在 `UseAuthentication` 之前注册，无法读取 Claims | P0 | 移动到 `UseAuthentication` 之后 |
| `UserManagementAppService` 使用硬编码权限字符串，缺少 `DFApp.` 前缀 | P0 | 替换为 `DFAppPermissions` 常量引用 |
| `ICurrentUser`/`CurrentUser` 定义在 `SqlSugarConfig.cs` 中，违反单一职责 | P1 | 分离到 `Infrastructure/ICurrentUser.cs` 和 `Infrastructure/CurrentUser.cs` |
| `AccountAppService.LoginAsync` 中存在未使用的冗余数据库查询 | P2 | 删除冗余的 `userRoles` 查询 |

### 修改文件清单

**新建文件**：
- `Infrastructure/ICurrentUser.cs` — ICurrentUser 接口
- `Infrastructure/CurrentUser.cs` — CurrentUser 实现类

**修改文件**：
- `Permissions/PermissionAttribute.cs` — 实现 `IAuthorizeData`
- `Program.cs` — 调整中间件顺序
- `Infrastructure/CurrentUserMiddleware.cs` — 简化 Claim 查找
- `Services/Account/UserManagementAppService.cs` — 权限常量替换
- `Services/Account/AccountAppService.cs` — 删除冗余查询
- `Data/SqlSugarConfig.cs` — 移除 ICurrentUser/CurrentUser 定义
- 30+ 个 Controller/Service 文件 — 命名空间更新（`Data.ICurrentUser` → `Infrastructure.ICurrentUser`）

## 6.2 JWT 认证优化

### 修复的问题

| 问题 | 严重度 | 修复方案 |
|------|--------|---------|
| Claim Type 映射不一致：短格式写入但长格式读取（依赖 fallback） | P0 | `MapInboundClaims = false`，统一使用短格式 |
| 权限 Claim Type 为魔法字符串散布在 3 个文件中 | P1 | 创建 `DFAppClaimTypes` 常量类 |
| 角色查询结果未写入 JWT Claims | P1 | 将角色 ID 写入 `"role"` Claim |
| `RoleClaimType` 未配置 | P1 | 配置 `RoleClaimType = DFAppClaimTypes.Role` |
| `ClockSkew` 未显式配置，默认 5 分钟容差 | P2 | 设置 `ClockSkew = TimeSpan.Zero` |
| `DFAppControllerBase` 无默认 `[Authorize]` | P2 | 类级别添加 `[Authorize]` |
| 权限 Claims 可能重复 | P3 | 添加 `.Distinct()` 去重 |

### 修改文件清单

**新建文件**：
- `Permissions/DFAppClaimTypes.cs` — Claim 类型常量类

**修改文件**：
- `Program.cs` — `MapInboundClaims = false`、`RoleClaimType`、`ClockSkew`
- `Controllers/DFAppControllerBase.cs` — 添加 `[Authorize]`
- `Services/Account/AccountAppService.cs` — 角色写入 Claims、权限去重
- `Permissions/PermissionChecker.cs` — 常量替换
- `Permissions/PermissionAuthorizationHandler.cs` — 常量替换

### JWT Claims 链路（修复后）

```
Token 生成（AccountAppService）
  ├─ sub: 用户ID
  ├─ unique_name: 用户名
  ├─ email: 邮箱
  ├─ role: 角色ID列表（新增）
  ├─ Permission: 权限名称列表（去重）
  └─ jti: Token ID

Token 验证（Program.cs JWT Bearer）
  ├─ MapInboundClaims = false（统一短格式）
  ├─ RoleClaimType = "role"
  └─ ClockSkew = 0

中间件链路
  ├─ UseAuthentication() → JWT 验证
  ├─ UseMiddleware<CurrentUserMiddleware>() → 提取 sub/unique_name
  └─ UseAuthorization() → 权限检查

权限检查（两种方式）
  ├─ Controller 层：[Permission("X.Y.Z")] → PermissionPolicyProvider → PermissionAuthorizationHandler
  └─ Service 层：CheckPermissionAsync(DFAppPermissions.X.Y.Z) → PermissionChecker
```

## 6.3 数据迁移脚本

### 新建文件

| 文件 | 用途 |
|------|------|
| `sql/verify-identity-data.sql` | Identity 数据完整性验证（不修改数据） |
| `sql/cleanup-abp-obsolete-tables.sql` | ABP 废弃表清理（约 25 张表） |

### 验证脚本覆盖范围
- 用户数据（总数/活跃/禁用/密码哈希）
- 角色数据（列表/默认/静态）
- 用户-角色关联（孤儿检查）
- 权限授予（用户级/角色级/有效性检查）
- 权限定义（按组统计/启用状态）
- 权限分组
- 综合验证摘要

### 清理脚本覆盖范围
| 分类 | 表数量 | 表名 |
|------|--------|------|
| Identity 废弃表 | 8 | AbpClaimTypes, AbpOrganizationUnits, AbpOrganizationUnitRoles, AbpUserClaims, AbpUserLogins, AbpUserOrganizationUnits, AbpUserTokens, AbpLinkUsers, AbpUserDelegations |
| 安全日志表 | 2 | AbpSecurityLogs, AbpSessions |
| 审计日志表 | 5 | AbpAuditLogActions, AbpEntityPropertyChanges, AbpEntityChanges, AbpAuditLogs, AbpAuditLogExcelFiles |
| 多租户表 | 2 | AbpTenantConnectionStrings, AbpTenants |
| 功能管理/设置表 | 6 | AbpFeatureValues, AbpFeatures, AbpFeatureGroups, AbpSettingValues, AbpSettings, AbpSettingDefinitions |
| 后台任务表 | 1 | AbpBackgroundJobs |
| BLOB 存储表 | 2 | AbpBlobs, AbpBlobContainers |

## 统计

| 指标 | 数量 |
|------|------|
| 新建文件 | 5 |
| 修改文件 | 37 |
| 修复 P0 问题 | 3 |
| 修复 P1 问题 | 3 |
| 修复 P2 问题 | 2 |
| 修复 P3 问题 | 1 |
| 新引入编译错误 | 0 |

## 遗留问题
- 角色管理服务缺失（RoleAppService/RoleController），需手动管理数据库
- 权限授予管理服务缺失（PermissionGrantAppService），需手动管理数据库
- Identity 实体命名空间不统一（Account vs Identity）

## 未涉及的内容（Phase 9 处理）
- 旧 ABP 项目目录清理
- 表名重命名（当前保留 Abp 前缀以兼容旧数据）
