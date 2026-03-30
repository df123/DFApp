# Phase 4.2 Batch 1 迁移总结 - 4 个简单 CrudAppService

**完成时间**：2026-03-30 | **状态**：已完成

## 概述

本次迁移完成了 Phase 4.2（迁移 CrudAppService）的第一批次，包含 4 个简单的 CrudAppService。同时完成了 Phase 3.3（替换仓储注入）的工作。

## 迁移服务列表

| # | 服务名 | 原文件 | 新文件 | 主键类型 |
|---|--------|--------|--------|---------|
| 1 | DynamicIPService | `src/DFApp.Application/IP/DynamicIPService.cs` | `src/DFApp.Web/Services/IP/DynamicIPService.cs` | `Guid` |
| 2 | ElectricVehicleService | `src/DFApp.Application/ElectricVehicle/ElectricVehicleService.cs` | `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleService.cs` | `Guid` |
| 3 | LotteryResultService | `src/DFApp.Application/Lottery/LotteryResultService.cs` | `src/DFApp.Web/Services/Lottery/LotteryResultService.cs` | `long` |
| 4 | MediaInfoService | `src/DFApp.Application/Media/MediaInfoService.cs` | `src/DFApp.Web/Services/Media/MediaInfoService.cs` | `long` |

## 各服务详细变更

### 1. DynamicIPService

**原文件**: `src/DFApp.Application/IP/DynamicIPService.cs`  
**新文件**: `src/DFApp.Web/Services/IP/DynamicIPService.cs`

#### 主要变更：
1. **基类变更**：`CrudAppService<DynamicIP, DynamicIPDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateDynamicIPDto>` → `CrudServiceBase<DynamicIP, Guid, DynamicIPDto, CreateUpdateDynamicIPDto, CreateUpdateDynamicIPDto>`
2. **仓储变更**：`IRepository<DynamicIP, Guid>` → `ISqlSugarRepository<DynamicIP, Guid>`
3. **移除**：`[Authorize(DFAppPermissions.DynamicIP.Default)]` 特性、所有权限策略名称设置
4. **新增**：`MapToGetOutputDto`、`MapToEntity`、`MapToEntity` 重载三个映射方法
5. **构造函数**：新增 `ICurrentUser` 和 `IPermissionChecker` 参数

#### 业务逻辑：
- 纯 CRUD 服务，无自定义方法
- 原服务仅设置权限策略名称，无额外业务逻辑

---

### 2. ElectricVehicleService

**原文件**: `src/DFApp.Application/ElectricVehicle/ElectricVehicleService.cs`  
**新文件**: `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleService.cs`

#### 主要变更：
1. **基类变更**：`CrudAppService<ElectricVehicle, ElectricVehicleDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateElectricVehicleDto>` → `CrudServiceBase<ElectricVehicle, Guid, ElectricVehicleDto, CreateUpdateElectricVehicleDto, CreateUpdateElectricVehicleDto>`
2. **仓储变更**：`IRepository<ElectricVehicle, Guid>` → `ISqlSugarRepository<ElectricVehicle, Guid>`
3. **移除**：`[Authorize(DFAppPermissions.ElectricVehicle.Default)]` 特性、所有权限策略名称设置
4. **新增**：三个映射方法
5. **构造函数**：新增 `ICurrentUser` 和 `IPermissionChecker` 参数

#### 业务逻辑：
- 纯 CRUD 服务，无自定义方法
- 原服务仅设置权限策略名称，无额外业务逻辑

---

### 3. LotteryResultService

**原文件**: `src/DFApp.Application/Lottery/LotteryResultService.cs`  
**新文件**: `src/DFApp.Web/Services/Lottery/LotteryResultService.cs`

#### 主要变更：
1. **基类变更**：`CrudAppService<LotteryResult, LotteryResultDto, long, PagedAndSortedResultRequestDto, CreateUpdateLotteryResultDto>` → `CrudServiceBase<LotteryResult, long, LotteryResultDto, CreateUpdateLotteryResultDto, CreateUpdateLotteryResultDto>`
2. **仓储变更**：`IRepository<LotteryResult, long>` → `ISqlSugarRepository<LotteryResult, long>`
3. **移除**：`[Authorize(DFAppPermissions.Lottery.Default)]` 特性
4. **新增**：三个映射方法
5. **构造函数**：新增 `ICurrentUser` 和 `IPermissionChecker` 参数

