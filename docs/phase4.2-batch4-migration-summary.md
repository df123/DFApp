# Phase 4.2 Batch 4 - 彩票模块服务迁移总结

## 迁移概览

本批次迁移了 3 个彩票模块的 CrudAppService，包括整个项目中最大的服务文件 LotteryService。

## 迁移服务列表

### 1. LotteryKL8SimulationService（快乐8模拟服务）

- **原文件**: `src/DFApp.Application/Lottery/Simulation/LotteryKL8SimulationService.cs` (10060字符)
- **新文件**: `src/DFApp.Web/Services/Lottery/Simulation/LotteryKL8SimulationService.cs`
- **实体**: `LotterySimulation` (Guid 主键)
- **DTO**: `LotterySimulationDto`, `CreateUpdateLotterySimulationDto`, `GenerateRandomNumbersDto`, `WinningStatisticsDto`, `StatisticsDto`

#### 主要变更点

| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `CrudAppService<...>` | `CrudServiceBase<...>` |
| 仓储 | `IRepository<LotterySimulation, Guid>` | `ISqlSugarRepository<LotterySimulation, Guid>` |
| 依赖仓储 | `IRepository<LotteryResult, long>` | `ISqlSugarRepository<LotteryResult, long>` |
| 依赖仓储 | `IRepository<LotteryPrizegrades, long>` | `ISqlSugarRepository<LotteryPrizegrades, long>` |
| 权限 | `[Authorize]` 特性 + 策略名称 | 移除（由 Controller 层处理） |
| 查询 | `AsyncExecuter.MaxAsync()` | `GetQueryable()` + `.ToList()` + LINQ `.Max()` |
| 分组查询 | `AsyncExecuter.ToListAsync()` + EF Core GroupBy | 内存分组（`GetListAsync()` + LINQ `GroupBy`） |
| 映射 | 自动（ABP ObjectMapper） | 手动映射 + `// TODO: 使用 Mapperly 映射` |

#### 迁移的方法

- `GenerateRandomNumbersAsync` - 生成随机号码
- `CalculateWinningAmountAsync` - 计算中奖金额
- `CalculateK8Prize` (private) - 计算快乐8单注奖金
- `GetPagedListAsync` - 获取分页列表（按组聚合）
- `DeleteByTermNumberAsync` - 根据期号删除
- `GetStatisticsAsync` - 获取统计数据
- `CalculateMatchCount` (private) - 计算匹配号码数

### 2. LotterySSQSimulationService（双色球模拟服务）

- **原文件**: `src/DFApp.Application/Lottery/Simulation/LotterySSQSimulationService.cs` (10561字符)
- **新文件**: `src/DFApp.Web/Services/Lottery/Simulation/LotterySSQSimulationService.cs`
- **实体**: `LotterySimulation` (Guid 主键)
- **DTO**: `LotterySimulationDto`, `CreateUpdateLotterySimulationDto`, `GenerateRandomNumbersDto`, `WinningStatisticsDto`, `StatisticsDto`

#### 主要变更点

| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `CrudAppService<...>` | `CrudServiceBase<...>` |
| 仓储 | `IRepository<LotterySimulation, Guid>` | `ISqlSugarRepository<LotterySimulation, Guid>` |
| 依赖仓储 | `IRepository<LotteryResult, long>` | `ISqlSugarRepository<LotteryResult, long>` |
| 依赖仓储 | `IRepository<LotteryPrizegrades, long>` | `ISqlSugarRepository<LotteryPrizegrades, long>` |
| 权限 | `[Authorize]` 特性 + 策略名称 | 移除 |
| 查询 | `AsyncExecuter.MaxAsync()` | `GetQueryable()` + `.ToList()` + LINQ `.Max()` |
| 分组查询 | `AsyncExecuter.ToListAsync()` + EF Core GroupBy | 内存分组 |
| 映射 | 自动（ABP ObjectMapper） | 手动映射 + `// TODO: 使用 Mapperly 映射` |

#### 迁移的方法

- `GenerateRandomNumbersAsync` - 生成随机号码（红球6个+蓝球1个）
- `CalculateWinningAmountAsync` - 计算中奖金额
- `CalculatePrizeAmount` (private) - 计算具体奖项金额（一等奖到六等奖）
- `GetStatisticsAsync` - 获取统计数据
- `DeleteByTermNumberAsync` - 删除指定期号模拟数据
- `GetPagedListAsync` - 获取分页列表（按组聚合，7个号码一组）

### 3. LotteryService（彩票信息服务 - 最复杂服务）

- **原文件**: `src/DFApp.Application/Lottery/LotteryService.cs` (25185字符)
- **新文件**: `src/DFApp.Web/Services/Lottery/LotteryService.cs`
- **实体**: `LotteryInfo` (long 主键)
- **DTO**: `LotteryDto`, `CreateUpdateLotteryDto`, `LotteryGroupDto`, `LotteryCombinationDto`, `StatisticsWinDto`, `StatisticsWinItemDto`, `StatisticsWinItemRequestDto`, `StatisticsInputDto`, `ConstsDto`

#### 主要变更点

| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `CrudAppService<...>` | `CrudServiceBase<...>` |
| 仓储 | `IRepository<LotteryInfo, long>` | `ISqlSugarRepository<LotteryInfo, long>` |
| 只读仓储 | `IReadOnlyRepository<LotteryResult, long>` | `ISqlSugarRepository<LotteryResult, long>` |
| 只读仓储 | `IReadOnlyRepository<LotteryPrizegrades, long>` | `ISqlSugarRepository<LotteryPrizegrades, long>` |
| 事务 | `IUnitOfWorkManager.Begin(true, true)` | `Repository.BeginTran()` / `CommitTran()` / `RollbackTran()` |
| 权限 | `[Authorize]` 特性 | 移除 |
| 参数校验 | `Check.NotNullOrWhiteSpace()` | `BusinessException` |
| 映射 | `ObjectMapper.Map<...>()` | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| 导航属性 | `result.Prizegrades = await ...GetListAsync()` | 直接查询 `LotteryPrizegrades` 仓储 |
| 排序 | `System.Linq.Dynamic.Core` | 保持使用 `System.Linq.Dynamic.Core` |

#### 迁移的方法

- `GetStatisticsWinItem` - 获取中奖统计项（分页）
- `GetStatisticsWinItemInternal` (private) - 中奖统计项内部实现
- `GetStatisticsWin` - 获取中奖统计
- `GetLotteryResultData` (private) - 获取彩票开奖结果数据
- `GetLotteryInfoData` (private) - 获取彩票购买信息数据
- `JudgeWin` (private) - 判断双色球中奖金额
- `GetActualAmount` (private) - 获取实际奖金金额（支持正则解析复合金额）
- `CreateLotteryBatch` - 批量创建彩票
- `CalculateCombination` - 计算组合投注
- `GetLotteryConst` - 获取彩票常量
- `GetStatisticsWinItemInputDto` - 获取中奖统计项（通过输入 DTO）
- `GetListGrouped` - 获取分组列表（分页）
- `GetLatestIndexNoByType` - 获取指定类型的最新期号
- `DeleteLotteryGroup` - 根据组号删除彩票组
- `DeleteLotteryGroupByIndexNoAndGroupId` - 根据期号和组号删除彩票组

## 已知编译问题

### 1. LotterySimulation 实体的 `required` 成员问题

`LotterySimulation` 实体定义中 `BallType` 和 `GameType` 属性使用了 `required` 关键字：

```csharp
public required LotteryBallType BallType { get; set; }
public required LotteryGameType GameType { get; set; }
```

这导致 `CrudServiceBase<LotterySimulation, Guid, ...>` 和 `ISqlSugarRepository<LotterySimulation, Guid>` 的 `new()` 约束无法满足。

**影响范围**: LotteryKL8SimulationService 和 LotterySSQSimulationService

**解决方案**: 需要在后续阶段修改 `LotterySimulation` 实体，移除 `required` 关键字或提供默认值。

### 2. ISugarQueryable 与 LINQ 扩展方法冲突

`ISugarQueryable<T>` 的 `OrderByDescending`、`ThenByDescending`、`FirstOrDefault` 等方法与 `System.Linq.ParallelEnumerable` 扩展方法存在冲突。

**解决方案**: 在链式调用中添加 `.ToList()` 将结果转换为 `List<T>` 后再使用 LINQ 方法。

## 未迁移的依赖

| 依赖 | 状态 | 说明 |
|------|------|------|
| `LotteryConst` | ✅ 已在 Domain.Shared 中 | 常量类，无需迁移 |
| `LotteryBallType` | ✅ 已在 Domain.Shared 中 | 枚举类型 |
| `LotteryGameType` | ✅ 已在 Domain.Shared 中 | 枚举类型 |
| `LotteryKL8PlayType` | ✅ 已在 Domain.Shared 中 | 枚举类型 |
| `LotteryResult` 实体 | ✅ 已迁移 | `src/DFApp.Web/Domain/Lottery/LotteryResult.cs` |
| `LotteryPrizegrades` 实体 | ✅ 已迁移 | `src/DFApp.Web/Domain/Lottery/LotteryPrizegrades.cs` |
| `LotterySimulation` 实体 | ✅ 已迁移 | `src/DFApp.Web/Domain/Lottery/LotterySimulation.cs` |
| `LotteryInfo` 实体 | ✅ 已迁移 | `src/DFApp.Web/Domain/Lottery/LotteryInfo.cs` |
| `LotteryResultService` | ✅ 已迁移 | `src/DFApp.Web/Services/Lottery/LotteryResultService.cs` |
| `LotteryDataFetchService` | ❌ 未迁移 | 数据抓取服务，本批次未涉及 |
| `CompoundLotteryService` | ❌ 未迁移 | 组合彩票服务，本批次未涉及 |
| DTO 类 | ✅ 仍在 Application.Contracts | 暂不迁移 DTO |

## 文件结构

```
src/DFApp.Web/Services/Lottery/
├── LotteryResultService.cs          (已迁移 - Batch 3)
├── LotteryService.cs                (新增 - Batch 4)
├── Simulation/
│   ├── LotteryKL8SimulationService.cs  (新增 - Batch 4)
│   └── LotterySSQSimulationService.cs  (新增 - Batch 4)
└── Statistics/
```
