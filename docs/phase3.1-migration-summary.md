# Phase 3.1 迁移总结文档

## 1. 概述

### 1.1 Phase 3.1 目标和范围

Phase 3.1 是框架迁移计划中数据访问层迁移的第一个子阶段，主要目标是：

- 确认并评估 Phase 1 中创建的 SqlSugar 通用仓储
- 移除软删除功能（已在 Phase 2.1 废除）
- 添加排序支持到分页方法
- 创建相关文档，为后续迁移提供参考

### 1.2 完成时间

Phase 3.1 于 2026 年 3 月 27 日完成。

### 1.3 主要工作内容

- 评估现有 SqlSugar 通用仓储是否满足 Phase 3.1 的需求
- 移除软删除相关方法（根据 Phase 2.1 的迁移总结）
- 添加排序支持到分页方法
- 创建 Phase 3.1 迁移总结文档

## 2. 完成的工作

### 2.1 评估现有 SqlSugar 通用仓储

Phase 1 中创建的 SqlSugar 通用仓储体系已确认可用，包括：

- **ISqlSugarRepository<T, TKey>** - SqlSugar 仓储接口，提供完整的 CRUD 操作
- **SqlSugarRepository<T, TKey>** - SqlSugar 仓储实现
- **ISqlSugarReadOnlyRepository<T, TKey>** - SqlSugar 只读仓储接口，仅提供查询功能
- **SqlSugarReadOnlyRepository<T, TKey>** - SqlSugar 只读仓储实现

### 2.2 移除软删除功能

根据 Phase 2.1 的迁移总结，软删除功能已废除，需要从仓储中移除软删除相关方法：

- 从 `ISqlSugarRepository<T, TKey>` 接口中移除软删除方法声明
- 从 `SqlSugarRepository<T, TKey>` 实现中移除软删除方法实现

### 2.3 添加排序支持到分页方法

为了提供更好的灵活性，在分页方法中添加了排序支持：

- 在 `ISqlSugarRepository<T, TKey>` 接口中添加支持排序的分页方法
- 在 `SqlSugarRepository<T, TKey>` 实现中实现支持排序的分页方法
- 在 `ISqlSugarReadOnlyRepository<T, TKey>` 接口中添加支持排序的分页方法
- 在 `SqlSugarReadOnlyRepository<T, TKey>` 实现中实现支持排序的分页方法

### 2.4 创建相关文档

- 创建 [`phase3.1-migration-summary.md`](phase3.1-migration-summary.md) 文档，详细记录 Phase 3.1 迁移总结

## 3. 修改的文件列表

### 3.1 仓储接口文件

#### [`src/DFApp.Web/Data/ISqlSugarRepository.cs`](src/DFApp.Web/Data/ISqlSugarRepository.cs)

- **修改内容**：
  1. 移除软删除方法声明（`SoftDeleteAsync` 相关方法）
  2. 添加支持排序的分页方法声明

- **修改原因**：
  1. 根据 Phase 2.1 的迁移总结，软删除功能已废除
  2. 提供更好的灵活性，支持分页时排序

- **具体修改**：

  **移除的软删除方法**：
  ```csharp
  // 已移除
  Task<int> SoftDeleteAsync(T entity);
  Task<int> SoftDeleteAsync(TKey id);
  Task<int> SoftDeleteAsync(List<T> entities);
  Task<int> SoftDeleteAsync(Expression<Func<T, bool>> expression);
  ```

  **新增的支持排序的分页方法**：
  ```csharp
  /// <summary>
  /// 分页查询（带排序）
  /// </summary>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  Task<(List<T> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc);

  /// <summary>
  /// 根据条件分页查询（带排序）
  /// </summary>
  /// <param name="expression">查询条件</param>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  Task<(List<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc);
  ```

#### [`src/DFApp.Web/Data/ISqlSugarReadOnlyRepository.cs`](src/DFApp.Web/Data/ISqlSugarReadOnlyRepository.cs)

- **修改内容**：添加支持排序的分页方法声明

- **修改原因**：提供更好的灵活性，支持分页时排序

- **具体修改**：

  **新增的支持排序的分页方法**：
  ```csharp
  /// <summary>
  /// 分页查询（带排序）
  /// </summary>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  Task<(List<T> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc);

  /// <summary>
  /// 根据条件分页查询（带排序）
  /// </summary>
  /// <param name="expression">查询条件</param>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  Task<(List<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc);
  ```

