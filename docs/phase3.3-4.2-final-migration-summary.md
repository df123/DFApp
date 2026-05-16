# Phase 3.3 + Phase 4.2 最终迁移总结

**完成时间**：2026-03-30 | **状态**：已完成 | **迁移服务总数**：17

---

## 1. 概述

### 1.1 迁移目标

- **Phase 3.3**：替换所有服务中的仓储注入，将 ABP 的 `IRepository<T, TKey>` 替换为 SqlSugar 的 `ISqlSugarRepository<T, TKey>`
- **Phase 4.2**：迁移所有继承自 `CrudAppService` 的服务到新的 `CrudServiceBase` 基类

### 1.2 总体状态

Phase 3.3 和 Phase 4.2 的迁移工作已全部完成。共迁移 **17 个服务**，覆盖了配置管理、电动汽车、记账、文件过滤、IP 管理、彩票、媒体、文件上传下载、Aria2 下载等所有业务模块。

---

## 2. 迁移范围统计

### 2.1 总体统计

| 指标 | 数量 |
|------|------|
| 迁移服务总数 | 17 |
| CrudAppService → CrudServiceBase | 15 |
| ApplicationService → AppServiceBase | 1 |
| 涉及实体数 | 20+ |
| 导航查询替代数 | 8+ |
| 自定义方法迁移数 | 50+ |

### 2.2 按模块分类统计

| 模块 | 迁移服务数 | 服务列表 |
|------|-----------|---------|
| 配置管理 | 1 | ConfigurationInfoService |
| 电动汽车 | 5 | ElectricVehicleService, ElectricVehicleChargingRecordService, ElectricVehicleCostService, GasolinePriceService, (GasolinePriceRefresher 未迁移) |
| 记账 | 2 | BookkeepingCategoryService, BookkeepingExpenditureService |
| 文件过滤 | 1 | KeywordFilterRuleService |
| IP 管理 | 1 | DynamicIPService |
| 彩票 | 4 | LotteryResultService, LotteryService, LotteryKL8SimulationService, LotterySSQSimulationService |
| 媒体 | 2 | MediaInfoService, ExternalLinkService |
| 文件上传下载 | 1 | FileUploadInfoService |
| Aria2 下载 | 1 | Aria2Service |

### 2.3 按批次分类统计

| 批次 | 服务数 | 复杂度 | 说明 |
|------|--------|--------|------|
| Phase 3.3 + 4.1 | 4 | 混合 | 首批迁移，建立迁移模式 |
| Batch 1 | 4 | 简单 | 纯 CRUD 服务，无自定义方法 |
| Batch 2 | 3 | 中等 | 含自定义查询、多表关联 |
| Batch 3 | 3 | 复杂 | 含大量业务逻辑、后台任务、多仓储 |
| Batch 4 | 3 | 复杂 | 彩票模块，含复杂计算和事务管理 |

---

## 3. 完整迁移服务列表

### 3.1 Phase 3.3 + 4.1（首批迁移）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| ConfigurationInfoService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Configuration/ConfigurationInfoService.cs` |
| GasolinePriceService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/ElectricVehicle/GasolinePriceService.cs` |
| BookkeepingCategoryService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Bookkeeping/BookkeepingCategoryService.cs` |
| KeywordFilterRuleService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/FileFilter/KeywordFilterRuleService.cs` |

### 3.2 Batch 1（简单服务）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| DynamicIPService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/IP/DynamicIPService.cs` |
| ElectricVehicleService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleService.cs` |
| LotteryResultService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Lottery/LotteryResultService.cs` |
| MediaInfoService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Media/MediaInfoService.cs` |

### 3.3 Batch 2（中等服务）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| FileUploadInfoService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/FileUploadDownload/FileUploadInfoService.cs` |
| ElectricVehicleChargingRecordService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleChargingRecordService.cs` |
| BookkeepingExpenditureService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Bookkeeping/BookkeepingExpenditureService.cs` |

### 3.4 Batch 3（复杂服务）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| ElectricVehicleCostService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleCostService.cs` |
| ExternalLinkService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Media/ExternalLinkService.cs` |
| Aria2Service | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Aria2/Aria2Service.cs` |

