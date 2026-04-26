# Phase 3.3 + Phase 4.3 最终迁移总结

**完成时间**：2026-03-30 | **状态**：已完成 | **迁移服务总数**：14

---

## 1. 概述

### 1.1 迁移目标

- **Phase 3.3**：替换所有服务中的仓储注入，将 ABP 的 `IRepository<T, TKey>` 替换为 SqlSugar 的 `ISqlSugarRepository<T, TKey>`
- **Phase 4.3**：迁移所有继承自 `ApplicationService`（非 `CrudAppService`）的服务到新的 `AppServiceBase` 基类，以及迁移实现 `ITransientDependency` 接口的内部服务

### 1.2 总体状态

Phase 3.3 和 Phase 4.3 的迁移工作已全部完成。共迁移 **14 个服务**（12 个 ApplicationService + 2 个内部服务），分为 7 个批次，覆盖了账户管理、Aria2 管理、Telegram 登录、RSS 订阅、彩票数据抓取等所有非 CRUD 业务模块。

---

## 2. 迁移范围统计

### 2.1 总体统计

| 指标 | 数量 |
|------|------|
| 迁移服务总数 | 14 |
| ApplicationService → AppServiceBase | 10 |
| DFAppAppService → AppServiceBase | 2 |
| ITransientDependency → 普通类实现接口 | 2 |
| 涉及模块数 | 5 |
| 迁移批次数 | 7 |

### 2.2 按模块分类统计

| 模块 | 迁移服务数 | 服务列表 |
|------|-----------|---------|
| 账户管理 | 2 | AccountAppService, UserManagementAppService |
| Aria2 管理 | 1 | Aria2ManageService |
| Telegram | 1 | TGLoginService |
| RSS | 8 | RssSourceAppService, RssSubscriptionAppService, RssMirrorItemAppService, RssWordSegmentAppService, RssSubscriptionDownloadAppService, RssFetchService, RssSubscriptionService, WordSegmentService |
| 彩票 | 2 | LotteryDataFetchService, CompoundLotteryService |

### 2.3 按批次分类统计

| 批次 | 服务数 | 复杂度 | 说明 |
|------|--------|--------|------|
| Batch 1 | 2 | 中等 | 账户管理模块，含用户认证和管理逻辑 |
| Batch 2 | 2 | 中等 | Aria2 管理和 Telegram 登录，含外部服务集成 |
| Batch 3 | 2 | 中等 | RSS 源和订阅服务，含 CRUD 和业务逻辑 |
| Batch 4 | 2 | 简单 | RSS 镜像项和分词服务，含简单 CRUD |
| Batch 5 | 2 | 复杂 | RSS 订阅下载和抓取服务，含后台任务和复杂业务逻辑 |
| Batch 6 | 2 | 简单 | RSS 订阅和分词内部服务，实现接口迁移 |
| Batch 7 | 2 | 复杂 | 彩票数据抓取和组合服务，含外部 API 调用和复杂计算 |

---

## 3. 完整迁移服务列表

### 3.1 Batch 1（账户管理）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| AccountAppService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Account/AccountAppService.cs` |
| UserManagementAppService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Account/UserManagementAppService.cs` |

### 3.2 Batch 2（Aria2 + Telegram）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| Aria2ManageService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Aria2/Aria2ManageService.cs` |
| TGLoginService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/TG/TGLoginService.cs` |

### 3.3 Batch 3（RSS 源 + 订阅）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| RssSourceAppService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Rss/RssSourceAppService.cs` |
| RssSubscriptionAppService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Rss/RssSubscriptionAppService.cs` |

### 3.4 Batch 4（RSS 镜像 + 分词）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| RssMirrorItemAppService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Rss/RssMirrorItemAppService.cs` |
| RssWordSegmentAppService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Rss/RssWordSegmentAppService.cs` |

### 3.5 Batch 5（RSS 下载 + 抓取）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| RssSubscriptionDownloadAppService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Rss/RssSubscriptionDownloadAppService.cs` |
| RssFetchService | DFAppAppService | AppServiceBase | `src/DFApp.Web/Services/Rss/RssFetchService.cs` |

### 3.6 Batch 6（RSS 内部服务）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| RssSubscriptionService | ITransientDependency | 普通类实现接口 | `src/DFApp.Web/Services/Rss/RssSubscriptionService.cs` |
| WordSegmentService | ITransientDependency | 普通类实现接口 | `src/DFApp.Web/Services/Rss/WordSegmentService.cs` |