### 3.2 仓储实现文件

#### [`src/DFApp.Web/Data/SqlSugarRepository.cs`](src/DFApp.Web/Data/SqlSugarRepository.cs)

- **修改内容**：
  1. 移除软删除方法实现（`SoftDeleteAsync` 相关方法）
  2. 实现支持排序的分页方法

- **修改原因**：
  1. 根据 Phase 2.1 的迁移总结，软删除功能已废除
  2. 实现接口中新增的支持排序的分页方法

- **具体修改**：

  **移除的软删除方法实现**：
  ```csharp
  // 已移除
  public async Task<int> SoftDeleteAsync(T entity)
  public async Task<int> SoftDeleteAsync(TKey id)
  public async Task<int> SoftDeleteAsync(List<T> entities)
  public async Task<int> SoftDeleteAsync(Expression<Func<T, bool>> expression)
  ```

  **新增的支持排序的分页方法实现**：
  ```csharp
  /// <summary>
  /// 分页查询（带排序）
  /// </summary>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)
  {
      RefAsync<int> totalCount = 0;
      var query = _db.Queryable<T>();
      if (orderByType == OrderByType.Asc)
      {
          query = query.OrderBy(orderByExpression, OrderByType.Asc);
      }
      else
      {
          query = query.OrderBy(orderByExpression, OrderByType.Desc);
      }
      var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);
      return (items, totalCount.Value);
  }

  /// <summary>
  /// 根据条件分页查询（带排序）
  /// </summary>
  /// <param name="expression">查询条件</param>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)
  {
      RefAsync<int> totalCount = 0;
      var query = _db.Queryable<T>().Where(expression);
      if (orderByType == OrderByType.Asc)
      {
          query = query.OrderBy(orderByExpression, OrderByType.Asc);
      }
      else
      {
          query = query.OrderBy(orderByExpression, OrderByType.Desc);
      }
      var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);
      return (items, totalCount.Value);
  }
  ```

#### [`src/DFApp.Web/Data/SqlSugarReadOnlyRepository.cs`](src/DFApp.Web/Data/SqlSugarReadOnlyRepository.cs)

- **修改内容**：实现支持排序的分页方法

- **修改原因**：实现接口中新增的支持排序的分页方法

- **具体修改**：

  **新增的支持排序的分页方法实现**：
  ```csharp
  /// <summary>
  /// 分页查询（带排序）
  /// </summary>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)
  {
      RefAsync<int> totalCount = 0;
      var query = _db.Queryable<T>();
      if (orderByType == OrderByType.Asc)
      {
          query = query.OrderBy(orderByExpression, OrderByType.Asc);
      }
      else
      {
          query = query.OrderBy(orderByExpression, OrderByType.Desc);
      }
      var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);
      return (items, totalCount.Value);
  }

  /// <summary>
  /// 根据条件分页查询（带排序）
  /// </summary>
  /// <param name="expression">查询条件</param>
  /// <param name="pageIndex">页码（从 1 开始）</param>
  /// <param name="pageSize">每页大小</param>
  /// <param name="orderByExpression">排序表达式</param>
  /// <param name="orderByType">排序类型（升序或降序）</param>
  /// <returns>分页结果</returns>
  public async Task<(List<T> Items, int TotalCount)> GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)
  {
      RefAsync<int> totalCount = 0;
      var query = _db.Queryable<T>().Where(expression);
      if (orderByType == OrderByType.Asc)
      {
          query = query.OrderBy(orderByExpression, OrderByType.Asc);
      }
      else
      {
          query = query.OrderBy(orderByExpression, OrderByType.Desc);
      }
      var items = await query.ToPageListAsync(pageIndex, pageSize, totalCount);
      return (items, totalCount.Value);
  }
  ```

## 4. 创建的文件列表

### 4.1 文档文件

#### [`docs/phase3.1-migration-summary.md`](docs/phase3.1-migration-summary.md)

- **文件用途**：记录 Phase 3.1 迁移总结
- **关键内容**：
  - Phase 3.1 目标和范围
  - 完成的工作
  - 修改的文件列表
  - 技术细节
  - 对项目的影响
  - 后续工作
  - 参考资料

## 5. 技术细节

