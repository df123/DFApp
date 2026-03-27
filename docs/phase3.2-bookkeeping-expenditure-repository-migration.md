# Phase 3.2 子任务 3：迁移 EfCoreBookkeepingExpenditureRepository

## 概述
将 `EfCoreBookkeepingExpenditureRepository` 从 EF Core 迁移到 SqlSugar，保留业务逻辑，移除导航查询。

## 完成时间
2026-03-27

## 迁移决策
**决定使用通用仓储，不创建自定义仓储**，原因如下：
1. `IBookkeepingExpenditureRepository` 接口没有定义任何额外的方法
2. `EfCoreBookkeepingExpenditureRepository` 只实现了一个 `WithDetailsAsync` 方法
3. `WithDetailsAsync` 方法仅用于导航查询（`IncludeSub()` 扩展方法）
4. 新的 SqlSugar 实体已标记 `Category` 导航属性为 `[SugarColumn(IsIgnore = true)]`
5. `BookkeepingCategoryService` 使用的查询是简单的 `AnyAsync`，通用仓储完全支持
6. 没有复杂的业务逻辑需要封装在仓储中

## 创建的文件

### 接口文件
**文件路径**: `src/DFApp.Web/Data/Bookkeeping/IBookkeepingExpenditureRepository.cs`

**内容**:
- 继承自 `ISqlSugarRepository<BookkeepingExpenditure, long>`
- 没有定义任何额外的方法（通用仓储已提供所有需要的方法）

## 删除的文件

### 1. 仓储实现类
**文件路径**: `src/DFApp.EntityFrameworkCore/Bookkeeping/EfCoreBookkeepingExpenditureRepository.cs`

**原因**: 该仓储只包含一个用于导航查询的方法，不再需要

### 2. 查询扩展类
**文件路径**: `src/DFApp.EntityFrameworkCore/Bookkeeping/BookkeepingExpenditureQueryableExtensions.cs`

**原因**: 该扩展类包含 `IncludeSub()` 方法，用于导航查询，不再需要

## 业务逻辑保留情况

### 原仓储的方法
1. **WithDetailsAsync()**
   - 功能：返回包含导航属性的查询
   - 实现：调用 `IncludeSub()` 扩展方法
   - 业务逻辑：已移除（不再使用导航查询）

### 新仓储的功能
由于使用通用仓储，所有通用仓储的方法都可用：
- `GetAsync()`
- `GetListAsync()`
- `InsertAsync()`
- `UpdateAsync()`
- `DeleteAsync()`
- `AnyAsync()`
- `CountAsync()`
- 等等

## 导航查询处理

### 原实现
```csharp
public override async Task<IQueryable<BookkeepingExpenditure>> WithDetailsAsync()
{
    return (await GetQueryableAsync()).IncludeSub();
}
```

`IncludeSub()` 扩展方法：
```csharp
public static IQueryable<BookkeepingExpenditure> IncludeSub(this IQueryable<BookkeepingExpenditure> queryable,
    bool include = true)
{
    if (!include)
    {
        return queryable;
    }
    return queryable.Include(x => x.Category);
}
```

### 新实现
新的 SqlSugar 实体已标记 `Category` 导航属性为 `[SugarColumn(IsIgnore = true)]`：
```csharp
[SugarColumn(IsIgnore = true)]
public BookkeepingCategory? Category { get; set; }
```

这意味着：
1. `Category` 属性不会被映射到数据库
2. 不再支持导航查询
3. 如果需要 `Category` 数据，需要通过 `CategoryId` 单独查询

### 影响分析
`BookkeepingCategoryService` 使用的查询：
```csharp
if (await _bookkeepingExpenditureRepository.AnyAsync(x => x.CategoryId == id))
{
    throw new UserFriendlyException("不能删除此类型，因为此类型有开支记录");
}
```

这个查询只使用 `CategoryId`，不使用导航属性，因此不受影响。

## 迁移过程中的技术细节

### 1. 接口继承
**EF Core 原接口**:
```csharp
public interface IBookkeepingExpenditureRepository : IRepository<BookkeepingExpenditure, long>
{
}
```

**SqlSugar 新接口**:
```csharp
public interface IBookkeepingExpenditureRepository : ISqlSugarRepository<BookkeepingExpenditure, long>
{
}
```

### 2. 仓储实现
**EF Core 原实现**:
```csharp
public class EfCoreBookkeepingExpenditureRepository : EfCoreRepository<DFAppDbContext, BookkeepingExpenditure, long>, IBookkeepingExpenditureRepository
{
    public EfCoreBookkeepingExpenditureRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<BookkeepingExpenditure>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeSub();
    }
}
```

**SqlSugar 新实现**:
- 不创建自定义实现类
- 使用通用仓储 `SqlSugarRepository<BookkeepingExpenditure, long>`
- 在 `Program.cs` 中注册：
  ```csharp
  builder.Services.AddScoped<DFApp.Web.Data.Bookkeeping.IBookkeepingExpenditureRepository, SqlSugarRepository<BookkeepingExpenditure, long>>();
  ```