### 3.7 Batch 7（彩票服务）

| 服务 | 原基类 | 新基类 | 新文件路径 |
|------|--------|--------|-----------|
| LotteryDataFetchService | DFAppAppService | AppServiceBase | `src/DFApp.Web/Services/Lottery/LotteryDataFetchService.cs` |
| CompoundLotteryService | ApplicationService | AppServiceBase | `src/DFApp.Web/Services/Lottery/CompoundLotteryService.cs` |

---

## 4. 通用迁移模式

所有 14 个服务均遵循以下迁移变更模式（与 Phase 4.2 一致）：

### 4.1 基类替换

| 原基类 | 新基类 | 适用场景 |
|--------|--------|---------|
| `ApplicationService` | `AppServiceBase` | ABP 标准应用服务 |
| `DFAppAppService` | `AppServiceBase` | 项目自定义应用服务基类 |
| `ITransientDependency` | 普通类实现接口 | 内部服务，直接实现业务接口 |

### 4.2 仓储替换

| 原仓储 | 新仓储 | 说明 |
|--------|--------|------|
| `IRepository<TEntity, TKey>` | `ISqlSugarRepository<TEntity, TKey>` | 通用仓储替换 |
| `IReadOnlyRepository<TEntity, TKey>` | `ISqlSugarRepository<TEntity, TKey>` | 只读仓储统一使用通用仓储 |
| `IRepository<User, Guid>` | `ISqlSugarRepository<User, Guid>` | 用户仓储替换 |

### 4.3 查询方式替换

| 原方式 | 新方式 | 说明 |
|--------|--------|------|
| `AsyncExecuter.ToListAsync()` | `.ToListAsync()` | SqlSugar 原生异步 |
| `AsyncExecuter.CountAsync()` | `.CountAsync()` | SqlSugar 原生异步 |
| `GetQueryableAsync()` | `GetQueryable()` | 同步获取查询对象 |
| `Repository.GetAsync(id)` | `Repository.GetByIdAsync(id)` | 按主键获取 |
| `Repository.GetListAsync()` | `Repository.GetListAsync()` | 获取列表（保持不变） |

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

### 4.6 导航查询替代

| 原方式 | 新方式 |
|--------|--------|
| `.Include(x => x.Navigation)` | 通过外键仓储批量查询 |
| 导航属性直接访问 | 先获取外键 ID 列表，再批量查询关联实体 |

### 4.7 工作单元移除

| 原方式 | 新方式 |
|--------|--------|
| `IUnitOfWorkManager.Begin()` | 移除（SqlSugar 自带事务管理） |
| `IUnitOfWorkManager` 事务 | `Repository.BeginTran()` / `CommitTran()` / `RollbackTran()` |

### 4.8 映射替换

| 原方式 | 新方式 |
|--------|--------|
| `ObjectMapper.Map<TEntity, TDto>(entity)` | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| ABP 自动映射 | 手动属性赋值 |

### 4.9 权限移除

| 原方式 | 新方式 |
|--------|--------|
| `[Authorize(DFAppPermissions.XXX.Default)]` | 移除（待后续添加） |
| 权限策略属性 | 移除 |

### 4.10 构造函数变更

所有服务的构造函数统一新增以下参数：

```csharp
ICurrentUser currentUser,      // 当前用户信息
IPermissionChecker permissionChecker  // 权限检查器
```

同时将 `IRepository` 参数替换为 `ISqlSugarRepository`。

---

## 5. 未迁移的依赖

### 5.1 服务级别的外部依赖

| 依赖 | 所属服务 | 状态 | 说明 |
|------|---------|------|------|
| `Aria2RpcClient` | Aria2ManageService | ❌ 未迁移 | Aria2 RPC 客户端 |
| `ListenTelegramService` | TGLoginService | ❌ 未迁移 | Telegram 监听服务 |
| `IAria2Service` | RssMirrorItemAppService, RssSubscriptionService | ❌ 未迁移 | Aria2 服务接口 |
| `IRssSubscriptionService` | RssSubscriptionDownloadAppService | ❌ 未迁移 | RSS 订阅服务接口 |
| `IBackgroundTaskQueue` | RssSubscriptionService | ❌ 未迁移 | 后台任务队列接口 |

### 5.2 通用未迁移依赖

