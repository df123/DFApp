# Phase 2.1 迁移总结文档

## 1. 概述

### 1.1 Phase 2.1 目标和范围

Phase 2.1 是框架迁移计划中的第一个子阶段，主要目标是：

- 确认 Phase 1 中创建的自定义实体基类体系
- 废除软删除功能，简化架构以适应 TDD 开发模式
- 创建相关文档，为后续迁移提供参考

### 1.2 完成时间

Phase 2.1 于 2026 年 3 月 27 日完成。

### 1.3 主要工作内容

- 确认并验证 Phase 1 中创建的自定义实体基类体系
- 废除软删除功能，修改相关代码
- 创建软删除废除说明文档
- 为后续实体迁移提供指导原则

## 2. 完成的工作

### 2.1 确认自定义实体基类体系

Phase 1 中创建的自定义实体基类体系已确认可用，包括：

- **EntityBase<TKey>** - 实体基类，提供基本的实体功能
- **AuditedEntity<TKey>** - 审计实体类，包含创建和修改信息
- **FullAuditedEntity<TKey>** - 完整审计实体类（已标记为废弃）
- **CreationAuditedEntity<TKey>** - 创建审计实体类，仅包含创建信息

### 2.2 废除软删除功能

在 TDD 架构迁移过程中，为了简化架构和减少不必要的复杂性，决定废除软删除功能：

- 禁用全局软删除过滤器
- 标记相关接口和基类为废弃
- 提供迁移指导原则

### 2.3 创建相关文档

- 创建 [`soft-delete-removal.md`](soft-delete-removal.md) 文档，详细记录软删除功能废除的操作和注意事项

## 3. 修改的文件列表

### 3.1 实体基类文件

#### [`src/DFApp.Web/Domain/FullAuditedEntity.cs`](src/DFApp.Web/Domain/FullAuditedEntity.cs)

- **修改内容**：在文件顶部添加废弃注释
- **修改原因**：标记此基类已废弃，建议使用 `AuditedEntity<TKey>` 替代
- **具体修改**：
  ```csharp
  // TODO: 已废弃 - 软删除功能已废除
  // 建议使用 AuditedEntity<TKey> 替代此基类
  ```

#### [`src/DFApp.Web/Domain/ISoftDelete.cs`](src/DFApp.Web/Domain/ISoftDelete.cs)

- **修改内容**：在文件顶部添加废弃注释
- **修改原因**：标记此接口已废弃，软删除功能已废除
- **具体修改**：
  ```csharp
  // TODO: 已废弃 - 软删除功能已废除
  ```

### 3.2 数据库配置文件

#### [`src/DFApp.Web/Data/SqlSugarConfig.cs`](src/DFApp.Web/Data/SqlSugarConfig.cs)

- **修改内容**：禁用 `ConfigureSoftDeleteFilter` 方法
- **修改原因**：软删除功能已废除，不再需要配置软删除过滤器
- **具体修改**：
  ```csharp
  private void ConfigureSoftDeleteFilter(ISqlSugarClient db)
  {
      // 软删除功能已废除，不再配置软删除过滤器
      return;
      // db.QueryFilter.Add(new TableFilterItem<ISoftDelete>(it => it.IsDeleted == false));
  }
  ```

## 4. 创建的文件列表

### 4.1 文档文件

#### [`docs/soft-delete-removal.md`](docs/soft-delete-removal.md)

- **文件用途**：记录软删除功能废除的详细说明
- **关键内容**：
  - 废除原因
  - 修改的文件列表
  - 对现有代码的影响
  - 后续迁移实体时的注意事项
  - 实体基类选择原则
  - 迁移步骤
  - 数据库迁移脚本示例

## 5. 技术细节

### 5.1 自定义实体基类体系

Phase 1 中创建的自定义实体基类体系在 Phase 2.1 中得到确认和验证：

#### 实体接口

1. **[`IEntity<TKey>`](src/DFApp.Web/Domain/IEntity.cs)**
   - 定义实体的基本标识
   - 包含 `Id` 属性

2. **[`IHasCreationTime`](src/DFApp.Web/Domain/IHasCreationTime.cs)**
   - 定义创建时间接口
   - 包含 `CreationTime` 属性

3. **[`ICreatorId`](src/DFApp.Web/Domain/ICreatorId.cs)**
   - 定义创建者 ID 接口
   - 包含 `CreatorId` 属性

4. **[`IHasModificationTime`](src/DFApp.Web/Domain/IHasModificationTime.cs)**
   - 定义修改时间接口
   - 包含 `LastModificationTime` 属性

5. **[`IModifierId`](src/DFApp.Web/Domain/IModifierId.cs)**
   - 定义修改者 ID 接口
   - 包含 `LastModifierId` 属性

