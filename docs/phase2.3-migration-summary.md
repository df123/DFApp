# Phase 2.3 迁移总结文档

## 1. 概述

### 1.1 Phase 2.3 目标和范围

Phase 2.3 是框架迁移计划中的第三个子阶段，主要目标是：

- 将 Identity 模块的 6 个实体类从 ABP Identity 基类迁移到自定义基类
- 使用 `AuditedEntity<Guid>`、`CreationAuditedEntity<Guid>` 和 `Entity` 替代 ABP Identity 的 `IdentityRole<Guid>`、`IdentityRoleClaim<Guid>`、`IdentityUserRole<Guid>`、`Permission`、`PermissionGrant`、`PermissionGroup` 等基类
- 添加 SqlSugar 属性（`[SugarTable]` 和 `[SugarColumn]`）
- 保持数据库表名和列名完全一致，确保数据兼容性
- 处理审计字段的特殊需求（部分字段标记为 IsIgnore）

### 1.2 完成时间

Phase 2.3 于 2026 年 3 月 27 日完成。

### 1.3 主要工作内容

- 迁移 Identity 模块的 6 个实体类
- 为所有实体类添加 SqlSugar 属性
- 修改实体基类，从 ABP Identity 基类迁移到自定义基类
- 创建数据库迁移脚本，记录变更内容
- 确保数据库结构兼容性，无需修改表结构
- 处理复合主键的特殊情况（UserRole 实体）

## 2. 迁移的实体类列表

### 2.1 Identity 模块（6个实体）

| 序号 | 实体类 | 原基类 | 新基类 | 表名 |
|------|--------|--------|--------|------|
| 1 | `Permission` | `Permission` | `CreationAuditedEntity<Guid>` | `AbpPermissions` |
| 2 | `PermissionGrant` | `PermissionGrant` | `AuditedEntity<Guid>` | `AbpPermissionGrants` |
| 3 | `PermissionGroup` | `PermissionGroup` | `Entity` | `AbpPermissionGroups` |
| 4 | `Role` | `IdentityRole<Guid>` | `AuditedEntity<Guid>` | `AbpRoles` |
| 5 | `RoleClaim` | `IdentityRoleClaim<Guid>` | `AuditedEntity<Guid>` | `AbpRoleClaims` |
| 6 | `UserRole` | `IdentityUserRole<Guid>` | 无基类（普通类） | `AbpUserRoles` |

### 2.2 基类选择统计

| 新基类 | 实体数量 | 占比 |
|--------|----------|------|
| `AuditedEntity<Guid>` | 3 | 50.0% |
| `CreationAuditedEntity<Guid>` | 1 | 16.7% |
| `Entity` | 1 | 16.7% |
| 无基类（普通类） | 1 | 16.7% |

## 3. 通用修改内容

### 3.1 Using 语句修改

所有实体类都进行了以下 using 语句修改：

**移除的 using 语句：**
- `using Volo.Abp.Identity;`
- `using Volo.Abp.PermissionManagement;`

**添加的 using 语句：**
- `using SqlSugar;`
- `using DFApp.Web.Domain;`

### 3.2 基类迁移规则

#### 从 ABP Identity 基类迁移到自定义基类

**Identity 模块实体的特殊性：**

Identity 模块的实体与 ABP Identity 紧密集成，迁移时需要特别注意：

1. **表名必须保持为 `AbpXXX` 格式**
   - 确保与现有数据库兼容
   - 避免与 ABP Identity 的其他功能冲突

2. **部分审计字段被标记为 IsIgnore**
   - 原因：ABP Identity 表中可能不存在这些字段
   - 解决：通过代码逻辑处理审计信息的保存

3. **UserRole 实体使用复合主键**
   - 主键：`UserId` 和 `RoleId`
   - 需要在 SqlSugar 配置中正确设置

### 3.3 SqlSugar 属性添加规则

#### `[SugarTable]` 属性

- 添加到所有实体类上
- 指定数据库表名（保持 `AbpXXX` 格式）
- 保持与原表名完全一致

示例：
```csharp
[SugarTable("AbpRoles")]
public class Role : AuditedEntity<Guid>
{
    // ...
}
```

#### `[SugarColumn]` 属性

- 添加到所有业务属性上
- 主要用于指定列名（`ColumnName`）
- 用于忽略基类字段（`IsIgnore = true`）
- 用于指定列数据类型（`ColumnDataType`）
- 用于指定主键（`IsPrimaryKey = true`）

