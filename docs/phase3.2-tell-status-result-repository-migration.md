# Phase 3.2 子任务 5：TellStatusResultRepository 迁移

## 概述
将 `TellStatusResultRepository` 从 EF Core 迁移到 SqlSugar，移除导航查询，使用通用仓储替代。

## 原始仓储分析

### 原始文件位置
- `src/DFApp.EntityFrameworkCore/Aria2/Response/TellStatus/TellStatusResultRepository.cs`

### 原始仓储结构
```csharp
public class TellStatusResultRepository : EfCoreRepository<DFAppDbContext, TellStatusResult, long>, ITellStatusResultRepository
{
    public TellStatusResultRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<TellStatusResult>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeSub();
    }
}
```

### 接口定义
```csharp
public interface ITellStatusResultRepository : IRepository<TellStatusResult, long>
{
    // 没有定义任何额外的业务方法
}
```

### 导航查询扩展
```csharp
public static class TellStatusEfCoreQueryableExtensions
{
    public static IQueryable<TellStatusResult> IncludeSub(this IQueryable<TellStatusResult> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable.Include(x => x.Files!).ThenInclude(x => x.Uris);
    }
}
```

## 迁移决策

### 决策结果
**不创建自定义仓储，直接使用通用仓储替代**

### 决策理由
1. **仓储非常简单**：`TellStatusResultRepository` 只有一个 `WithDetailsAsync` 方法用于加载导航属性
2. **不再使用导航查询**：根据迁移要求，不再使用导航查询，所以 `WithDetailsAsync` 方法不再需要
3. **接口无额外业务方法**：`ITellStatusResultRepository` 接口没有定义任何额外的业务方法
4. **通用仓储足够**：可以直接使用 `ISqlSugarRepository<TellStatusResult, long>` 替代

### 依赖分析
`TellStatusResultRepository` 被以下类使用：
- `Aria2Manager`：使用 `_resultRepository.InsertAsync(result)` 进行插入操作
- `Aria2Service`：使用导航查询访问 `Files` 属性

**注意**：`Aria2Service` 中使用了导航查询，这些代码将在后续阶段迁移时处理。

## 实体迁移

### TellStatusResult 实体
**新文件位置**：`src/DFApp.Web/Domain/Aria2/Response/TellStatus/TellStatusResult.cs`

**主要变更**：
1. 继承基类从 `CreationAuditedAggregateRoot<long>` 改为 `CreationAuditedEntity<long>`
2. 添加 `[SugarTable("TellStatusResults")]` 特性标记表名
3. 使用 `[SugarColumn(IsIgnore = true)]` 标记导航属性 `Files`
4. 添加中文注释

```csharp
[SugarTable("TellStatusResults")]
public class TellStatusResult : CreationAuditedEntity<long>
{
    public string? Bitfield { get; set; }
    public long? CompletedLength { get; set; }
    public long? Connections { get; set; }
    public string? Dir { get; set; }
    public long? DownloadSpeed { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    // 导航属性，不映射到数据库
    [SugarColumn(IsIgnore = true)]
    public List<FilesItem>? Files { get; set; }

    public string? GID { get; set; }
    public long? NumPieces { get; set; }
    public long? PieceLength { get; set; }
    public string? Status { get; set; }
    public long? TotalLength { get; set; }
    public long? UploadLength { get; set; }
    public long? UploadSpeed { get; set; }
}
```

### FilesItem 实体
**新文件位置**：`src/DFApp.Web/Domain/Aria2/Response/TellStatus/FilesItem.cs`

**主要变更**：
1. 继承基类从 `CreationAuditedAggregateRoot<int>` 改为 `CreationAuditedEntity<int>`
2. 添加 `[SugarTable("FilesItems")]` 特性标记表名
3. 使用 `[SugarColumn(IsIgnore = true)]` 标记导航属性 `Uris` 和 `Result`
4. 保留外键属性 `ResultId`

```csharp
[SugarTable("FilesItems")]
public class FilesItem : CreationAuditedEntity<int>
{
    public long? CompletedLength { get; set; }
    public long? Index { get; set; }
    public long? Length { get; set; }
    public string? Path { get; set; }
    public bool? Selected { get; set; }

    // 导航属性，不映射到数据库
    [SugarColumn(IsIgnore = true)]
    public List<UrisItem>? Uris { get; set; }

    // 导航属性，不映射到数据库
    [SugarColumn(IsIgnore = true)]
    public TellStatusResult Result { get; set; } = null!;

    public long ResultId { get; set; }
}
```

### UrisItem 实体
**新文件位置**：`src/DFApp.Web/Domain/Aria2/Response/TellStatus/UrisItem.cs`

**主要变更**：
1. 继承基类从 `CreationAuditedAggregateRoot<short>` 改为 `CreationAuditedEntity<short>`
2. 添加 `[SugarTable("UrisItems")]` 特性标记表名
3. 使用 `[SugarColumn(IsIgnore = true)]` 标记导航属性 `FilesItem`
4. 保留外键属性 `FilesItemId`