6. **[`IHasDeletionTime`](src/DFApp.Web/Domain/IHasDeletionTime.cs)**（已废弃）
   - 定义删除时间接口
   - 包含 `DeletionTime` 属性

7. **[`IDeleterId`](src/DFApp.Web/Domain/IDeleterId.cs)**（已废弃）
   - 定义删除者 ID 接口
   - 包含 `DeleterId` 属性

8. **[`ISoftDelete`](src/DFApp.Web/Domain/ISoftDelete.cs)**（已废弃）
   - 定义软删除接口
   - 包含 `IsDeleted` 属性

9. **[`IAuditedObject`](src/DFApp.Web/Domain/IAuditedObject.cs)**
   - 定义审计对象接口
   - 组合了创建和修改相关接口

10. **[`IFullAuditedObject`](src/DFApp.Web/Domain/IFullAuditedObject.cs)**（已废弃）
    - 定义完整审计对象接口
    - 组合了创建、修改和删除相关接口

11. **[`ICreationAuditedObject`](src/DFApp.Web/Domain/ICreationAuditedObject.cs)**
    - 定义创建审计对象接口
    - 组合了创建相关接口

#### 实体基类

1. **[`EntityBase<TKey>`](src/DFApp.Web/Domain/EntityBase.cs)**
   - 实体基类，实现 `IEntity<TKey>`
   - 提供基本的实体功能
   - 包含 `ConcurrencyStamp` 字段，用于乐观并发控制
   - 通过 SqlSugar 的 AOP 机制自动生成和更新并发标记

2. **[`Entity<TKey>`](src/DFApp.Web/Domain/Entity.cs)**
   - 简单实体类，继承自 `EntityBase<TKey>`

3. **[`AuditedEntity<TKey>`](src/DFApp.Web/Domain/AuditedEntity.cs)**（推荐使用）
   - 审计实体类，继承自 `EntityBase<TKey>`
   - 实现了 `IAuditedObject`
   - 包含创建和修改信息：
     - `CreationTime`
     - `CreatorId`
     - `LastModificationTime`
     - `LastModifierId`

4. **[`FullAuditedEntity<TKey>`](src/DFApp.Web/Domain/FullAuditedEntity.cs)**（已废弃）
   - 完整审计实体类，继承自 `AuditedEntity<TKey>`
   - 实现了 `IFullAuditedObject`
   - 包含创建、修改和删除信息：
     - `IsDeleted`
     - `DeletionTime`
     - `DeleterId`

5. **[`CreationAuditedEntity<TKey>`](src/DFApp.Web/Domain/CreationAuditedEntity.cs)**
   - 创建审计实体类，继承自 `EntityBase<TKey>`
   - 实现了 `ICreationAuditedObject`
   - 仅包含创建信息

6. **[`AuditedEntity.Guid.cs`](src/DFApp.Web/Domain/AuditedEntity.Guid.cs)**
   - Guid 类型的审计实体便捷类

7. **[`FullAuditedEntity.Guid.cs`](src/DFApp.Web/Domain/FullAuditedEntity.Guid.cs)**（已废弃）
   - Guid 类型的完整审计实体便捷类

8. **[`CreationAuditedEntity.Guid.cs`](src/DFApp.Web/Domain/CreationAuditedEntity.Guid.cs)**
   - Guid 类型的创建审计实体便捷类

### 5.2 软删除废除

#### 废除原因

在项目架构从领域驱动设计（DDD）迁移到测试驱动开发（TDD）的过程中，为了简化架构和减少不必要的复杂性，决定废除软删除功能。软删除功能在 TDD 架构中不再作为核心功能，实体将采用直接删除的方式。

#### 修改的代码

1. **[`FullAuditedEntity.cs`](src/DFApp.Web/Domain/FullAuditedEntity.cs)**
   - 在文件顶部添加废弃注释
   - 保留文件以避免破坏可能有旧代码引用的代码

2. **[`ISoftDelete.cs`](src/DFApp.Web/Domain/ISoftDelete.cs)**
   - 在文件顶部添加废弃注释
   - 保留文件以避免破坏可能有旧代码引用的代码

3. **[`SqlSugarConfig.cs`](src/DFApp.Web/Data/SqlSugarConfig.cs)**
   - 禁用 `ConfigureSoftDeleteFilter` 方法
   - 保留方法签名，避免编译错误

#### 对现有代码的影响

1. **实体类影响**
   - 继承 `FullAuditedEntity<TKey>` 的实体仍然可以正常工作，但软删除相关的字段（`IsDeleted`、`DeletionTime`、`DeleterId`）将不再被自动过滤
   - 删除操作将直接从数据库中删除记录，而不是标记为已删除

