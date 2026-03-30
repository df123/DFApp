# Phase 4.3 Batch 3 - RSS 服务迁移摘要

## 迁移日期
2026-03-30

## 迁移的服务

### 1. RssSourceAppService
- **原文件**: `src/DFApp.Application/Rss/RssSourceAppService.cs`
- **新文件**: `src/DFApp.Web/Services/Rss/RssSourceAppService.cs`

#### 主要变更
| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `ApplicationService` (ABP) | `AppServiceBase` |
| 仓储 | `IRepository<RssSource, long>` (ABP) | `ISqlSugarRepository<RssSource, long>` |
| 查询方式 | `GetQueryableAsync()` + `AsyncExecuter` | `GetQueryable()` + SqlSugar 扩展 |
| 对象映射 | `ObjectMapper.Map<>()` (ABP AutoMap) | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| 异常类型 | `UserFriendlyException` (ABP) | `BusinessException` |
| 权限控制 | `[Authorize]` 特性 | 移除（由 Controller 层或中间件处理） |
| 日志 | `Logger` (ABP 基类提供) | `ILogger<T>` 注入 |
| 分页结果 | `Volo.Abp.Application.Dtos.PagedResultDto` | `DFApp.Web.Services.ElectricVehicle.PagedResultDto` |
| 排序参数 | `PagedAndSortedResultRequestDto` (ABP) | `Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto`（完全限定名） |

#### 方法迁移情况
| 方法 | 状态 | 备注 |
|------|------|------|
| `GetListAsync` | ✅ 已迁移 | 使用 SqlSugar 的 `GetQueryable()` + `CountAsync()`/`ToListAsync()` |
| `GetAsync` | ✅ 已迁移 | 使用 `GetByIdAsync()` + `EnsureEntityExists()` |
| `CreateAsync` | ✅ 已迁移 | URL 唯一性验证保留 |
| `UpdateAsync` | ✅ 已迁移 | URL 唯一性验证保留 |
| `DeleteAsync` | ✅ 已迁移 | 直接调用 `DeleteAsync(id)` |
| `TriggerFetchAsync` | ✅ 已迁移 | 后台任务触发用 TODO 标记 |

### 2. RssSubscriptionAppService
- **原文件**: `src/DFApp.Application/Rss/RssSubscriptionAppService.cs`
- **新文件**: `src/DFApp.Web/Services/Rss/RssSubscriptionAppService.cs`

#### 主要变更
| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `ApplicationService` (ABP) | `AppServiceBase` |
| 仓储 | `IRepository<RssSubscription, long>` + `IRepository<RssSource, long>` (ABP) | `ISqlSugarRepository<RssSubscription, long>` + `ISqlSugarRepository<RssSource, long>` |
| 查询方式 | `GetQueryableAsync()` + `AsyncExecuter` | `GetQueryable()` + SqlSugar 扩展 |
| 对象映射 | `ObjectMapper.Map<>()` (ABP AutoMap) | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| 异常类型 | `UserFriendlyException` (ABP) | `BusinessException` |
| 并发处理 | `AbpDbConcurrencyException` 捕获重试 | 移除并发重试逻辑（SqlSugar 不使用 ConcurrencyStamp） |
| 权限控制 | `[Authorize]` 特性 | 移除 |
| 日志 | `Logger` (ABP 基类提供) | `ILogger<T>` 注入 |

#### 方法迁移情况
| 方法 | 状态 | 备注 |
|------|------|------|
| `GetListAsync` | ✅ 已迁移 | 支持按 IsEnabled、RssSourceId、Filter 过滤，填充 RssSourceName |
| `GetAsync` | ✅ 已迁移 | 通过外键查询填充 RssSourceName |
| `CreateAsync` | ✅ 已迁移 | 手动映射 DTO 到实体 |
| `UpdateAsync` | ✅ 已迁移 | 移除了 ABP 并发异常重试逻辑 |
| `DeleteAsync` | ✅ 已迁移 | 直接调用 `DeleteAsync(id)` |
| `ToggleEnableAsync` | ✅ 已迁移 | 移除了 ABP 并发异常重试逻辑 |

## 优化项

1. **移除并发重试逻辑**: 原代码中 `UpdateAsync` 和 `ToggleEnableAsync` 使用 `AbpDbConcurrencyException` 进行并发重试。迁移后移除此逻辑，因为：
   - 新架构使用 SqlSugar 而非 EF Core 的并发戳机制
   - `ConcurrencyStamp` 字段在实体中已不再需要

2. **统一实体检查**: 使用 `AppServiceBase.EnsureEntityExists()` 替代直接依赖 ABP 的 `GetAsync()` 抛异常行为

3. **导航属性替代**: 原代码通过 `subscription.Source` 导航属性访问 RSS 源名称，迁移后通过 `_rssSourceRepository` 外键查询实现

## 未迁移的依赖

1. **DTO 类型**: 仍使用 `DFApp.Application.Contracts` 中的 DTO（`RssSourceDto`、`RssSubscriptionDto` 等），这些 DTO 继承自 ABP 的 `EntityDto` 和 `PagedAndSortedResultRequestDto`
2. **后台任务集成**: `TriggerFetchAsync` 中的后台任务触发机制需要后续集成 Quartz.NET 调度
3. **Mapperly 映射**: 所有手动映射标记了 `// TODO: 使用 Mapperly 映射`，待后续统一替换

## 文件清单

| 文件 | 操作 |
|------|------|
| `src/DFApp.Web/Services/Rss/RssSourceAppService.cs` | 新建 |
| `src/DFApp.Web/Services/Rss/RssSubscriptionAppService.cs` | 新建 |
