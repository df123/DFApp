# Phase 3.2 子任务 6：FilesItemRepository 迁移

## 概述
将 `FilesItemRepository` 从 EF Core 迁移到 SqlSugar，保留业务逻辑，移除导航查询。

## 原始仓储分析

### 原始文件位置
- `src/DFApp.EntityFrameworkCore/Aria2/Response/TellStatus/FilesItemRepository.cs`
- `src/DFApp.Domain/Aria2/Repository/Response/TellStatus/IFilesItemRepository.cs`

### 原始仓储结构
```csharp
public class FilesItemRepository : EfCoreRepository<DFAppDbContext, FilesItem>, IFilesItemRepository
{
    public FilesItemRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }
}
```

### 接口定义
```csharp
public interface IFilesItemRepository:IRepository<FilesItem>
{
    // 没有定义任何额外的业务方法
}
```

## 迁移决策

### 决策结果
**不创建自定义仓储，直接使用通用仓储替代**

### 决策理由
1. **仓储非常简单**：`FilesItemRepository` 没有任何自定义业务方法，只是继承自 `EfCoreRepository`
2. **接口无额外业务方法**：`IFilesItemRepository` 接口没有定义任何额外的业务方法
3. **未被使用**：搜索结果显示，没有任何服务或类使用 `IFilesItemRepository` 或 `FilesItemRepository`
4. **通用仓储足够**：可以直接使用 `ISqlSugarRepository<FilesItem, int>` 或 `ISqlSugarReadOnlyRepository<FilesItem, int>` 替代
5. **遵循原则**：符合"简单的 Repository 应使用通用仓储替代"的原则

### 依赖分析
`FilesItemRepository` 没有被任何服务使用。它可能只是为了保持代码结构的完整性而存在。

## 实体迁移

### FilesItem 实体
**文件位置**：`src/DFApp.Web/Domain/Aria2/Response/TellStatus/FilesItem.cs`

**迁移状态**：已在子任务5 中完成迁移

**主要变更**：
1. 继承基类从 `CreationAuditedAggregateRoot<int>` 改为 `CreationAuditedEntity<int>`
2. 添加 `[SugarTable("FilesItems")]` 特性标记表名
3. 使用 `[SugarColumn(IsIgnore = true)]` 标记导航属性 `Uris` 和 `Result`
4. 保留外键属性 `ResultId`
5. 添加中文注释

```csharp
[SugarTable("FilesItems")]
public class FilesItem : CreationAuditedEntity<int>
{
    /// <summary>
    /// 已完成长度
    /// </summary>
    public long? CompletedLength { get; set; }

    /// <summary>
    /// 索引
    /// </summary>
    public long? Index { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    public long? Length { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool? Selected { get; set; }

    /// <summary>
    /// URI列表（导航属性，不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<UrisItem>? Uris { get; set; }

    /// <summary>
    /// TellStatus结果（导航属性，不映射到数据库）
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public TellStatusResult Result { get; set; } = null!;

    /// <summary>
    /// TellStatus结果ID
    /// </summary>
    public long ResultId { get; set; }
}
```

## 仓储迁移

### 迁移方式
不创建自定义仓储，直接使用通用仓储 `ISqlSugarRepository<FilesItem, int>` 或 `ISqlSugarReadOnlyRepository<FilesItem, int>`。

### 使用示例
```csharp
// 在需要使用 FilesItem 的地方，直接使用通用仓储
public class SomeService
{
    private readonly ISqlSugarRepository<FilesItem, int> _filesItemRepository;

    public SomeService(ISqlSugarRepository<FilesItem, int> filesItemRepository)
    {
        _filesItemRepository = filesItemRepository;
    }

    // 使用通用仓储的方法
    public async Task<FilesItem> GetByIdAsync(int id)
    {
        return await _filesItemRepository.GetByIdAsync(id);
    }

    public async Task<List<FilesItem>> GetListAsync()
    {
        return await _filesItemRepository.GetListAsync();
    }

    public async Task<List<FilesItem>> GetByResultIdAsync(long resultId)
    {
        return await _filesItemRepository.GetListAsync(x => x.ResultId == resultId);
    }

    public async Task InsertAsync(FilesItem entity)
    {
        await _filesItemRepository.InsertAsync(entity);
    }

    public async Task UpdateAsync(FilesItem entity)
    {
        await _filesItemRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _filesItemRepository.DeleteAsync(id);
    }
}
```

### 只读操作示例
如果只需要查询操作，可以使用 `ISqlSugarReadOnlyRepository<FilesItem, int>`：

```csharp
public class SomeQueryService
{
    private readonly ISqlSugarReadOnlyRepository<FilesItem, int> _filesItemRepository;

    public SomeQueryService(ISqlSugarReadOnlyRepository<FilesItem, int> filesItemRepository)
    {
        _filesItemRepository = filesItemRepository;
    }

    public async Task<List<FilesItem>> GetByResultIdAsync(long resultId)
    {
        return await _filesItemRepository.GetListAsync(x => x.ResultId == resultId);
    }

    public async Task<FilesItem?> GetByIdAsync(int id)
    {
        return await _filesItemRepository.GetByIdAsync(id);
    }
}
```

## 导航查询处理

### 原始导航查询
原始代码中，FilesItem 有两个导航属性：
1. `Uris` - URI列表
2. `Result` - TellStatus结果

### 迁移后处理
由于不再使用导航查询，需要通过以下方式访问关联数据：