2. **数据库操作影响**
   - 查询操作：之前被软删除过滤器自动过滤的记录现在可以被查询到
   - 删除操作：删除操作将直接从数据库中删除记录
   - AOP 自动填充：`ConfigureAop` 方法中关于软删除的代码段仍然存在，但由于软删除功能已废除，这些代码实际上不会被使用

#### 后续迁移实体时的注意事项

1. **实体基类选择**
   - 推荐使用 `AuditedEntity<TKey>`：包含审计字段，不包含软删除相关字段
   - 避免使用 `FullAuditedEntity<TKey>`：此基类已标记为废弃
   - 简单实体使用 `Entity<TKey>`：如果不需要审计功能

2. **迁移步骤**
   - 修改基类：从 `FullAuditedEntity<TKey>` 改为 `AuditedEntity<TKey>`
   - 清理数据库字段：可以通过 SQL 迁移脚本删除 `IsDeleted`、`DeletionTime`、`DeleterId` 字段
   - 更新查询逻辑：如果查询中使用了 `WHERE IsDeleted = false`，可以移除此条件

### 5.3 乐观并发控制

#### ConcurrencyStamp 字段定义

所有继承自 `EntityBase<TKey>` 的实体都包含 `ConcurrencyStamp` 字段，用于实现乐观并发控制：

```csharp
/// <summary>
/// 基础实体类，包含 Id 和 ConcurrencyStamp
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public abstract class EntityBase<TKey> : IEntity<TKey>
{
    /// <summary>
    /// 实体唯一标识
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public TKey Id { get; set; }

    /// <summary>
    /// 并发标记，用于乐观并发控制
    /// </summary>
    [SugarColumn(Length = 128)]
    public string ConcurrencyStamp { get; set; }
}
```

#### ABP 标准实现方式

在 ABP Framework 中，`ConcurrencyStamp` 字段的标准实现方式：

- 字段类型：`string`
- 字段长度：128 字符
- 用途：乐观并发控制，防止并发更新冲突
- 生成方式：在实体插入时自动生成 GUID，在更新时自动更新为新的 GUID
- 验证方式：在更新操作时，通过检查 `ConcurrencyStamp` 是否与数据库中的值一致来判断是否有并发冲突

#### SqlSugar 实现方式

当前项目使用 SqlSugar 的 AOP（面向切面编程）机制实现 `ConcurrencyStamp` 的自动生成和更新：

1. **插入操作**：
   - 当执行插入操作时，如果 `ConcurrencyStamp` 为 `null`，则自动生成新的 GUID
   - 代码位于 [`SqlSugarConfig.cs`](src/DFApp.Web/Data/SqlSugarConfig.cs) 的 `ConfigureAop` 方法中

```csharp
// 设置并发标记
if (entityInfo.PropertyName == "ConcurrencyStamp" && entityInfo.EntityValue != null)
{
    var property = entityInfo.EntityValue.GetType().GetProperty("ConcurrencyStamp");
    if (property != null && property.GetValue(entityInfo.EntityValue) == null)
    {
        property.SetValue(entityInfo.EntityValue, Guid.NewGuid().ToString());
    }
}
```

2. **更新操作**：
   - 当执行更新操作时，自动将 `ConcurrencyStamp` 更新为新的 GUID
   - 这样可以确保每次更新都会生成新的并发标记

```csharp
// 更新并发标记
if (entityInfo.PropertyName == "ConcurrencyStamp" && entityInfo.EntityValue != null)
{
    var property = entityInfo.EntityValue.GetType().GetProperty("ConcurrencyStamp");
    if (property != null)
    {
        property.SetValue(entityInfo.EntityValue, Guid.NewGuid().ToString());
    }
}
```

#### 与 ABP 标准的对比

| 特性 | ABP 标准 | SqlSugar 实现 |
|------|----------|---------------|
| 字段类型 | `string` | `string` |
| 字段长度 | 128 字符 | 128 字符 |
| 插入时生成 | 自动生成 GUID | 自动生成 GUID（通过 AOP） |
| 更新时更新 | 自动更新为新的 GUID | 自动更新为新的 GUID（通过 AOP） |
| 实现机制 | ABP 框架内置 | SqlSugar AOP 机制 |

#### 乐观并发控制的优势

1. **防止并发冲突**：通过 `ConcurrencyStamp` 字段，可以检测并发更新冲突
2. **自动管理**：通过 AOP 机制，无需手动管理并发标记的生成和更新
3. **与 ABP 兼容**：字段定义和长度与 ABP 标准一致，便于数据迁移
4. **透明性**：对业务代码透明，无需额外处理

#### 使用建议