### 3.5 Batch 4（彩票服务）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| LotteryKL8SimulationService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Lottery/Simulation/LotteryKL8SimulationService.cs` |
| LotterySSQSimulationService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Lottery/Simulation/LotterySSQSimulationService.cs` |
| LotteryService | CrudAppService | CrudServiceBase | `src/DFApp.Web/Services/Lottery/LotteryService.cs` |

---

## 4. 通用迁移模式

所有 17 个服务均遵循以下迁移变更模式：

### 4.1 基类替换

| 原基类 | 新基类 | 适用场景 |
|--------|--------|---------|
| `CrudAppService<TEntity, TGetOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>` | `CrudServiceBase<TEntity, TKey, TGetOutputDto, TCreateInput, TUpdateInput>` | 标准 CRUD 服务 |
| `ApplicationService` | `AppServiceBase` | 非 CRUD 的应用服务 |

### 4.2 仓储替换

| 原仓储 | 新仓储 | 说明 |
|--------|--------|------|
| `IRepository<TEntity, TKey>` | `ISqlSugarRepository<TEntity, TKey>` | 通用仓储替换 |
| `IReadOnlyRepository<TEntity, TKey>` | `ISqlSugarRepository<TEntity, TKey>` | 只读仓储统一使用通用仓储 |
| `ITellStatusResultRepository` | `ISqlSugarRepository<TellStatusResult, long>` | 自定义仓储替换为通用仓储 |
| `IGasolinePriceRepository`（部分场景） | `ISqlSugarRepository<GasolinePrice, Guid>` | 含业务方法的自定义仓储保留 |

### 4.3 查询方式替换

| 原方式 | 新方式 | 说明 |
|--------|--------|------|
| `AsyncExecuter.ToListAsync()` | `.ToListAsync()` | SqlSugar 原生异步 |
| `AsyncExecuter.CountAsync()` | `.CountAsync()` | SqlSugar 原生异步 |
| `AsyncExecuter.SumAsync()` | `.Sum()` / `query.Sum()` | SqlSugar 原生聚合 |
| `AsyncExecuter.MaxAsync()` | `GetQueryable() + .ToList() + LINQ .Max()` | 内存聚合 |
| `GetQueryableAsync()` | `GetQueryable()` | 同步获取查询对象 |
| `Repository.GetAsync(id)` | `Repository.GetByIdAsync(id)` | 按主键获取 |
| `ReadOnlyRepository.FirstAsync()` | `Repository.GetFirstOrDefaultAsync()` | 获取第一条 |
| `ReadOnlyRepository.AnyAsync()` | `Repository.GetQueryable().Any()` | 判断是否存在 |
| `ReadOnlyRepository.WithDetailsAsync()` | `Repository.GetQueryable()` + 外键查询 | 导航属性替代 |
| `Repository.WithDetailsAsync()` | `Repository.GetQueryable()` + 外键查询 | 导航属性替代 |

### 4.4 异常替换

| 原异常 | 新异常 |
|--------|--------|
| `UserFriendlyException` | `BusinessException` |
| `Check.NotNullOrWhiteSpace()` | `BusinessException` |

### 4.5 软删除移除

| 原方式 | 新方式 |
|--------|--------|
| `IDataFilter.Disable<ISoftDelete>()` | 移除 |
| `IsDeleted` 属性检查 | 移除 |
| `IsDeleted = false` 恢复逻辑 | 移除，已存在记录直接抛出异常 |
| `ReadOnlyRepository.GetListAsync(true)` | `Repository.GetListAsync()` |

### 4.6 导航查询替代

| 原方式 | 新方式 |
|--------|--------|
| `.Include(x => x.Vehicle)` | 通过 `_vehicleRepository.GetByIdAsync()` 外键查询 |
| `.Include(x => x.Category)` | 通过 `_categoryRepository` 批量查询，构建 `categoryNameMap` |
| `x.Vehicle.Name` 导航属性访问 | 先获取 VehicleId 列表，再批量查询 |
| `x.Category.Category` 导航属性访问 | 先获取匹配分类 ID 列表，再用 `matchingCategoryIds.Contains()` |
| `data.Files` 导航属性访问 | 通过 `_filesItemRepository.GetListAsync(f => resultIds.Contains(f.ResultId))` |
| `result.Prizegrades` 导航属性访问 | 直接查询 `LotteryPrizegrades` 仓储 |

### 4.7 工作单元移除

| 原方式 | 新方式 |
|--------|--------|
| `IUnitOfWorkManager.Begin()` | 移除（SqlSugar 自带事务管理） |
| `IUnitOfWorkManager.Begin(requiresNew: true)` | 移除 |
| `IUnitOfWorkManager.Current.SaveChangesAsync()` | 移除 |
| `IUnitOfWorkManager` 事务 | `Repository.BeginTran()` / `CommitTran()` / `RollbackTran()` |

### 4.8 映射替换

| 原方式 | 新方式 |
|--------|--------|
| `ObjectMapper.Map<TEntity, TDto>(entity)` | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| ABP 自动 CRUD 映射 | `MapToGetOutputDto`、`MapToEntity`、`MapToEntity` 重载三个方法 |

### 4.9 权限移除

| 原方式 | 新方式 |
|--------|--------|
| `[Authorize(DFAppPermissions.XXX.Default)]` | 移除（待后续添加） |
| `GetPolicyName = "XXX"` | 移除 |
| `GetListPolicyName = "XXX"` | 移除 |
| `CreatePolicyName = "XXX"` | 移除 |
| `UpdatePolicyName = "XXX"` | 移除 |
| `DeletePolicyName = "XXX"` | 移除 |

### 4.10 构造函数变更

所有服务的构造函数统一新增以下参数：

```csharp
ICurrentUser currentUser,      // 当前用户信息
IPermissionChecker permissionChecker  // 权限检查器
```

同时将 `IRepository` 参数替换为 `ISqlSugarRepository`。

### 4.11 日志替换

| 原方式 | 新方式 |
|--------|--------|
| ABP `Logger` 属性（`Logger.LogWarning`） | 注入 `ILogger<T>`（`_logger.LogWarning`） |

---

## 5. 已知编译问题

以下编译问题是预期的，将在后续阶段统一解决，**不需要在当前阶段修复**：

### 5.1 `required` 成员约束问题

部分实体定义中使用了 `required` 关键字，导致 `new()` 泛型约束无法满足：

| 实体 | `required` 属性 | 影响的服务 |
|------|----------------|-----------|
| `DynamicIP` | IP, Port | DynamicIPService |
| `MediaInfo` | ChatTitle, SavePath, MimeType | MediaInfoService |
| `MediaExternalLink` | （有 required 属性） | ExternalLinkService |
| `LotterySimulation` | BallType, GameType | LotteryKL8SimulationService, LotterySSQSimulationService |

**解决方案**：后续修改实体定义，移除 `required` 关键字或提供默认值。

### 5.2 `IEntity<TKey>` 接口不匹配

实体继承 `AuditedEntity<TKey>` 而非直接实现 `IEntity<TKey>` 接口，导致泛型约束不匹配。

**解决方案**：后续统一修改实体基类。

### 5.3 命名空间歧义

部分服务存在命名空间与类名冲突：

| 冲突 | 解决方式 |
|------|---------|
| `ElectricVehicle` 既是命名空间又是类名 | `using ElectricVehicleEntity = DFApp.ElectricVehicle.ElectricVehicle` |
| `IConfigurationInfoRepository` 命名空间歧义 | `using IConfigurationInfoRepository = DFApp.Web.Data.Configuration.IConfigurationInfoRepository` |

### 5.4 DTO 类引用 ABP 基类

部分 DTO 类仍在 `src/DFApp.Application.Contracts/` 中，引用了 ABP 的 `AuditedEntityDto<long>` 等基类。

**解决方案**：后续迁移 DTO 类到新架构。

### 5.5 ISugarQueryable 与 LINQ 扩展方法冲突

`ISugarQueryable<T>` 的 `OrderByDescending`、`ThenByDescending`、`FirstOrDefault` 等方法与 `System.Linq.ParallelEnumerable` 扩展方法存在冲突。

**解决方案**：在链式调用中添加 `.ToList()` 将结果转换为 `List<T>` 后再使用 LINQ 方法。

---

## 6. 未迁移的依赖

### 6.1 内部依赖

| 依赖 | 类型 | 状态 | 说明 |
|------|------|------|------|
| Mapperly 映射器 | 映射 | ❌ 未迁移 | 所有映射使用 `// TODO: 使用 Mapperly 映射` 伪代码 |
| 权限特性 | 授权 | ❌ 未迁移 | 所有 `[Authorize]` 已移除，待后续添加 |
| Controller 层 | API | ❌ 未迁移 | 尚未创建对应的 API Controller |
| DTO 类 | 数据传输 | ❌ 未迁移 | 仍在 `src/DFApp.Application.Contracts/` 中 |

