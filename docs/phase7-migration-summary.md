# Phase 7 迁移总结：基础设施迁移（压缩版）

**完成时间**：2026-04-02 | **状态**：已完成 | **迁移范围**：Quartz.NET 后台任务、全局异常处理收尾、SignalR、中间件精简

## 概述

Phase 7 是基础设施迁移，共 4 个子任务。其中 7.2（SignalR）和 7.4（中间件精简）在 Phase 1-6 期间已完成，本次实际工作为 **7.1（Quartz.NET）** 和 **7.3（全局异常处理收尾）**。

## 7.1 Quartz.NET 后台任务迁移

将 4 个后台任务从 ABP `QuartzBackgroundWorkerBase` 迁移到标准 Quartz.NET `IJob` 接口，并迁移前置依赖 `GasolinePriceRefresher` 服务。

### 迁移的 Job

| Job | 复杂度 | 调度策略 | 依赖服务 |
|-----|--------|---------|---------|
| `GasolinePriceRefreshJob` | 低 | Cron `0 0 21 * * ?`（每晚 21:00） | `GasolinePriceRefresher` |
| `DiskSpaceCheckJob` | 低 | Simple 每 10 分钟 | `IRssSubscriptionService` |
| `LotteryResultJob` | 高 | Cron `0 0 23 * * ?`（每晚 23:00） | `ISqlSugarRepository`×4, `LotteryMapper`, `IHttpClientFactory` |
| `RssMirrorFetchJob` | 高 | Simple 每 5 分钟 | `ISqlSugarRepository`×3, `ISqlSugarReadOnlyRepository`, `ISqlSugarClient`, `IWordSegmentService`, `IRssSubscriptionService` |

### 前置依赖迁移

| 服务 | 说明 |
|------|------|
| `GasolinePriceRefresher` | 从旧 Domain 层迁移到 `Services/ElectricVehicle/GasolinePriceRefresher.cs`，注册为 Scoped DI |

### ABP → 标准 Quartz.NET 替换映射

| ABP 原始 | 迁移后 |
|----------|--------|
| 继承 `QuartzBackgroundWorkerBase` | 实现 `IJob` 接口 |
| `IRepository<T, TKey>` | `ISqlSugarRepository<T, TKey>` |
| `IReadOnlyRepository<T, TKey>` | `ISqlSugarReadOnlyRepository<T, TKey>` |
| `IObjectMapper.Map<>()` | Mapperly `LotteryMapper.MapToEntityFromExternalResultItem()` |
| `IUnitOfWorkManager.Begin()` | `_repository.BeginTran()` / `ISqlSugarClient.Ado.BeginTran()` |
| `InsertManyAsync()` | `InsertAsync(List<T>)` / `InsertRangeAsync()` |
| `GetQueryableAsync()` | `GetQueryable()` |
| `GetAsync(id)` | `GetByIdAsync(id)` |
| `FirstOrDefaultAsync()` | `GetFirstOrDefaultAsync()` |

### Job 生命周期管理（Program.cs）

```csharp
// 注册 DI
builder.Services.AddScoped<GasolinePriceRefresher>();

// 注册 Job 并配置调度
q.AddJob<GasolinePriceRefreshJob>(j => j.WithIdentity("GasolinePriceRefreshJob"))
    .AddTrigger(t => t.WithCronSchedule("0 0 21 * * ?"));
q.AddJob<DiskSpaceCheckJob>(j => j.WithIdentity("DiskSpaceCheckJob"))
    .AddTrigger(t => t.WithSimpleSchedule(s => s.WithIntervalInMinutes(10).RepeatForever()));
q.AddJob<LotteryResultJob>(j => j.WithIdentity("LotteryResultJob"))
    .AddTrigger(t => t.WithCronSchedule("0 0 23 * * ?"));
q.AddJob<RssMirrorFetchJob>(j => j.WithIdentity("RssMirrorFetchJob"))
    .AddTrigger(t => t.WithSimpleSchedule(s => s.WithIntervalInMinutes(5).RepeatForever()));
```

## 7.2 SignalR

✅ **已完成**（Phase 1-6 期间），无需修改。`NotificationHub` 已使用标准 SignalR Hub 模式。

## 7.3 全局异常处理收尾

替换 `DFApp.Web/` 中残留的 ABP 异常类型。

| 文件 | 替换数量 | 替换内容 |
|------|---------|---------|
| `Data/Configuration/ConfigurationInfoRepository.cs` | 3 处 | `UserFriendlyException` → `BusinessException` |

## 7.4 中间件精简

✅ **已完成**（Phase 1-6 期间）。`src/DFApp.Web/` 中无 ABP 中间件残留。

## 文件变更清单

**新建文件（5 个）**：
- `src/DFApp.Web/Background/GasolinePriceRefreshJob.cs`
- `src/DFApp.Web/Background/DiskSpaceCheckJob.cs`
- `src/DFApp.Web/Background/LotteryResultJob.cs`
- `src/DFApp.Web/Background/RssMirrorFetchJob.cs`
- `src/DFApp.Web/Services/ElectricVehicle/GasolinePriceRefresher.cs`

**修改文件（2 个）**：
- `src/DFApp.Web/Program.cs` — 注册 4 个 Job 调度 + GasolinePriceRefresher DI
- `src/DFApp.Web/Data/Configuration/ConfigurationInfoRepository.cs` — 3 处异常替换

## 统计

| 指标 | 数量 |
|------|------|
| 新建文件 | 5 |
| 修改文件 | 2 |
| 迁移 Job | 4 |
| 异常替换 | 3 处 |
| 新引入编译错误 | 0 |

## 编译验证

- Phase 7 修改未引入新的编译错误
- 预存 3 个编译错误来自 `DFApp.Application/Aria2BackgroundWorker.cs`（非本次引入，属遗留 ABP 依赖）

## 清理验证

- ✅ `src/DFApp.Web/` 中无 `QuartzBackgroundWorkerBase` 引用残留
- ✅ `src/DFApp.Web/DFApp.Web.csproj` 中无 `Volo.Abp` 包残留
- ⚠️ `DFAppWebModule.cs.bak` 备份文件建议后续清理

## 遗留问题

### 命名空间/依赖未迁移
- `LotteryInputDto` 和 `LotteryConst` 仍来自旧命名空间 `DFApp.Lottery`（未迁移到 Web 项目）
- `IRssSubscriptionService` 和 `IWordSegmentService` 接口定义在旧 Domain 项目中

### 旧文件未删除（受约束保护，Phase 9 清理）
- `DFApp.Application/Background/LotteryResultTimer.cs`
- `DFApp.Application/Background/RssMirrorFetchWorker.cs`
- `DFApp.Application/Background/GasolinePriceRefreshWorker.cs`
- `DFApp.Application/Background/DiskSpaceCheckWorker.cs`
- `DFApp.Application.csproj` 中的 `Volo.Abp.BackgroundWorkers.Quartz` 包引用
- `DFAppApplicationModule.cs` 中的 `[DependsOn(typeof(AbpBackgroundWorkersQuartzModule))]`

### 不属于本次迁移范围
- `Aria2BackgroundWorker.cs` 和 `ListenTelegramService.cs` 仍使用 ABP 依赖（`IRepository`、`IObjectMapper`、`IUnitOfWorkManager`）