- 所有实体都应继承自 `EntityBase<TKey>` 或其派生类，以自动获得并发控制功能
- 在更新实体时，无需手动设置 `ConcurrencyStamp`，AOP 会自动处理
- 如果需要实现自定义的并发控制逻辑，可以在 Service 层中扩展

## 6. 对项目的影响

### 6.1 对现有代码的影响

1. **向后兼容性**
   - 保留了 `FullAuditedEntity<TKey>` 和 `ISoftDelete` 接口，避免破坏可能有旧代码引用的代码
   - 现有代码可以继续使用这些类和接口，但软删除功能不再生效

2. **功能变更**
   - 删除操作从软删除改为物理删除
   - 查询操作不再自动过滤已删除的记录

### 6.2 对数据库的影响

1. **数据库结构**
   - 现有数据库中的软删除相关字段（`IsDeleted`、`DeletionTime`、`DeleterId`）仍然存在
   - 这些字段不再被使用，可以在后续迁移中清理

2. **数据完整性**
   - 物理删除操作会直接从数据库中删除记录
   - 需要确保删除操作不会破坏数据完整性

### 6.3 对后续迁移的影响

1. **实体迁移**
   - 迁移实体时应使用 `AuditedEntity<TKey>` 而不是 `FullAuditedEntity<TKey>`
   - 需要评估是否需要清理软删除相关的数据库字段
   - 所有继承自 `EntityBase<TKey>` 的实体都会自动获得乐观并发控制功能

2. **代码简化**
   - 废除软删除功能后，代码逻辑更加简单
   - 减少了不必要的复杂性，更符合 TDD 开发模式

3. **乐观并发控制机制已就绪**
   - `ConcurrencyStamp` 字段已集成到 `EntityBase<TKey>` 中
   - 通过 SqlSugar 的 AOP 机制自动生成和更新并发标记
   - 后续迁移实体时会自动继承此功能，无需额外配置
   - 与 ABP 标准兼容，便于数据迁移和功能对接

## 7. 后续工作

### 7.1 Phase 2.2：迁移 25+ 实体

Phase 2.2 将迁移 25+ 个实体，从 ABP 基类改为自定义基类：

- 使用 `AuditedEntity<TKey>` 替代 `FullAuditedEntity<TKey>`
- 添加 `[SugarTable]`/`[SugarColumn]` 属性
- 保持数据库列名完全一致
- 评估是否需要清理软删除相关的数据库字段

### 7.2 Phase 2.3：创建自定义 User/Role/Permission 实体

Phase 2.3 将创建自定义 User/Role/Permission 实体，替代 ABP Identity 表：

- 表结构兼容旧数据
- 使用 `AuditedEntity<TKey>` 作为基类
- 不包含软删除相关字段

### 7.3 注意事项

1. **渐进式迁移**
   - 不需要一次性迁移所有实体
   - 可以在维护或重构时逐步迁移

2. **测试覆盖**
   - 在迁移实体后，确保有充分的测试覆盖
   - 特别关注删除操作和查询逻辑

3. **数据备份**
   - 在执行数据库迁移前，请务必备份数据
   - 特别是在删除软删除字段时

## 8. 参考资料

### 8.1 项目文档

- [`framework-migration-plan.md`](framework-migration-plan.md) - 框架迁移计划
- [`phase1-migration-summary.md`](phase1-migration-summary.md) - Phase 1 迁移总结
- [`soft-delete-removal.md`](soft-delete-removal.md) - 软删除废除说明
- [`backend-tdd-testing-guide.md`](backend-tdd-testing-guide.md) - 后端 TDD 测试指南

### 8.2 相关文件

- [`src/DFApp.Web/Domain/FullAuditedEntity.cs`](src/DFApp.Web/Domain/FullAuditedEntity.cs) - 完整审计实体类（已废弃）
- [`src/DFApp.Web/Domain/ISoftDelete.cs`](src/DFApp.Web/Domain/ISoftDelete.cs) - 软删除接口（已废弃）
- [`src/DFApp.Web/Data/SqlSugarConfig.cs`](src/DFApp.Web/Data/SqlSugarConfig.cs) - SqlSugar 配置类

## 9. 附录

### 9.1 完成标准检查清单

- [x] 确认自定义实体基类体系可用
- [x] 废除软删除功能
- [x] 修改相关代码
- [x] 创建软删除废除说明文档
- [x] 提供后续迁移指导原则

### 9.2 变更历史

| 日期 | 版本 | 变更内容 |
|------|------|----------|
| 2026-03-27 | 1.0 | 初始版本，记录 Phase 2.1 迁移总结 |

---

**文档版本**: 1.0  
**最后更新**: 2026 年 3 月 27 日  
**维护者**: DFApp 开发团队