### 6.2 外部依赖（服务级别）

| 依赖 | 所属服务 | 状态 | 说明 |
|------|---------|------|------|
| `GasolinePriceRefresher` | GasolinePriceService | ❌ 未迁移 | 油价刷新器，仍使用 ABP 仓储 |
| `Aria2RpcClient` | Aria2Service | ❌ 未迁移 | Aria2 RPC 客户端 |
| `IBackgroundTaskQueue` | ExternalLinkService, Aria2Service | ❌ 未迁移 | 后台任务队列接口 |
| `IQueueManagement` | Aria2Service | ❌ 未迁移 | 队列管理接口 |
| `IKeywordFilterRuleRepository` | Aria2Service | ❌ 未迁移 | 继承自 ABP IRepository 的自定义仓储 |
| `LotteryDataFetchService` | LotteryService | ❌ 未迁移 | 彩票数据抓取服务 |
| `CompoundLotteryService` | LotteryService | ❌ 未迁移 | 组合彩票服务 |

### 6.3 共享工具类

| 依赖 | 位置 | 状态 |
|------|------|------|
| `Aria2Consts` | `DFApp.Domain.Shared` | ✅ 可用 |
| `LotteryBallType` / `LotteryGameType` / `LotteryKL8PlayType` | `DFApp.Domain.Shared` | ✅ 可用 |
| `SpaceHelper` | `DFApp.Domain.Shared` | ✅ 可用 |
| `BencodeNET` | NuGet 包 | ✅ 可用 |