### 5.1 SqlSugar 通用仓储体系

Phase 1 中创建的 SqlSugar 通用仓储体系在 Phase 3.1 中得到确认和优化：

#### ISqlSugarRepository<T, TKey>

读写仓储接口，提供完整的 CRUD 操作：

1. **查询操作**：
   - `GetByIdAsync(TKey id)` - 根据 ID 获取实体
   - `GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression)` - 根据条件获取单个实体
   - `GetListAsync()` - 获取所有实体列表
   - `GetListAsync(Expression<Func<T, bool>> expression)` - 根据条件获取实体列表
   - `GetPagedListAsync(int pageIndex, int pageSize)` - 分页查询
   - `GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize)` - 根据条件分页查询
   - `GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType)` - 分页查询（带排序）
   - `GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType)` - 根据条件分页查询（带排序）
   - `GetQueryable()` - 获取可查询对象
   - `GetQueryable(Expression<Func<T, bool>> expression)` - 获取可查询对象（带条件）
   - `CountAsync()` - 统计数量
   - `CountAsync(Expression<Func<T, bool>> expression)` - 根据条件统计数量
   - `AnyAsync(Expression<Func<T, bool>> expression)` - 判断是否存在

2. **插入操作**：
   - `InsertAsync(T entity)` - 插入实体
   - `InsertAsync(List<T> entities)` - 批量插入实体

3. **更新操作**：
   - `UpdateAsync(T entity)` - 更新实体
   - `UpdateAsync(List<T> entities)` - 批量更新实体
   - `UpdateAsync(Expression<Func<T, bool>> expression, T entity)` - 根据条件更新实体

4. **删除操作**：
   - `DeleteAsync(T entity)` - 删除实体
   - `DeleteAsync(TKey id)` - 根据 ID 删除实体
   - `DeleteAsync(List<T> entities)` - 批量删除实体
   - `DeleteAsync(Expression<Func<T, bool>> expression)` - 根据条件删除实体

5. **事务操作**：
   - `BeginTran()` - 开始事务
   - `CommitTran()` - 提交事务
   - `RollbackTran()` - 回滚事务

#### ISqlSugarReadOnlyRepository<T, TKey>

只读仓储接口，仅提供查询功能：

1. **查询操作**：
   - `GetByIdAsync(TKey id)` - 根据 ID 获取实体
   - `GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression)` - 根据条件获取单个实体
   - `GetListAsync()` - 获取所有实体列表
   - `GetListAsync(Expression<Func<T, bool>> expression)` - 根据条件获取实体列表
   - `GetPagedListAsync(int pageIndex, int pageSize)` - 分页查询
   - `GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize)` - 根据条件分页查询
   - `GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType)` - 分页查询（带排序）
   - `GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType)` - 根据条件分页查询（带排序）
   - `GetQueryable()` - 获取可查询对象
   - `GetQueryable(Expression<Func<T, bool>> expression)` - 获取可查询对象（带条件）
   - `CountAsync()` - 统计数量
   - `CountAsync(Expression<Func<T, bool>> expression)` - 根据条件统计数量
   - `AnyAsync(Expression<Func<T, bool>> expression)` - 判断是否存在

### 5.2 软删除移除

#### 移除原因

根据 Phase 2.1 的迁移总结，软删除功能已废除，需要从仓储中移除软删除相关方法。软删除功能在 TDD 架构中不再作为核心功能，实体将采用直接删除的方式。

#### 移除的方法

从 `ISqlSugarRepository<T, TKey>` 接口和 `SqlSugarRepository<T, TKey>` 实现中移除了以下方法：

1. `SoftDeleteAsync(T entity)` - 软删除实体
2. `SoftDeleteAsync(TKey id)` - 根据 ID 软删除实体
3. `SoftDeleteAsync(List<T> entities)` - 批量软删除实体
4. `SoftDeleteAsync(Expression<Func<T, bool>> expression)` - 根据条件软删除实体

#### 对现有代码的影响

1. **删除操作**：
   - 删除操作将直接从数据库中删除记录，而不是标记为已删除
   - 使用 `DeleteAsync` 方法进行物理删除

2. **查询操作**：
   - 查询操作不再自动过滤已删除的记录（因为已删除的记录已被物理删除）

### 5.3 排序支持

#### 新增方法