示例：
```csharp
[SugarColumn(ColumnName = "Name", Length = 256)]
public string Name { get; set; } = string.Empty;

[SugarColumn(IsIgnore = true)]
public new string ConcurrencyStamp { get; set; } = string.Empty;

[SugarColumn(IsPrimaryKey = true, ColumnName = "UserId")]
public Guid UserId { get; set; }
```

### 3.4 审计字段处理规则

Identity 模块实体的审计字段处理比较特殊：

1. **部分实体的审计字段被标记为 IsIgnore**
   - 原因：ABP Identity 表中可能不存在这些字段
   - 影响：这些字段不会映射到数据库
   - 解决：通过代码逻辑处理审计信息的保存

2. **使用 `new` 关键字隐藏基类字段**
   - 使用 `new` 关键字重新定义基类字段
   - 将其标记为 `IsIgnore = true`
   - 避免与数据库字段冲突

示例：
```csharp
[SugarColumn(IsIgnore = true)]
public new string ConcurrencyStamp { get; set; } = string.Empty;
```

## 4. 每个模块的详细修改内容

### 4.1 Identity 模块

#### 4.1.1 Permission 实体

**文件路径：** `src/DFApp.Web/Domain/Identity/Permission.cs`

**修改内容：**
1. 修改基类：从 `Permission` 改为 `CreationAuditedEntity<Guid>`
2. 添加 using 语句：`using SqlSugar;` 和 `using DFApp.Web.Domain;`
3. 添加 `[SugarTable("AbpPermissions")]` 属性
4. 为所有业务属性添加 `[SugarColumn(ColumnName = "...")]` 属性
5. 将基类字段标记为 `IsIgnore = true`（ConcurrencyStamp、CreationTime、CreatorId）
6. 将 TenantId 字段标记为 `IsIgnore = true`

**业务字段：**
- `GroupName` (string) - 分组名称
- `Name` (string) - 权限名称
- `ParentName` (string?) - 父权限名称
- `DisplayName` (string) - 显示名称
- `IsEnabled` (bool) - 是否启用
- `MultiTenancySide` (int) - 多租户侧
- `Providers` (string?) - 提供者
- `StateCheckers` (string?) - 状态检查器
- `ExtraProperties` (string?) - 扩展属性

**特殊说明：**
- 所有基类字段都被标记为 `IsIgnore = true`
- TenantId 字段也被标记为 `IsIgnore = true`
- 这些字段在数据库中可能不存在，需要通过代码逻辑处理

#### 4.1.2 PermissionGrant 实体

**文件路径：** `src/DFApp.Web/Domain/Identity/PermissionGrant.cs`

**修改内容：**
1. 修改基类：从 `PermissionGrant` 改为 `AuditedEntity<Guid>`
2. 添加 using 语句：`using SqlSugar;` 和 `using DFApp.Web.Domain;`
3. 添加 `[SugarTable("AbpPermissionGrants")]` 属性
4. 为所有业务属性添加 `[SugarColumn(ColumnName = "...")]` 属性
5. 将所有基类字段标记为 `IsIgnore = true`

**业务字段：**
- `TenantId` (Guid?) - 租户 ID
- `Name` (string) - 权限名称
- `ProviderName` (string) - 提供者名称
- `ProviderKey` (string) - 提供者键

**特殊说明：**
- 所有基类字段都被标记为 `IsIgnore = true`
- 这些字段在数据库中可能不存在，需要通过代码逻辑处理

#### 4.1.3 PermissionGroup 实体

**文件路径：** `src/DFApp.Web/Domain/Identity/PermissionGroup.cs`

**修改内容：**
1. 修改基类：从 `PermissionGroup` 改为 `Entity`
2. 添加 using 语句：`using SqlSugar;` 和 `using DFApp.Web.Domain;`
3. 添加 `[SugarTable("AbpPermissionGroups")]` 属性
4. 为所有业务属性添加 `[SugarColumn(ColumnName = "...")]` 属性
5. 将 ConcurrencyStamp 字段标记为 `IsIgnore = true`

**业务字段：**
- `Name` (string) - 分组名称
- `DisplayName` (string) - 显示名称
- `ExtraProperties` (string?) - 扩展属性（JSON 格式）

**特殊说明：**
- ConcurrencyStamp 字段被标记为 `IsIgnore = true`
- 该字段在数据库中可能不存在，需要通过代码逻辑处理

#### 4.1.4 Role 实体

**文件路径：** `src/DFApp.Web/Domain/Identity/Role.cs`