#### 访问 UrisItem
```csharp
// 查询 FilesItem
var filesItem = await _filesItemRepository.GetByIdAsync(id);

// 通过外键查询 UrisItem
var urisItemRepository = _serviceProvider.GetRequiredService<ISqlSugarRepository<UrisItem, short>>();
var uris = await urisItemRepository.GetListAsync(x => x.FilesItemId == filesItem.Id);
```

#### 访问 TellStatusResult
```csharp
// 查询 FilesItem
var filesItem = await _filesItemRepository.GetByIdAsync(id);

// 通过外键查询 TellStatusResult
var tellStatusResultRepository = _serviceProvider.GetRequiredService<ISqlSugarRepository<TellStatusResult, long>>();
var result = await tellStatusResultRepository.GetByIdAsync(filesItem.ResultId);
```

#### 使用 JOIN 查询
如果需要一次性查询关联数据，可以使用 JOIN：

```csharp
// FilesItem 和 TellStatusResult JOIN
var query = _filesItemRepository.AsQueryable()
    .LeftJoin<TellStatusResult>((f, r) => f.ResultId == r.Id)
    .Where((f, r) => f.Id == id)
    .Select((f, r) => new { FilesItem = f, TellStatusResult = r });

var result = await query.FirstAsync();

// FilesItem 和 UrisItem JOIN
var query2 = _filesItemRepository.AsQueryable()
    .LeftJoin<UrisItem>((f, u) => f.Id == u.FilesItemId)
    .Where((f, u) => f.Id == id)
    .Select((f, u) => new { FilesItem = f, UrisItem = u });

var result2 = await query2.ToListAsync();
```

**注意**：具体的迁移方案将在后续阶段（Aria2Service 迁移）中实现。

## 影响范围

### 需要删除的文件
1. `src/DFApp.EntityFrameworkCore/Aria2/Response/TellStatus/FilesItemRepository.cs`
2. `src/DFApp.Domain/Aria2/Repository/Response/TellStatus/IFilesItemRepository.cs`

### 需要修改的文件
无（该仓储未被使用）

## 已完成的工作

1. ✅ 分析 `FilesItemRepository` 的业务方法和依赖
2. ✅ 确认 `FilesItem` 实体已在子任务5 中迁移完成
3. ✅ 评估是否需要创建自定义仓储
4. ✅ 决定使用通用仓储替代自定义仓储
5. ✅ 创建迁移文档

## 待完成的工作

1. ⏳ 删除旧的 EF Core 仓储文件（在后续阶段统一删除）
2. ⏳ 如果后续有服务需要使用 FilesItem，将使用通用仓储

## 注意事项

1. **导航查询移除**：所有导航属性已用 `[SugarColumn(IsIgnore = true)]` 标记，不再映射到数据库
2. **外键保留**：外键属性 `ResultId` 已保留
3. **业务逻辑保持**：原始的业务逻辑将在后续阶段迁移时保持不变
4. **未被使用**：该仓储目前未被任何服务使用，如果后续需要使用，将直接使用通用仓储
5. **编译错误**：迁移过程中不会出现编译错误，因为该仓储未被使用

## 总结

本次迁移成功完成了以下工作：
1. 分析了 `FilesItemRepository` 的结构和依赖
2. 确认 `FilesItem` 实体已在子任务5 中迁移完成
3. 决定不创建自定义仓储，直接使用通用仓储 `ISqlSugarRepository<FilesItem, int>` 或 `ISqlSugarReadOnlyRepository<FilesItem, int>` 替代
4. 提供了通用仓储的使用示例和导航查询的处理方案

迁移遵循了"简单的 Repository 应使用通用仓储替代"的原则，避免了不必要的自定义仓储创建。由于该仓储未被任何服务使用，不需要创建新的仓储文件，直接使用通用仓储即可。

## Phase 3.2 总结

至此，Phase 3.2 的所有 6 个子任务已全部完成：

1. ✅ **EfCoreKeywordFilterRuleRepository** - 创建了自定义仓储 `KeywordFilterRuleRepository`
2. ✅ **EfCoreGasolinePriceRepository** - 创建了自定义仓储 `GasolinePriceRepository`
3. ✅ **EfCoreBookkeepingExpenditureRepository** - 使用通用仓储替代
4. ✅ **EfCoreConfigurationInfoRepository** - 创建了自定义仓储 `ConfigurationInfoRepository`
5. ✅ **TellStatusResultRepository** - 使用通用仓储替代
6. ✅ **FilesItemRepository** - 使用通用仓储替代

### 迁移统计

- **创建自定义仓储**：3 个（KeywordFilterRuleRepository、GasolinePriceRepository、ConfigurationInfoRepository）
- **使用通用仓储**：3 个（BookkeepingExpenditureRepository、TellStatusResultRepository、FilesItemRepository）
- **迁移的实体**：6 个（KeywordFilterRule、GasolinePrice、BookkeepingExpenditure、ConfigurationInfo、TellStatusResult、FilesItem）

### 迁移原则遵循情况

1. ✅ 简单的 Repository 使用通用仓储替代
2. ✅ 有复杂业务逻辑的 Repository 创建自定义仓储
3. ✅ 不再使用导航查询
4. ✅ 所有代码注释使用中文
5. ✅ 所有新增代码放在 `src/DFApp.Web` 项目中

Phase 3.2 迁移工作圆满完成！
