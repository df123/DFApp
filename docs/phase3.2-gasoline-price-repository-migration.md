# Phase 3.2 子任务 2：迁移 EfCoreGasolinePriceRepository

## 概述
将 `EfCoreGasolinePriceRepository` 从 EF Core 迁移到 SqlSugar，保留业务逻辑，移除导航查询。

## 完成时间
2026-03-27

## 迁移决策
**决定创建自定义仓储**，原因如下：
1. 虽然业务逻辑相对简单，但有多个服务依赖 `IGasolinePriceRepository` 接口
2. 需要保持接口的一致性，避免在多个服务中修改依赖注入
3. 业务方法 `GetLatestPriceAsync` 和 `GetPriceByDateAsync` 提供了特定的查询语义，封装在仓储中更合适
4. 使用只读仓储 `ISqlSugarReadOnlyRepository` 更符合查询操作的特点

## 创建的文件

### 1. 接口文件
**文件路径**: `src/DFApp.Web/Data/ElectricVehicle/IGasolinePriceRepository.cs`

**内容**:
- 继承自 `ISqlSugarReadOnlyRepository<GasolinePrice, Guid>`
- 定义了 2 个业务方法：
  - `GetLatestPriceAsync(string province)` - 获取指定省份的最新汽油价格
  - `GetPriceByDateAsync(string province, DateTime date)` - 获取指定省份和日期的汽油价格

### 2. 实现类文件
**文件路径**: `src/DFApp.Web/Data/ElectricVehicle/GasolinePriceRepository.cs`

**内容**:
- 继承自 `SqlSugarReadOnlyRepository<GasolinePrice, Guid>`
- 实现了 `IGasolinePriceRepository` 接口
- 使用 SqlSugar 的 LINQ 查询替代 EF Core 的查询
- 保留了所有业务方法：
  - `GetLatestPriceAsync()` - 使用 `GetQueryable().Where().OrderByDescending().FirstAsync()`
  - `GetPriceByDateAsync()` - 使用 `GetQueryable().Where().FirstAsync()`

## 修改的文件

### 1. 依赖注入配置
**文件路径**: `src/DFApp.Web/Program.cs`

**修改内容**:
- 添加了自定义仓储的注册：
  ```csharp
  builder.Services.AddScoped<DFApp.Web.Data.ElectricVehicle.IGasolinePriceRepository, DFApp.Web.Data.ElectricVehicle.GasolinePriceRepository>();
  ```

## 业务逻辑保留情况

### 保留的方法
1. **GetLatestPriceAsync(string province)**
   - 功能：获取指定省份的最新汽油价格
   - 实现：使用 SqlSugar 的 `GetQueryable().Where().OrderByDescending().FirstAsync()`
   - 业务逻辑：完全保留
     - 按省份筛选
     - 按日期降序排序
     - 返回第一条记录

2. **GetPriceByDateAsync(string province, DateTime date)**
   - 功能：获取指定省份和日期的汽油价格
   - 实现：使用 SqlSugar 的 `GetQueryable().Where().FirstAsync()`
   - 业务逻辑：完全保留
     - 按省份和日期筛选
     - 日期比较使用 `.Date` 属性忽略时间部分
     - 返回第一条记录

## 迁移过程中的技术细节

### 1. SqlSugar 查询语法
**EF Core 原代码**:
```csharp
var dbSet = await GetDbSetAsync();
return await dbSet
    .Where(x => x.Province == province)
    .OrderByDescending(x => x.Date)
    .FirstOrDefaultAsync();
```

**SqlSugar 新代码**:
```csharp
return await GetQueryable()
    .Where(x => x.Province == province)
    .OrderByDescending(x => x.Date)
    .FirstAsync();
```

### 2. 排序处理
- EF Core 使用 `OrderByDescending()` 进行降序排序
- SqlSugar 也使用 `OrderByDescending()` 进行降序排序

### 3. 查询方法
- EF Core 使用 `GetDbSetAsync()` 获取 `DbSet<T>`
- SqlSugar 使用 `GetQueryable()` 获取 `ISugarQueryable<T>`

### 4. 异步处理
- EF Core 的 `FirstOrDefaultAsync()` 是异步方法
- SqlSugar 的 `FirstAsync()` 是异步方法

### 5. 只读仓储
- 由于这两个方法都是查询操作，使用 `ISqlSugarReadOnlyRepository` 更合适
- 不需要修改数据，因此不需要使用 `ISqlSugarRepository`

## 遇到的问题和解决方案

### 问题 1：构造函数参数类型错误
**问题描述**:
```
The type or namespace name 'ISqlSugarClientProvider' could not be found
```

**解决方案**:
- 使用 `ISqlSugarClient` 而不是 `ISqlSugarClientProvider`
- `SqlSugarReadOnlyRepository` 的构造函数接受 `ISqlSugarClient` 参数

### 问题 2：Queryable 方法调用错误
**问题描述**:
```
Non-invocable member 'Queryable' cannot be used like a method.
```

**解决方案**:
- 使用 `GetQueryable()` 方法而不是 `Queryable()`
- `SqlSugarReadOnlyRepository` 提供了 `GetQueryable()` 方法来获取可查询对象

## 未完成的任务

### 1. 服务层迁移
以下服务仍然使用原来的 `IGasolinePriceRepository` 接口（ABP 版本）：
- `src/DFApp.Application/ElectricVehicle/GasolinePriceService.cs`
- `src/DFApp.Application/ElectricVehicle/GasolinePriceRefresher.cs`
- `src/DFApp.Application/ElectricVehicle/ElectricVehicleCostService.cs`

这些服务需要在后续阶段迁移到新的架构。

### 2. 编译错误
由于服务层仍然使用旧的接口，会出现编译错误。这是预期中的，按照任务要求："迁移过程中会出现无法编译的情况，不要为了解决而解决"。

## 测试建议

### 1. 单元测试
需要为以下方法编写单元测试：
- `GetLatestPriceAsync(string province)`
- `GetPriceByDateAsync(string province, DateTime date)`

### 2. 集成测试
需要测试以下场景：
- 获取指定省份的最新价格（应该返回日期最新的记录）
- 获取指定省份和日期的价格（应该返回匹配的记录）
- 当没有匹配记录时，应该返回 null
- 日期比较应该忽略时间部分

## 总结

本次迁移成功完成了以下目标：
1. ✅ 创建了新的 SqlSugar 版本的仓储接口和实现类
2. ✅ 保留了所有业务方法和逻辑
3. ✅ 使用 SqlSugar 的查询替代了 EF Core 的查询
4. ✅ 移除了导航查询（原仓储本身就没有导航查询）
5. ✅ 决定创建自定义仓储（而不是使用通用仓储）
6. ✅ 使用只读仓储 `ISqlSugarReadOnlyRepository` 更符合查询操作的特点
7. ✅ 注册了自定义仓储到依赖注入容器
8. ✅ 无需修改数据库表结构（无需生成 SQL 文件）

迁移过程中遇到的问题都已解决，业务逻辑完全保留。服务层的迁移将在后续阶段进行。