**修改内容：**
1. 修改基类：从 `IdentityRole<Guid>` 改为 `AuditedEntity<Guid>`
2. 添加 using 语句：`using SqlSugar;` 和 `using DFApp.Web.Domain;`
3. 添加 `[SugarTable("AbpRoles")]` 属性
4. 为所有业务属性添加 `[SugarColumn(ColumnName = "...")]` 属性
5. 将部分基类字段标记为 `IsIgnore = true`（CreatorId、LastModificationTime、LastModifierId）

**业务字段：**
- `TenantId` (Guid?) - 租户 ID
- `Name` (string) - 角色名称
- `NormalizedName` (string) - 规范化角色名称
- `IsDefault` (bool) - 是否为默认角色
- `IsStatic` (bool) - 是否为静态角色（不可删除）
- `IsPublic` (bool) - 是否为公共角色
- `EntityVersion` (int) - 实体版本
- `ExtraProperties` (string) - 扩展属性（JSON 格式）

**特殊说明：**
- CreatorId、LastModificationTime、LastModifierId 字段被标记为 `IsIgnore = true`
- 这些字段在数据库中可能不存在，需要通过代码逻辑处理

#### 4.1.5 RoleClaim 实体

**文件路径：** `src/DFApp.Web/Domain/Identity/RoleClaim.cs`

**修改内容：**
1. 修改基类：从 `IdentityRoleClaim<Guid>` 改为 `AuditedEntity<Guid>`
2. 添加 using 语句：`using SqlSugar;` 和 `using DFApp.Web.Domain;`
3. 添加 `[SugarTable("AbpRoleClaims")]` 属性
4. 为所有业务属性添加 `[SugarColumn(ColumnName = "...")]` 属性
5. 将所有基类字段标记为 `IsIgnore = true`
6. 将 Role 导航属性标记为 `IsIgnore = true`

**业务字段：**
- `RoleId` (Guid) - 角色 ID
- `TenantId` (Guid?) - 租户 ID
- `ClaimType` (string) - 声明类型
- `ClaimValue` (string?) - 声明值

**导航属性：**
- `Role` (Role?) - 角色

**特殊说明：**
- 所有基类字段都被标记为 `IsIgnore = true`
- Role 导航属性也被标记为 `IsIgnore = true`
- 这些字段在数据库中可能不存在，需要通过代码逻辑处理

#### 4.1.6 UserRole 实体

**文件路径：** `src/DFApp.Web/Domain/Identity/UserRole.cs`

**修改内容：**
1. 修改基类：从 `IdentityUserRole<Guid>` 改为无基类（普通类）
2. 添加 using 语句：`using SqlSugar;`
3. 添加 `[SugarTable("AbpUserRoles")]` 属性
4. 为所有属性添加 `[SugarColumn]` 属性
5. 为 UserId 和 RoleId 设置复合主键

**业务字段：**
- `UserId` (Guid) - 用户 ID（主键）
- `RoleId` (Guid) - 角色 ID（主键）
- `TenantId` (Guid?) - 租户 ID

**特殊说明：**
- 使用复合主键（UserId, RoleId）
- 无审计字段
- 无基类，是一个普通类

## 5. 遇到的问题和解决方案

### 5.1 审计字段不映射到数据库

#### 问题描述

Identity 模块的实体在迁移后，部分审计字段被标记为 `IsIgnore = true`，这意味着这些字段不会映射到数据库。这可能会导致审计信息丢失。

#### 解决方案

1. **通过代码逻辑处理审计信息**
   - 在应用服务层手动设置审计字段
   - 使用拦截器或过滤器自动填充审计字段
   - 在保存实体时，通过代码逻辑将审计信息保存到其他表或日志中

2. **使用数据库触发器**
   - 在数据库中创建触发器，自动填充审计字段
   - 在插入或更新记录时，触发器自动设置审计信息

3. **使用 SqlSugar 的拦截器**
   - 实现 SqlSugar 的拦截器接口
   - 在执行 SQL 之前或之后，自动处理审计信息

### 5.2 复合主键的处理

#### 问题描述

UserRole 实体使用复合主键（UserId, RoleId），需要在 SqlSugar 配置中正确设置。

#### 解决方案

1. **使用 `[SugarColumn(IsPrimaryKey = true)]` 属性**
   - 为 UserId 和 RoleId 都添加 `[SugarColumn(IsPrimaryKey = true)]` 属性
   - SqlSugar 会自动识别复合主键

2. **在 SqlSugar 配置中设置主键**
   - 在配置 SqlSugar 时，为 UserRole 实体指定复合主键
   - 使用 `.ConfigureExternalServices` 方法配置

### 5.3 与 ABP Identity 的兼容性

#### 问题描述