```csharp
[SugarTable("UrisItems")]
public class UrisItem : CreationAuditedEntity<short>
{
    public string? Status { get; set; }
    public string? Uri { get; set; }

    // 导航属性，不映射到数据库
    [SugarColumn(IsIgnore = true)]
    public FilesItem FilesItem { get; set; } = null!;

    public int FilesItemId { get; set; }
}
```

## 仓储迁移

### 迁移方式
不创建自定义仓储，直接使用通用仓储 `ISqlSugarRepository<TellStatusResult, long>`。

### 使用示例
```csharp
// 在需要使用 TellStatusResultRepository 的地方，改为使用通用仓储
public class SomeService
{
    private readonly ISqlSugarRepository<TellStatusResult, long> _tellStatusResultRepository;

    public SomeService(ISqlSugarRepository<TellStatusResult, long> tellStatusResultRepository)
    {
        _tellStatusResultRepository = tellStatusResultRepository;
    }

    // 使用通用仓储的方法
    public async Task<TellStatusResult> GetByIdAsync(long id)
    {
        return await _tellStatusResultRepository.GetByIdAsync(id);
    }

    public async Task<List<TellStatusResult>> GetListAsync()
    {
        return await _tellStatusResultRepository.GetListAsync();
    }

    public async Task InsertAsync(TellStatusResult entity)
    {
        await _tellStatusResultRepository.InsertAsync(entity);
    }
}
```

## 导航查询处理

### 原始导航查询
原始代码使用 `WithDetailsAsync()` 加载导航属性：
```csharp
var data = await _tellStatusResultRepository.GetListAsync(true);
// data.Files 将被自动加载
```

### 迁移后处理
由于不再使用导航查询，需要通过以下方式访问关联数据：

#### 方案 1：通过外键查询
```csharp
// 查询 TellStatusResult
var result = await _tellStatusResultRepository.GetByIdAsync(id);

// 通过外键查询 FilesItem
var files = await _filesItemRepository.GetListAsync(x => x.ResultId == result.Id);
```

#### 方案 2：使用 JOIN 查询
```csharp
var query = _tellStatusResultRepository.AsQueryable()
    .LeftJoin<FilesItem>((t, f) => t.Id == f.ResultId)
    .Where((t, f) => t.Id == id)
    .Select((t, f) => new { TellStatusResult = t, FilesItem = f });

var result = await query.ToListAsync();
```

**注意**：具体的迁移方案将在后续阶段（Aria2Service 迁移）中实现。

## 影响范围

### 需要修改的文件
1. `src/DFApp.Domain/Aria2/Aria2Manager.cs` - 依赖 `ITellStatusResultRepository`
2. `src/DFApp.Application/Aria2/Aria2Service.cs` - 依赖 `ITellStatusResultRepository` 并使用导航查询

### 需要删除的文件
1. `src/DFApp.EntityFrameworkCore/Aria2/Response/TellStatus/TellStatusResultRepository.cs`
2. `src/DFApp.EntityFrameworkCore/Aria2/Response/TellStatus/TellStatusEfCoreQueryableExtensions.cs`

### 需要废弃的接口
1. `src/DFApp.Domain/Aria2/Repository/Response/TellStatus/ITellStatusResultRepository.cs`

## 已完成的工作

1. ✅ 分析 `TellStatusResultRepository` 的业务方法和依赖
2. ✅ 迁移 `TellStatusResult` 实体到 `DFApp.Web` 项目
3. ✅ 迁移 `FilesItem` 实体到 `DFApp.Web` 项目
4. ✅ 迁移 `UrisItem` 实体到 `DFApp.Web` 项目
5. ✅ 评估是否需要创建自定义仓储
6. ✅ 决定使用通用仓储替代自定义仓储
7. ✅ 创建迁移文档

## 待完成的工作

1. ⏳ 修改 `Aria2Manager` 使用通用仓储
2. ⏳ 修改 `Aria2Service` 使用通用仓储并处理导航查询
3. ⏳ 删除旧的 EF Core 仓储文件
4. ⏳ 更新依赖注入配置
5. ⏳ 测试迁移后的功能

## 注意事项

1. **导航查询移除**：所有导航属性已用 `[SugarColumn(IsIgnore = true)]` 标记，不再映射到数据库
2. **外键保留**：所有外键属性（如 `ResultId`、`FilesItemId`）都已保留
3. **业务逻辑保持**：原始的业务逻辑将在后续阶段迁移时保持不变
4. **编译错误**：迁移过程中会出现编译错误，这是正常的，将在后续阶段解决
5. **依赖未迁移**：`Aria2Service` 和 `Aria2Manager` 的迁移将在后续阶段进行

## 总结

本次迁移成功完成了以下工作：
1. 将 `TellStatusResult`、`FilesItem` 和 `UrisItem` 实体从 ABP 框架迁移到 SqlSugar
2. 移除了所有导航查询，使用 `[SugarColumn(IsIgnore = true)]` 标记导航属性
3. 决定不创建自定义仓储，直接使用通用仓储 `ISqlSugarRepository<TellStatusResult, long>` 替代
4. 为后续的 `Aria2Service` 和 `Aria2Manager` 迁移做好了准备

迁移遵循了"简单的 Repository 应使用通用仓储替代"的原则，避免了不必要的自定义仓储创建。
