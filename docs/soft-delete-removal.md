# 软删除功能废除说明

## 概述

本文档记录了 DFApp 项目从 DDD 架构迁移到 TDD 架构过程中，软删除功能的废除操作。

## 废除原因

在项目架构从领域驱动设计（DDD）迁移到测试驱动开发（TDD）的过程中，为了简化架构和减少不必要的复杂性，决定废除软删除功能。软删除功能在 TDD 架构中不再作为核心功能，实体将采用直接删除的方式。

## 修改的文件列表

### 1. 实体基类文件

#### `src/DFApp.Web/Domain/FullAuditedEntity.cs`
- **修改内容**：在文件顶部添加废弃注释
- **注释内容**：
  ```csharp
  // TODO: 已废弃 - 软删除功能已废除
  // 建议使用 AuditedEntity<TKey> 替代此基类
  ```
- **保留原因**：可能有旧代码引用，保留文件但不推荐使用

#### `src/DFApp.Web/Domain/ISoftDelete.cs`
- **修改内容**：在文件顶部添加废弃注释
- **注释内容**：
  ```csharp
  // TODO: 已废弃 - 软删除功能已废除
  ```
- **保留原因**：可能有旧代码引用，保留文件但不推荐使用

### 2. 数据库配置文件

#### `src/DFApp.Web/Data/SqlSugarConfig.cs`
- **修改内容**：禁用 `ConfigureSoftDeleteFilter` 方法
- **修改详情**：
  ```csharp
  /// <summary>
  /// 配置全局软删除过滤器
  /// </summary>
  /// <param name="db">SqlSugar 客户端</param>
  private void ConfigureSoftDeleteFilter(ISqlSugarClient db)
  {
      // 软删除功能已废除，不再配置软删除过滤器
      return;
      // db.QueryFilter.Add(new TableFilterItem<ISoftDelete>(it => it.IsDeleted == false));
  }
  ```
- **保留原因**：保留方法签名，避免编译错误，只添加 `return;` 禁用功能

## 对现有代码的影响

### 实体类影响

1. **继承 `FullAuditedEntity<TKey>` 的实体**
   - 这些实体仍然可以正常工作，但软删除相关的字段（`IsDeleted`、`DeletionTime`、`DeleterId`）将不再被自动过滤
   - 删除操作将直接从数据库中删除记录，而不是标记为已删除

2. **实现 `ISoftDelete` 接口的实体**
   - 接口仍然存在，但不再被 SqlSugar 的全局过滤器使用
   - 如果需要使用软删除功能，需要手动实现过滤逻辑

### 数据库操作影响

1. **查询操作**
   - 之前被软删除过滤器自动过滤的记录现在可以被查询到
   - 如果需要过滤已删除记录，需要在查询条件中手动添加 `WHERE IsDeleted = false`

2. **删除操作**
   - 删除操作将直接从数据库中删除记录
   - `IsDeleted`、`DeletionTime`、`DeleterId` 字段将不再被自动设置

3. **AOP 自动填充**
   - `ConfigureAop` 方法中关于软删除的代码段（第 147-177 行）仍然存在，但由于软删除功能已废除，这些代码实际上不会被使用
   - 如果实体继承自 `FullAuditedEntity<TKey>`，这些字段仍然会被设置，但不会被过滤器使用

## 后续迁移实体时的注意事项

### 实体基类选择

在迁移或创建新实体时，应遵循以下原则：

1. **推荐使用 `AuditedEntity<TKey>`**
   - 这是新的推荐基类
   - 包含审计字段：`CreationTime`、`LastModificationTime`、`CreatorId`、`LastModifierId`
   - 不包含软删除相关字段

2. **避免使用 `FullAuditedEntity<TKey>`**
   - 此基类已标记为废弃
   - 包含软删除相关字段：`IsDeleted`、`DeletionTime`、`DeleterId`
   - 这些字段在 TDD 架构中不再使用

3. **简单实体使用 `Entity<TKey>`**
   - 如果不需要审计功能，可以使用最基础的实体基类
   - 只包含主键字段

### 迁移步骤

对于继承自 `FullAuditedEntity<TKey>` 的旧实体，迁移步骤如下：

1. **修改基类**
   ```csharp
   // 旧代码
   public class MyEntity : FullAuditedEntity<Guid>
   {
   }

   // 新代码
   public class MyEntity : AuditedEntity<Guid>
   {
   }
   ```

2. **清理数据库字段**
   - 如果实体不再需要软删除字段，可以通过 SQL 迁移脚本删除 `IsDeleted`、`DeletionTime`、`DeleterId` 字段
   - 注意：删除字段前请确保已备份重要数据

3. **更新查询逻辑**
   - 如果查询中使用了 `WHERE IsDeleted = false`，可以移除此条件
   - 如果需要保留软删除行为，需要手动实现过滤逻辑

### 数据库迁移

如果需要清理软删除相关的数据库字段，可以创建以下 SQL 迁移脚本：

```sql
-- 示例：移除特定表的软删除字段
-- 注意：请根据实际情况修改表名
ALTER TABLE MyEntity DROP COLUMN IsDeleted;
ALTER TABLE MyEntity DROP COLUMN DeletionTime;
ALTER TABLE MyEntity DROP COLUMN DeleterId;
```

## 相关文档

- [框架迁移计划](framework-migration-plan.md)
- [Phase 1 迁移总结](phase1-migration-summary.md)
- [后端 TDD 测试指南](backend-tdd-testing-guide.md)

## 变更历史

| 日期 | 版本 | 变更内容 |
|------|------|----------|
| 2026-03-27 | 1.0 | 初始版本，记录软删除功能废除操作 |

## 注意事项

1. **不要删除废弃文件**
   - `FullAuditedEntity.cs` 和 `ISoftDelete.cs` 文件保留，只标记为废弃
   - 这样可以避免破坏可能有旧代码引用的代码

2. **渐进式迁移**
   - 不需要一次性迁移所有实体
   - 可以在维护或重构时逐步迁移

3. **测试覆盖**
   - 在迁移实体后，确保有充分的测试覆盖
   - 特别关注删除操作和查询逻辑

4. **数据备份**
   - 在执行数据库迁移前，请务必备份数据
   - 特别是在删除软删除字段时