#### 业务逻辑：
- 纯 CRUD 服务，无自定义方法
- 原服务未设置任何权限策略名称

---

### 4. MediaInfoService

**原文件**: `src/DFApp.Application/Media/MediaInfoService.cs`  
**新文件**: `src/DFApp.Web/Services/Media/MediaInfoService.cs`

#### 主要变更：
1. **基类变更**：`CrudAppService<MediaInfo, MediaInfoDto, long, FilterAndPagedAndSortedResultRequestDto, CreateUpdateMediaInfoDto>` → `CrudServiceBase<MediaInfo, long, MediaInfoDto, CreateUpdateMediaInfoDto, CreateUpdateMediaInfoDto>`
2. **仓储变更**：`IRepository<MediaInfo, long>` → `ISqlSugarRepository<MediaInfo, long>`
3. **移除**：
   - `[Authorize]` 特性和权限策略名称设置
   - `_mediaInfoRepository.DisableTracking()` 调用
   - `CreateFilteredQueryAsync` 重写方法（改为 `GetFilteredPagedListAsync` 公共方法）
4. **新增**：
   - `GetFilteredPagedListAsync(string? filter, int pageIndex, int pageSize)` — 替代原 `CreateFilteredQueryAsync`，支持按 MediaId/ChatTitle/Message/MimeType 过滤并分页
   - `GetChartDataAsync()` — 替代原 `GetChartData()`，按 ChatTitle 分组统计
   - `DeleteInvalidItemsAsync()` — 替代原 `DeleteInvalidItems()`，删除未完成下载且超过 1 分钟的记录
   - 三个映射方法
5. **构造函数**：新增 `ICurrentUser` 和 `IPermissionChecker` 参数，移除 `_mediaInfoRepository` 字段（直接使用基类 `Repository`）

#### 业务逻辑变更：
- `GetChartData` → `GetChartDataAsync`：移除 `DisableTracking()` 调用，直接使用 `Repository.GetListAsync()`
- `DeleteInvalidItems` → `DeleteInvalidItemsAsync`：移除 `[Authorize]` 特性，使用 `Repository.DeleteAsync()`
- `CreateFilteredQueryAsync` → `GetFilteredPagedListAsync`：从 protected 重写方法改为 public 方法，直接调用基类的 `GetPagedListAsync`

---

## 通用迁移模式

所有 4 个服务均遵循以下迁移模式：

| 变更项 | 原模式 | 新模式 |
|--------|--------|--------|
| 基类 | `CrudAppService<...>` | `CrudServiceBase<...>` |
| 仓储 | `IRepository<T, TKey>` | `ISqlSugarRepository<T, TKey>` |
| 权限 | `[Authorize]` + PolicyName | 移除（待后续添加） |
| 映射 | ABP 自动映射 | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| 构造函数 | `IRepository` | `ICurrentUser` + `IPermissionChecker` + `ISqlSugarRepository` |
| 异常 | `UserFriendlyException` | `BusinessException` |

## 编译问题（预期）

以下编译问题是预期的，将在后续阶段统一解决：

1. **`required` 成员约束**：`DynamicIP`（IP、Port）和 `MediaInfo`（ChatTitle、SavePath、MimeType）实体有 `required` 属性，不满足 `new()` 约束
2. **`IEntity<TKey>` 接口不匹配**：实体继承 `AuditedEntity<TKey>` 而非直接实现 `IEntity<TKey>` 接口
3. **`MD5` 属性缺失**：`MediaInfoDto` 中有 `MD5` 属性，但 `MediaInfo` 实体中未定义该属性

## 未迁移的依赖

1. **Mapperly 映射器**：所有映射使用 `// TODO: 使用 Mapperly 映射` 伪代码，待 Phase 4.4 统一处理
2. **权限特性**：所有 `[Authorize]` 已移除，待 Phase 6 统一添加权限控制
3. **Controller**：尚未创建对应的 API Controller，待 Phase 5 处理
4. **DTO 类**：仍在 `src/DFApp.Application.Contracts/` 中，引用了 ABP 的 `AuditedEntityDto` 等基类

## 下一步

继续迁移 Batch 2 的 CrudAppService（较复杂的服务）：
- BookkeepingExpenditureService
- ElectricVehicleChargingRecordService
- ElectricVehicleCostService
- FileUploadInfoService