Identity 模块的实体与 ABP Identity 紧密集成，迁移后需要确保与现有系统的兼容性。

#### 解决方案

1. **保持表名和列名不变**
   - 使用 `[SugarTable]` 属性指定表名，保持 `AbpXXX` 格式
   - 使用 `[SugarColumn(ColumnName = "...")]` 属性指定列名，保持与原列名完全一致

2. **保持业务逻辑不变**
   - 实体的业务字段保持不变
   - 实体的导航属性保持不变（标记为 IsIgnore）
   - 实体的构造函数保持不变

3. **渐进式迁移**
   - 不需要一次性迁移所有 Identity 相关功能
   - 可以在维护或重构时逐步迁移
   - 保留旧的代码，直到迁移完成

## 6. 数据库迁移脚本

### 6.1 Identity 模块实体迁移脚本

**文件路径：** `sql/migrate-identity-entities-to-custom-base-classes.sql`

**脚本内容：**

```sql
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
```

## 7. 与前序阶段的关系

### 7.1 与 Phase 2.1 的关系

Phase 2.1 完成了自定义基类的创建，Phase 2.3 使用这些基类来迁移 Identity 模块的实体：

- Phase 2.1 创建了 `Entity`、`AuditedEntity<TKey>`、`CreationAuditedEntity<TKey>` 等基类
- Phase 2.3 使用这些基类来替代 ABP Identity 的基类

### 7.2 与 Phase 2.2 的关系

Phase 2.2 完成了 23 个业务实体的迁移，Phase 2.3 完成了 Identity 模块 6 个实体的迁移：

- Phase 2.2 迁移了 ElectricVehicle、Lottery、Bookkeeping、Configuration、IP、FileFilter、FileUploadDownload、Media、Rss、Account 等 10 个模块的实体
- Phase 2.3 迁移了 Identity 模块的实体
- 两个阶段都遵循相同的迁移原则和规则

### 7.3 与后续阶段的关系

Phase 2.3 完成后，需要进行以下后续工作：

1. **迁移 Identity 相关的应用服务**
   - 将 Identity 相关的应用服务迁移到 `DFApp.Web` 项目
   - 使用新的服务基类（`AppServiceBase` 和 `CrudServiceBase`）
   - 使用新的仓储接口（`ISqlSugarRepository` 和 `ISqlSugarReadOnlyRepository`）

2. **迁移 Identity 相关的控制器**
   - 将 Identity 相关的控制器迁移到 `DFApp.Web` 项目
   - 使用新的控制器基类（`DFAppControllerBase`）
   - 使用新的权限特性（`PermissionAttribute`）

3. **处理审计字段的保存**
   - 实现审计字段的自动填充逻辑
   - 使用拦截器或过滤器处理审计信息
   - 确保审计信息能够正确保存

4. **测试 Identity 功能**
   - 测试用户登录和注册功能
   - 测试角色和权限管理功能
   - 测试权限授予和检查功能

## 8. 下一步建议

### 8.1 Phase 3 的主要任务

Phase 3 将继续推进 ABP Framework 的移除工作，主要任务包括：

1. **迁移 Identity 相关的应用服务**
   - 将 Identity 相关的应用服务迁移到 `DFApp.Web` 项目
   - 使用新的服务基类（`AppServiceBase` 和 `CrudServiceBase`）
   - 使用新的仓储接口（`ISqlSugarRepository` 和 `ISqlSugarReadOnlyRepository`）

2. **迁移 Identity 相关的控制器**
   - 将 Identity 相关的控制器迁移到 `DFApp.Web` 项目
   - 使用新的控制器基类（`DFAppControllerBase`）
   - 使用新的权限特性（`PermissionAttribute`）

3. **解决编译错误**
   - 修复 Identity 相关应用服务层的编译错误
   - 修复 Identity 相关控制器层的编译错误
   - 修复 EF Core 相关的错误

4. **移除 EF Core**
   - 移除 `DFApp.EntityFrameworkCore` 项目
   - 移除 EF Core 相关包
   - 使用 SqlSugar 进行所有数据库操作

5. **移除 ABP Identity**
   - 移除 ABP Identity 相关包
   - 移除 ABP Identity 相关配置
   - 使用自定义的 Identity 实现

6. **更新前端**
   - 更新 API 调用以适配新的后端
   - 更新权限检查逻辑
   - 更新错误处理逻辑

### 8.2 测试建议

1. **单元测试**
   - 为迁移后的 Identity 实体类编写单元测试
   - 测试实体的 CRUD 操作
   - 测试审计字段的自动填充