| 依赖 | 类型 | 状态 | 说明 |
|------|------|------|------|
| Mapperly 映射器 | 映射 | ❌ 未迁移 | 所有映射使用 `// TODO: 使用 Mapperly 映射` 伪代码 |
| 权限特性 | 授权 | ❌ 未迁移 | 所有 `[Authorize]` 已移除，待后续添加 |
| Controller 层 | API | ❌ 未迁移 | 尚未创建对应的 API Controller |
| DTO 类 | 数据传输 | ❌ 未迁移 | 仍在 `src/DFApp.Application.Contracts/` 中 |

---

## 6. 文件结构

迁移后的服务文件结构如下：

```
src/DFApp.Web/Services/
├── Account/
│   ├── AccountAppService.cs
│   └── UserManagementAppService.cs
├── Aria2/
│   ├── Aria2ManageService.cs
│   └── Aria2Service.cs（Phase 4.2 已迁移）
├── Lottery/
│   ├── LotteryDataFetchService.cs
│   ├── CompoundLotteryService.cs
│   ├── LotteryResultService.cs（Phase 4.2 已迁移）
│   ├── LotteryService.cs（Phase 4.2 已迁移）
│   └── Simulation/
│       ├── LotteryKL8SimulationService.cs（Phase 4.2 已迁移）
│       └── LotterySSQSimulationService.cs（Phase 4.2 已迁移）
├── Rss/
│   ├── RssFetchService.cs
│   ├── RssMirrorItemAppService.cs
│   ├── RssSourceAppService.cs
│   ├── RssSubscriptionAppService.cs
│   ├── RssSubscriptionDownloadAppService.cs
│   ├── RssSubscriptionService.cs
│   ├── RssWordSegmentAppService.cs
│   └── WordSegmentService.cs
└── TG/
    └── TGLoginService.cs
```

---

## 7. 下一步工作

### 7.1 Phase 4.4：迁移 DTO 映射（Mapperly）

- 为每个服务创建对应的 Mapperly 映射器类
- 替换所有 `// TODO: 使用 Mapperly 映射` 伪代码
- 使用 `[Mapper]` 特性标记映射器类
- 实现实体到 DTO 和 DTO 到实体的映射方法

### 7.2 Phase 5：创建 Controller 层

为每个服务创建对应的 API Controller：

- 路由采用 `/api/app/{kebab-case-entity}` 模式
- 添加权限特性
- 添加参数验证
- 添加 Swagger 文档注释

### 7.3 Phase 6：添加权限控制

- 为每个服务的公共方法添加权限特性
- 定义相应的权限名称
- 确保权限检查逻辑正确实现

### 7.4 迁移后台任务（Background Workers）

- 迁移 `GasolinePriceRefresher` 等后台服务
- 迁移 `Aria2RpcClient` 等外部服务客户端
- 迁移 `ListenTelegramService` 等 Telegram 相关服务

---

## 8. 相关文档

| 文档 | 说明 |
|------|------|
| [Phase 3.3 + 4.2 最终迁移总结](phase3.3-4.2-final-migration-summary.md) | Phase 3.3 + 4.2 的 17 个 CrudAppService 迁移详情 |
| [Phase 4.3 Batch 1 迁移总结](phase4.3-batch1-migration-summary.md) | 账户管理模块迁移详情 |
| [Phase 4.3 Batch 2 迁移总结](phase4.3-batch2-migration-summary.md) | Aria2 + Telegram 模块迁移详情 |
| [Phase 4.3 Batch 3 迁移总结](phase4.3-batch3-migration-summary.md) | RSS 源 + 订阅迁移详情 |
| [Phase 4.3 Batch 4 迁移总结](phase4.3-batch4-migration-summary.md) | RSS 镜像 + 分词迁移详情 |
| [Phase 4.3 Batch 5 迁移总结](phase4.3-batch5-migration-summary.md) | RSS 下载 + 抓取迁移详情 |
| [Phase 4.3 Batch 6 迁移总结](phase4.3-batch6-migration-summary.md) | RSS 内部服务迁移详情 |
| [Phase 4.3 Batch 7 迁移总结](phase4.3-batch7-migration-summary.md) | 彩票服务迁移详情 |
| [框架迁移计划](framework-migration-plan.md) | 整体迁移计划 |
| [执行进度](执行进度.md) | 迁移执行进度跟踪 |
