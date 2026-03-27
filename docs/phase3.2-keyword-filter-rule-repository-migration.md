# Phase 3.2 子任务 1：迁移 EfCoreKeywordFilterRuleRepository

## 概述
将 `EfCoreKeywordFilterRuleRepository` 从 EF Core 迁移到 SqlSugar，保留业务逻辑，移除导航查询。

## 完成时间
2026-03-27

## 迁移决策
**决定创建自定义仓储**，原因如下：
1. `ShouldFilterFileAsync` 和 `ShouldFilterFilesAsync` 包含复杂的业务逻辑
2. 这些方法不仅仅是简单的数据访问，而是包含了业务规则：
   - 支持黑名单和白名单模式
   - 支持多种匹配模式（Contains、StartsWith、EndsWith、Exact、Regex）
   - 支持大小写敏感/不敏感
   - 按优先级排序处理规则
   - 正则表达式匹配需要异常处理
3. 私有方法 `IsMatch` 包含了复杂的匹配逻辑
4. 这些方法需要在多个地方使用，应该封装在仓储中

## 创建的文件

### 1. 接口文件
**文件路径**: `src/DFApp.Web/Data/FileFilter/IKeywordFilterRuleRepository.cs`

**内容**:
- 继承自 `ISqlSugarRepository<KeywordFilterRule, long>`
- 定义了 4 个业务方法：
  - `GetAllEnabledRulesAsync()` - 获取所有启用的过滤规则（按优先级排序）
  - `GetEnabledRulesByTypeAsync(FilterType filterType)` - 根据过滤类型获取启用的规则
  - `ShouldFilterFileAsync(string fileName)` - 检查文件名是否匹配任何规则
  - `ShouldFilterFilesAsync(IEnumerable<string> fileNames)` - 批量检查多个文件名

### 2. 实现类文件
**文件路径**: `src/DFApp.Web/Data/FileFilter/KeywordFilterRuleRepository.cs`

**内容**:
- 继承自 `SqlSugarRepository<KeywordFilterRule, long>`
- 实现了 `IKeywordFilterRuleRepository` 接口
- 使用 SqlSugar 的 LINQ 查询替代 EF Core 的查询
- 保留了所有业务方法：
  - `GetAllEnabledRulesAsync()` - 使用 `GetQueryable().Where().OrderBy().ToListAsync()`
  - `GetEnabledRulesByTypeAsync()` - 使用 `GetQueryable().Where().OrderBy().ToListAsync()`
  - `ShouldFilterFileAsync()` - 保留原有业务逻辑
  - `ShouldFilterFilesAsync()` - 保留原有业务逻辑
  - `IsMatch()` - 私有方法，保留原有匹配逻辑

## 修改的文件

### 1. 实体文件
**文件路径**: `src/DFApp.Web/Domain/FileFilter/KeywordFilterRule.cs`

**修改内容**:
- 将 `Keyword` 属性从 `required string` 改为 `string`，并提供默认值 `string.Empty`
- 原因：满足 `ISqlSugarRepository<T, TKey>` 的 `new()`约束

### 2. 依赖注入配置
**文件路径**: `src/DFApp.Web/Program.cs`

**修改内容**:
- 添加了自定义仓储的注册：
  ```csharp
  builder.Services.AddScoped<DFApp.FileFilter.IKeywordFilterRuleRepository, DFApp.FileFilter.KeywordFilterRuleRepository>();
  ```

## 业务逻辑保留情况

### 保留的方法
1. **GetAllEnabledRulesAsync()**
   - 功能：获取所有启用的规则，按优先级和 ID 排序
   - 实现：使用 SqlSugar 的 `GetQueryable().Where().OrderBy().ToListAsync()`
   - 业务逻辑：完全保留

2. **GetEnabledRulesByTypeAsync(FilterType filterType)**
   - 功能：获取指定类型的启用规则，按优先级和 ID 排序
   - 实现：使用 SqlSugar 的 `GetQueryable().Where().OrderBy().ToListAsync()`
   - 业务逻辑：完全保留

3. **ShouldFilterFileAsync(string fileName)**
   - 功能：判断单个文件是否应该被过滤
   - 实现：保留原有业务逻辑
   - 业务逻辑：完全保留
     - 支持黑名单和白名单模式
     - 按优先级排序处理规则
     - 如果有白名单规则但没有匹配，则过滤掉

4. **ShouldFilterFilesAsync(IEnumerable<string> fileNames)**
   - 功能：批量判断文件是否应该被过滤
   - 实现：保留原有业务逻辑
   - 业务逻辑：完全保留
     - 批量处理多个文件名
     - 优化性能，只查询一次规则