---

## 7. 下一步工作

### 7.1 Phase 4.3：迁移 ApplicationService（非 CrudAppService 的服务）

迁移不继承 `CrudAppService` 但继承 `ApplicationService` 的服务：

- `AccountAppService` - 账户服务
- `UserManagementAppService` - 用户管理服务
- `Aria2ManageService` - Aria2 管理服务
- `LotteryDataFetchService` - 彩票数据抓取服务
- `CompoundLotteryService` - 组合彩票服务
- `RssFetchService` - RSS 抓取服务
- `RssSubscriptionAppService` - RSS 订阅服务
- `RssMirrorItemAppService` - RSS 镜像项服务
- `RssSourceAppService` - RSS 源服务
- `RssSubscriptionDownloadAppService` - RSS 订阅下载服务
- `RssSubscriptionService` - RSS 订阅服务
- `RssWordSegmentAppService` - RSS 分词服务
- `WordSegmentService` - 分词服务
- `TGLoginService` - Telegram 登录服务

### 7.2 Phase 4.4：迁移 DTO 映射（Mapperly）

- 为每个服务创建对应的 Mapperly 映射器类
- 替换所有 `// TODO: 使用 Mapperly 映射` 伪代码
- 使用 `[Mapper]` 特性标记映射器类
- 实现实体到 DTO 和 DTO 到实体的映射方法

### 7.3 Phase 5：创建 Controller 层

为每个服务创建对应的 API Controller：

- 路由采用 `/api/app/{kebab-case-entity}` 模式
- 添加权限特性
- 添加参数验证
- 添加 Swagger 文档注释

### 7.4 Phase 6：添加权限控制

- 为每个服务的公共方法添加权限特性
- 定义相应的权限名称
- 确保权限检查逻辑正确实现

---

## 8. 文件结构

迁移后的服务文件结构如下：

```
src/DFApp.Web/Services/
├── Aria2/
│   └── Aria2Service.cs
├── Bookkeeping/
│   ├── BookkeepingCategoryService.cs
│   └── BookkeepingExpenditureService.cs
├── Configuration/
│   └── ConfigurationInfoService.cs
├── ElectricVehicle/
│   ├── ElectricVehicleService.cs
│   ├── ElectricVehicleChargingRecordService.cs
│   ├── ElectricVehicleCostService.cs
│   └── GasolinePriceService.cs
├── FileFilter/
│   └── KeywordFilterRuleService.cs
├── FileUploadDownload/
│   └── FileUploadInfoService.cs
├── IP/
│   └── DynamicIPService.cs
├── Lottery/
│   ├── LotteryResultService.cs
│   ├── LotteryService.cs
│   └── Simulation/
│       ├── LotteryKL8SimulationService.cs
│       └── LotterySSQSimulationService.cs
└── Media/
    ├── MediaInfoService.cs
    └── ExternalLinkService.cs
```

---

## 9. 相关文档

| 文档 | 说明 |
|------|------|
| [Phase 3.3 + 4.1 迁移总结](phase3.3-4.1-migration-summary.md) | 首批 4 个服务的迁移详情 |
| [Phase 4.2 Batch 1 迁移总结](phase4.2-batch1-migration-summary.md) | 4 个简单 CRUD 服务的迁移详情 |
| [Phase 4.2 Batch 2 迁移总结](phase4.2-batch2-migration-summary.md) | 3 个中等复杂度服务的迁移详情 |
| [Phase 4.2 Batch 3 迁移总结](phase4.2-batch3-migration-summary.md) | 3 个复杂服务的迁移详情 |
| [Phase 4.2 Batch 4 迁移总结](phase4.2-batch4-migration-summary.md) | 3 个彩票模块服务的迁移详情 |
| [框架迁移计划](framework-migration-plan.md) | 整体迁移计划 |
| [执行进度](执行进度.md) | 迁移执行进度跟踪 |
