# 权限管理系统重建

## 概述

重建权限管理系统，使用新的 `AppPermissionGrants` 表替代旧的 `AbpPermissionGrants` 表，彻底解决 GUID 大小写不匹配和 SqlSugar LINQ 翻译问题。

## 动机

旧的 `AbpPermissionGrants` 表存在以下问题：
- GUID 大小写不一致（ABP 迁移数据为大写，新数据可能为小写）
- SqlSugar LINQ 的 `Contains` 方法翻译存在兼容性问题
- 继承 ABP 基类带来不必要的复杂性

## 新表设计

### AppPermissionGrants

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | INTEGER | 自增主键 |
| PermissionName | TEXT(200) | 权限名称，如 `DFApp.RoleManagement` |
| ProviderType | TEXT(20) | 授予目标类型：`Role` 或 `User` |
| ProviderKey | TEXT(100) | 授予目标标识：角色名称或用户 ID |
| CreationTime | TEXT | 创建时间 |

关键设计决策：
- 使用 `long` 自增主键（避免 GUID 大小写问题）
- `ProviderType` 使用可读字符串 `"Role"` / `"User"` 替代 `"R"` / `"U"`
- `ProviderKey` 存储角色名称（非 GUID），避免 GUID 匹配问题
- 不继承任何 ABP 基类

## 修改的文件

### 新增文件
- `src/DFApp.Web/Domain/AppPermissionGrant.cs` — 新权限授予实体
- `sql/migrate-to-app-permission-grants.sql` — 数据迁移脚本

### 修改文件
- `src/DFApp.Web/Services/Account/AccountAppService.cs` — 权限加载逻辑改为从新表查询
- `src/DFApp.Web/Services/Identity/PermissionGrantManagementAppService.cs` — CRUD 改为使用新实体
- `src/DFApp.Web/Services/Identity/RoleManagementAppService.cs` — 级联删除改为操作新表
- `src/DFApp.Web/DTOs/Identity/PermissionGrantDto.cs` — DTO 字段更新

### 未修改文件
- `src/DFApp.Web/Services/Identity/UserRoleManagementAppService.cs` — 不涉及权限授予操作
- `src/DFApp.Web/Controllers/PermissionGrantManagementController.cs` — 控制器不变，仅调用 Service

## API 变更

### DTO 字段变更

| 旧字段 | 新字段 | 说明 |
|--------|--------|------|
| `Id` (Guid) | `Id` (long) | 主键类型变更 |
| `Name` | `PermissionName` | 更明确的命名 |
| `ProviderName` ("R"/"U") | `ProviderType` ("Role"/"User") | 使用可读字符串 |

### 接口影响

`PermissionGrantManagementController` 的接口参数发生变更：
- `GetListAsync`: `providerName` → `providerType`, `providerKey` 含义变更
- `UpdateAsync`: 同上
- `GrantAsync`: 同上
- `RevokeAsync`: 同上

## 登录权限加载流程

1. 查询用户角色关联（`AbpUserRoles` 表，仍使用 UPPER() 匹配 GUID）
2. 获取所有角色，内存中匹配角色名称
3. 将角色名称添加到 JWT claims（`role` claim）
4. 从 `AppPermissionGrants` 查询用户级权限（`ProviderType="User"`, `ProviderKey=userId`）
5. 从 `AppPermissionGrants` 查询角色级权限（`ProviderType="Role"`），内存中匹配角色名称
6. 将所有权限添加到 JWT claims（`Permission` claim）

## 数据迁移

执行 `sql/migrate-to-app-permission-grants.sql`：
1. 创建 `AppPermissionGrants` 表和索引
2. 迁移用户级权限：`ProviderKey` 统一转为小写
3. 迁移角色级权限：`ProviderKey` 从角色 GUID 转换为角色名称
4. 验证迁移结果

## 角色重命名处理

当角色名称变更时，`RoleManagementAppService.UpdateAsync` 会同步更新 `AppPermissionGrants` 中 `ProviderType="Role"` 且 `ProviderKey=旧角色名` 的记录。

## 前端适配（待办）

前端 `src/api/permission.ts` 仍使用旧的 API 路径和参数格式：
- 路径：`/api/permission-management/permissions`（需改为 `/api/app/permission-grant-management`）
- 参数：`providerName` "R"/"U"（需改为 `providerType` "Role"/"User"）
- 字段：`name`（需改为 `permissionName`）