### 3. 导航属性处理
**EF Core 原实体**:
```csharp
public BookkeepingCategory? Category { get; set; }
public long CategoryId { get;set; }
```

**SqlSugar 新实体**:
```csharp
[SugarColumn(IsIgnore = true)]
public BookkeepingCategory? Category { get; set; }

public long CategoryId { get; set; }
```

## 遇到的问题和解决方案

### 问题 1：是否需要创建自定义仓储？
**问题**:
- 原仓储只包含一个 `WithDetailsAsync` 方法
- 该方法用于导航查询
- 是否需要保留这个方法？

**解决方案**:
- 决定不创建自定义仓储
- 原因：
  1. 新的 SqlSugar 实体已标记 `Category` 为 `[SugarColumn(IsIgnore = true)]`
  2. 不再支持导航查询
  3. `WithDetailsAsync` 方法不再需要
  4. 通用仓储完全满足现有需求
  5. 没有复杂的业务逻辑需要封装

### 问题 2：如何处理导航查询的移除？
**问题**:
- 原仓储使用 `IncludeSub()` 扩展方法进行导航查询
- 新架构不再支持导航查询
- 如何确保业务不受影响？

**解决方案**:
- 在新的 SqlSugar 实体中标记 `Category` 为 `[SugarColumn(IsIgnore = true)]`
- 检查所有使用该仓储的代码
- 确认没有代码依赖导航属性
- `BookkeepingCategoryService` 只使用 `CategoryId`，不受影响

## 未完成的任务

### 1. 依赖注入配置
需要在 `Program.cs` 中注册新的仓储：
```csharp
builder.Services.AddScoped<DFApp.Web.Data.Bookkeeping.IBookkeepingExpenditureRepository, SqlSugarRepository<BookkeepingExpenditure, long>>();
```

### 2. 服务层迁移
以下服务仍然使用原来的 `IBookkeepingExpenditureRepository` 接口（ABP 版本）：
- `src/DFApp.Application/Bookkeeping/Category/BookkeepingCategoryService.cs`

这些服务需要在后续阶段迁移到新的架构。

### 3. 编译错误
由于服务层仍然使用旧的接口，会出现编译错误。这是预期中的，按照任务要求："迁移过程中会出现无法编译的情况，不要为了解决而解决"。

## 测试建议

### 1. 单元测试
由于使用通用仓储，不需要为仓储编写特定的单元测试。通用仓储本身应该有自己的单元测试。

### 2. 集成测试
需要测试以下场景：
- 使用 `AnyAsync()` 检查是否存在指定分类的支出记录
- 使用 `GetAsync()` 获取支出记录
- 使用 `GetListAsync()` 获取支出记录列表
- 使用 `InsertAsync()` 创建支出记录
- 使用 `UpdateAsync()` 更新支出记录
- 使用 `DeleteAsync()` 删除支出记录

### 3. 业务逻辑测试
需要测试以下业务场景：
- 删除分类时，如果存在该分类的支出记录，应该抛出异常
- 创建支出记录时，应该能够正确保存到数据库
- 更新支出记录时，应该能够正确更新数据库

## 总结

本次迁移成功完成了以下目标：
1. ✅ 创建了新的 SqlSugar 版本的仓储接口
2. ✅ 决定使用通用仓储，不创建自定义仓储
3. ✅ 删除了旧的 EF Core 仓储实现
4. ✅ 删除了旧的查询扩展类
5. ✅ 移除了导航查询
6. ✅ 新的 SqlSugar 实体已标记导航属性为 `[SugarColumn(IsIgnore = true)]`

## 与其他迁移任务的对比

| 迁移任务 | 是否创建自定义仓储 | 原因 |
|---------|------------------|------|
| EfCoreKeywordFilterRuleRepository | ✅ 是 | 包含复杂的业务逻辑（文件名匹配、过滤规则处理） |
| EfCoreGasolinePriceRepository | ✅ 是 | 包含特定的业务方法（获取最新价格、按日期获取价格） |
| EfCoreBookkeepingExpenditureRepository | ❌ 否 | 只包含导航查询方法，没有复杂业务逻辑 |

## 后续步骤

1. 在 `Program.cs` 中注册新的仓储
2. 迁移 `BookkeepingCategoryService` 到新架构
3. 编写单元测试和集成测试
4. 更新相关文档

## 参考资料

- [Phase 3.2 子任务 1：迁移 EfCoreKeywordFilterRuleRepository](./phase3.2-keyword-filter-rule-repository-migration.md)
- [Phase 3.2 子任务 2：迁移 EfCoreGasolinePriceRepository](./phase3.2-gasoline-price-repository-migration.md)
- [框架迁移计划](./framework-migration-plan.md)