5. **IsMatch(string fileName, KeywordFilterRule rule)**
   - 功能：判断文件名是否匹配规则
   - 实现：保留原有匹配逻辑
   - 业务逻辑：完全保留
     - 支持 5 种匹配模式（Contains、StartsWith、EndsWith、Exact、Regex）
     - 支持大小写敏感/不敏感
     - 正则表达式异常处理

## 迁移过程中的技术细节

### 1. SqlSugar 查询语法
**EF Core 原代码**:
```csharp
var dbSet = await GetDbSetAsync();
return dbSet
    .Where(x => x.IsEnabled)
    .OrderBy(x => x.Priority)
    .ThenBy(x => x.Id)
    .ToList();
```

**SqlSugar 新代码**:
```csharp
return await GetQueryable()
    .Where(x => x.IsEnabled)
    .OrderBy(x => x.Priority)
    .OrderBy(x => x.Id, OrderByType.Asc)
    .ToListAsync();
```

### 2. 排序处理
- EF Core 使用 `ThenBy()` 进行多字段排序
- SqlSugar 使用多个 `OrderBy()` 调用，并指定 `OrderByType.Asc`

### 3. 异步处理
- EF Core 的 `ToListAsync()` 是同步方法（在原代码中）
- SqlSugar 的 `ToListAsync()` 是异步方法

## 遇到的问题和解决方案

### 问题 1：required 成员导致编译错误
**问题描述**:
```
'KeywordFilterRule' cannot satisfy the 'new()' constraint on parameter 'T' in the generic type or method 'ISqlSugarRepository<T, TKey>' because 'KeywordFilterRule' has required members.
```

**解决方案**:
- 将 `Keyword` 属性从 `required string` 改为 `string`，并提供默认值 `string.Empty`
- 原因：`ISqlSugarRepository<T, TKey>` 要求 `T` 必须有无参构造函数（`new()`约束）

### 问题 2：接口命名冲突
**问题描述**:
- 原来的接口在 `src/DFApp.Domain/FileFilter/IKeywordFilterRuleRepository.cs`
- 新的接口在 `src/DFApp.Web/Data/FileFilter/IKeywordFilterRuleRepository.cs`
- 两个接口都在 `DFApp.FileFilter` 命名空间下

**解决方案**:
- 保留两个接口，让它们共存
- 新的接口继承自 `ISqlSugarRepository<KeywordFilterRule, long>`
- 原来的接口继承自 `IRepository<KeywordFilterRule, long>`（ABP）
- 在 `Program.cs` 中注册新的实现类
- 旧的 `EfCoreKeywordFilterRuleRepository` 仍然存在，但不再使用

## 未完成的任务

### 1. 服务层迁移
以下服务仍然使用原来的 `IKeywordFilterRuleRepository` 接口（ABP 版本）：
- `src/DFApp.Application/Aria2/Aria2Service.cs`
- `src/DFApp.Application/FileFilter/KeywordFilterRuleService.cs`

这些服务需要在后续阶段迁移到新的架构。

### 2. 编译错误
由于服务层仍然使用旧的接口，会出现编译错误。这是预期中的，按照任务要求："迁移过程中会出现无法编译的情况，不要为了解决而解决"。

## 测试建议

### 1. 单元测试
需要为以下方法编写单元测试：
- `GetAllEnabledRulesAsync()`
- `GetEnabledRulesByTypeAsync(FilterType filterType)`
- `ShouldFilterFileAsync(string fileName)`
- `ShouldFilterFilesAsync(IEnumerable<string> fileNames)`
- `IsMatch(string fileName, KeywordFilterRule rule)`

### 2. 集成测试
需要测试以下场景：
- 黑名单模式：匹配到的文件被过滤
- 白名单模式：只有匹配到的文件被保留
- 多种匹配模式（Contains、StartsWith、EndsWith、Exact、Regex）
- 大小写敏感/不敏感
- 优先级排序
- 正则表达式异常处理

## 总结

本次迁移成功完成了以下目标：
1. ✅ 创建了新的 SqlSugar 版本的仓储接口和实现类
2. ✅ 保留了所有业务方法和逻辑
3. ✅ 使用 SqlSugar 的查询替代了 EF Core 的查询
4. ✅ 移除了导航查询（原仓储本身就没有导航查询）
5. ✅ 决定创建自定义仓储（而不是使用通用仓储）
6. ✅ 注册了自定义仓储到依赖注入容器

迁移过程中遇到的问题都已解决，业务逻辑完全保留。服务层的迁移将在后续阶段进行。