在 `ISqlSugarRepository<T, TKey>`、`SqlSugarRepository<T, TKey>`、`ISqlSugarReadOnlyRepository<T, TKey>` 和 `SqlSugarReadOnlyRepository<T, TKey>` 中添加了以下方法：

1. `GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)` - 分页查询（带排序）
2. `GetPagedListAsync(Expression<Func<T, bool>> expression, int pageIndex, int pageSize, Expression<Func<T, object>> orderByExpression, OrderByType orderByType = OrderByType.Asc)` - 根据条件分页查询（带排序）

#### 排序类型

使用 SqlSugar 的 `OrderByType` 枚举来指定排序类型：

- `OrderByType.Asc` - 升序（默认）
- `OrderByType.Desc` - 降序

#### 使用示例

```csharp
// 升序排序
var result = await repository.GetPagedListAsync(1, 10, x => x.CreationTime, OrderByType.Asc);

// 降序排序
var result = await repository.GetPagedListAsync(1, 10, x => x.CreationTime, OrderByType.Desc);

// 带条件的升序排序
var result = await repository.GetPagedListAsync(x => x.IsEnabled, 1, 10, x => x.Priority, OrderByType.Asc);
```

## 6. 对项目的影响

### 6.1 对现有代码的影响

1. **向后兼容性**
   - 移除了软删除方法，如果有代码使用了这些方法，需要修改为使用物理删除方法
   - 新增的排序方法是可选的，不影响现有代码

2. **功能变更**
   - 删除操作从软删除改为物理删除（与 Phase 2.1 一致）
   - 分页查询现在支持排序，提供了更好的灵活性

### 6.2 对数据库的影响

1. **数据库结构**
   - 无影响，因为只是仓储层面的修改

2. **数据操作**
   - 删除操作将直接从数据库中删除记录
   - 分页查询可以指定排序字段和排序方向

### 6.3 对后续迁移的影响

1. **仓储迁移**
   - Phase 3.2 将迁移 6 个自定义仓储，保留业务方法，用 SqlSugar 的查询替代 EF Core 的查询
   - 通用仓储已经提供了完整的 CRUD 操作和分页查询功能，可以满足大部分业务需求

2. **服务层迁移**
   - Phase 4 将迁移服务层，使用新的通用仓储替代旧的 EF Core 仓储
   - 服务层可以使用支持排序的分页方法，提供更好的用户体验

3. **代码简化**
   - 移除软删除功能后，代码逻辑更加简单
   - 排序支持使得分页查询更加灵活，减少了业务层的代码

## 7. 后续工作

### 7.1 Phase 3.2：迁移 6 个自定义仓储

Phase 3.2 将迁移 6 个自定义仓储，保留业务方法：

1. **EfCoreKeywordFilterRuleRepository** - 包含复杂的业务逻辑（文件过滤规则）✅ 已完成
2. **EfCoreGasolinePriceRepository** - 包含自定义业务逻辑（油价查询）✅ 已完成
3. **EfCoreBookkeepingExpenditureRepository** - 简单的 Repository（包含导航查询）✅ 已完成
4. **EfCoreConfigurationInfoRepository** - 包含自定义业务逻辑（配置信息查询）
5. **TellStatusResultRepository** - Aria2 相关
6. **FilesItemRepository** - Aria2 相关

#### 已完成的迁移

**EfCoreKeywordFilterRuleRepository**（子任务 1）
- 决定创建自定义仓储
- 保留了所有业务方法和逻辑
- 详见：[phase3.2-keyword-filter-rule-repository-migration.md](phase3.2-keyword-filter-rule-repository-migration.md)

**EfCoreGasolinePriceRepository**（子任务 2）
- 决定创建自定义仓储
- 保留了所有业务方法和逻辑
- 详见：[phase3.2-gasoline-price-repository-migration.md](phase3.2-gasoline-price-repository-migration.md)

**EfCoreBookkeepingExpenditureRepository**（子任务 3）
- 决定使用通用仓储，不创建自定义仓储
- 原因：只包含导航查询方法，没有复杂业务逻辑
- 移除了导航查询（`WithDetailsAsync` 方法）
- 详见：[phase3.2-bookkeeping-expenditure-repository-migration.md](phase3.2-bookkeeping-expenditure-repository-migration.md)

#### 迁移决策对比