2. **集成测试**
   - 测试 Identity 应用服务与数据库的集成
   - 测试 Identity 控制器的 API 接口
   - 测试权限系统的集成

3. **功能测试**
   - 测试用户登录和注册功能
   - 测试角色和权限管理功能
   - 测试权限授予和检查功能

### 8.3 数据迁移建议

1. **数据备份**
   - 在执行任何数据库迁移前，请务必备份数据
   - 特别是在修改 Identity 表结构时

2. **渐进式迁移**
   - 不需要一次性迁移所有 Identity 相关功能
   - 可以在维护或重构时逐步迁移
   - 保留旧的数据，直到迁移完成

3. **数据验证**
   - 迁移后验证数据完整性
   - 验证审计字段是否正确填充
   - 验证权限系统是否正常工作

### 8.4 文档更新建议

1. **更新架构文档**
   - 更新项目架构图
   - 更新模块依赖关系
   - 更新技术栈说明

2. **更新 API 文档**
   - 更新 Swagger 文档
   - 更新 API 接口说明
   - 更新权限说明

3. **更新开发文档**
   - 更新开发指南
   - 更新测试指南
   - 更新部署指南

## 9. 附录

### 9.1 完成标准检查清单

- [x] 迁移 Identity 模块的 6 个实体类
- [x] 为所有实体类添加 SqlSugar 属性
- [x] 修改实体基类，从 ABP Identity 基类迁移到自定义基类
- [x] 创建数据库迁移脚本
- [x] 确保数据库结构兼容性
- [x] 生成迁移总结报告

### 9.2 变更历史

| 日期 | 版本 | 变更内容 |
|------|------|----------|
| 2026-03-27 | 1.0 | 初始版本，记录 Phase 2.3 迁移总结 |

### 9.3 参考文档

- [`framework-migration-plan.md`](framework-migration-plan.md) - 框架迁移计划
- [`phase1-migration-summary.md`](phase1-migration-summary.md) - Phase 1 迁移总结
- [`phase2.1-migration-summary.md`](phase2.1-migration-summary.md) - Phase 2.1 迁移总结
- [`phase2.2-migration-summary.md`](phase2.2-migration-summary.md) - Phase 2.2 迁移总结
- [`soft-delete-removal.md`](soft-delete-removal.md) - 软删除废除说明
- [`backend-tdd-testing-guide.md`](backend-tdd-testing-guide.md) - 后端 TDD 测试指南

### 9.4 相关文件

#### 迁移的实体类文件

**Identity 模块：**
- [`src/DFApp.Web/Domain/Identity/Permission.cs`](src/DFApp.Web/Domain/Identity/Permission.cs)
- [`src/DFApp.Web/Domain/Identity/PermissionGrant.cs`](src/DFApp.Web/Domain/Identity/PermissionGrant.cs)
- [`src/DFApp.Web/Domain/Identity/PermissionGroup.cs`](src/DFApp.Web/Domain/Identity/PermissionGroup.cs)
- [`src/DFApp.Web/Domain/Identity/Role.cs`](src/DFApp.Web/Domain/Identity/Role.cs)
- [`src/DFApp.Web/Domain/Identity/RoleClaim.cs`](src/DFApp.Web/Domain/Identity/RoleClaim.cs)
- [`src/DFApp.Web/Domain/Identity/UserRole.cs`](src/DFApp.Web/Domain/Identity/UserRole.cs)

#### 数据库迁移脚本文件

- [`sql/migrate-identity-entities-to-custom-base-classes.sql`](sql/migrate-identity-entities-to-custom-base-classes.sql) - Identity 模块实体迁移脚本

#### 自定义基类文件

- [`src/DFApp.Web/Domain/EntityBase.cs`](src/DFApp.Web/Domain/EntityBase.cs) - 实体基类
- [`src/DFApp.Web/Domain/Entity.cs`](src/DFApp.Web/Domain/Entity.cs) - 简单实体类
- [`src/DFApp.Web/Domain/AuditedEntity.cs`](src/DFApp.Web/Domain/AuditedEntity.cs) - 审计实体类
- [`src/DFApp.Web/Domain/CreationAuditedEntity.cs`](src/DFApp.Web/Domain/CreationAuditedEntity.cs) - 创建审计实体类
- [`src/DFApp.Web/Domain/FullAuditedEntity.cs`](src/DFApp.Web/Domain/FullAuditedEntity.cs) - 完整审计实体类（已废弃）

---

**文档版本**: 1.0  
**最后更新**: 2026 年 3 月 27 日  
**维护者**: DFApp 开发团队