| 迁移任务 | 是否创建自定义仓储 | 原因 |
|---------|------------------|------|
| EfCoreKeywordFilterRuleRepository | ✅ 是 | 包含复杂的业务逻辑（文件名匹配、过滤规则处理） |
| EfCoreGasolinePriceRepository | ✅ 是 | 包含特定的业务方法（获取最新价格、按日期获取价格） |
| EfCoreBookkeepingExpenditureRepository | ❌ 否 | 只包含导航查询方法，没有复杂业务逻辑 |

迁移时需要：
- 保留业务方法（如 `GetAllParametersInModule`）
- 用 SqlSugar 的查询替代 EF Core 的查询
- 移除导航查询（已废除）
- 使用新的通用仓储作为基类

### 7.2 Phase 3.3：替换所有服务中的仓储注入

Phase 3.3 将替换所有服务中的仓储注入：

- `AsyncExecuter.ToListAsync()` → SqlSugar `.ToListAsync()`
- `GetQueryableAsync()` → SqlSugar 查询
- `IUnitOfWorkManager` → SqlSugar 事务

### 7.3 注意事项

1. **渐进式迁移**
   - 不需要一次性迁移所有服务
   - 可以在维护或重构时逐步迁移

2. **测试覆盖**
   - 在迁移服务后，确保有充分的测试覆盖
   - 特别关注删除操作和查询逻辑

3. **数据备份**
   - 在执行数据库迁移前，请务必备份数据

## 8. 参考资料

### 8.1 项目文档

- [`framework-migration-plan.md`](framework-migration-plan.md) - 框架迁移计划
- [`phase1-migration-summary.md`](phase1-migration-summary.md) - Phase 1 迁移总结
- [`phase2.1-migration-summary.md`](phase2.1-migration-summary.md) - Phase 2.1 迁移总结
- [`soft-delete-removal.md`](soft-delete-removal.md) - 软删除废除说明
- [`backend-tdd-testing-guide.md`](backend-tdd-testing-guide.md) - 后端 TDD 测试指南
- [`phase3.2-keyword-filter-rule-repository-migration.md`](phase3.2-keyword-filter-rule-repository-migration.md) - Phase 3.2 子任务 1：迁移 EfCoreKeywordFilterRuleRepository
- [`phase3.2-gasoline-price-repository-migration.md`](phase3.2-gasoline-price-repository-migration.md) - Phase 3.2 子任务 2：迁移 EfCoreGasolinePriceRepository
- [`phase3.2-bookkeeping-expenditure-repository-migration.md`](phase3.2-bookkeeping-expenditure-repository-migration.md) - Phase 3.2 子任务 3：迁移 EfCoreBookkeepingExpenditureRepository

### 8.2 相关文件

- [`src/DFApp.Web/Data/ISqlSugarRepository.cs`](src/DFApp.Web/Data/ISqlSugarRepository.cs) - SqlSugar 仓储接口
- [`src/DFApp.Web/Data/SqlSugarRepository.cs`](src/DFApp.Web/Data/SqlSugarRepository.cs) - SqlSugar 仓储实现
- [`src/DFApp.Web/Data/ISqlSugarReadOnlyRepository.cs`](src/DFApp.Web/Data/ISqlSugarReadOnlyRepository.cs) - SqlSugar 只读仓储接口
- [`src/DFApp.Web/Data/SqlSugarReadOnlyRepository.cs`](src/DFApp.Web/Data/SqlSugarReadOnlyRepository.cs) - SqlSugar 只读仓储实现

## 9. 附录

### 9.1 完成标准检查清单

- [x] 评估现有仓储文件是否满足 Phase 3.1 需求
- [x] 根据需要进行优化和完善
- [x] 移除软删除方法
- [x] 添加排序支持到分页方法
- [x] 检查并评估简单的单独 Repository
- [x] 创建 Phase 3.1 迁移总结文档
- [x] 记录所有修改的文件
- [x] 记录所有新增的功能
- [x] 为后续迁移提供指导

### 9.2 变更历史

| 日期 | 版本 | 变更内容 |
|------|------|----------|
| 2026-03-27 | 1.0 | 初始版本，记录 Phase 3.1 迁移总结 |

---

**文档版本**: 1.0  
**最后更新**: 2026 年 3 月 27 日  
**维护者**: DFApp 开发团队
