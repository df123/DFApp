# Phase 4.2 Batch 3 迁移总结 - 3 个复杂 CrudAppService

**完成时间**：2026-03-30 | **状态**：已完成

## 迁移范围

本次迁移完成了 3 个复杂的 CrudAppService 迁移，这些服务涉及大量业务逻辑、多表关联查询和后台任务。

## 迁移服务列表

| # | 服务名 | 原文件 | 新文件 | 字符数 |
|---|--------|--------|--------|--------|
| 1 | ElectricVehicleCostService | `src/DFApp.Application/ElectricVehicle/ElectricVehicleCostService.cs` | `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleCostService.cs` | 15347 |
| 2 | ExternalLinkService | `src/DFApp.Application/Media/ExternalLink/ExternalLinkService.cs` | `src/DFApp.Web/Services/Media/ExternalLinkService.cs` | 9437 |
| 3 | Aria2Service | `src/DFApp.Application/Aria2/Aria2Service.cs` | `src/DFApp.Web/Services/Aria2/Aria2Service.cs` | 21850 |

---

## 各服务迁移详情

### 1. ElectricVehicleCostService

**原文件**：`src/DFApp.Application/ElectricVehicle/ElectricVehicleCostService.cs`  
**新文件**：`src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleCostService.cs`

#### 主要变更：

1. **基类变更**
   - 从 `CrudAppService<ElectricVehicleCost, ElectricVehicleCostDto, Guid, FilterAndPagedAndSortedResultRequestDto, CreateUpdateElectricVehicleCostDto>` 迁移到
   - `CrudServiceBase<ElectricVehicleCost, Guid, ElectricVehicleCostDto, CreateUpdateElectricVehicleCostDto, CreateUpdateElectricVehicleCostDto>`

2. **仓储注入变更**
   - `IRepository<ElectricVehicle, Guid>` → `ISqlSugarRepository<ElectricVehicle, Guid>`
   - `IGasolinePriceRepository` → `ISqlSugarRepository<GasolinePrice, Guid>`（去除自定义仓储，改用通用仓储）
   - `IConfigurationInfoRepository` → `IConfigurationInfoRepository`（保留自定义仓储，因含业务方法）
   - `IRepository<ElectricVehicleChargingRecord, Guid>` → `ISqlSugarRepository<ElectricVehicleChargingRecord, Guid>`

3. **导航查询改为外键查询**
   - `CreateFilteredQueryAsync` 中 `Repository.WithDetailsAsync()` → `Repository.GetQueryable()`
   - 原 `x.Vehicle.Name` 导航属性过滤 → 子查询获取匹配的 VehicleId 列表
   - 原 `MapToGetListOutputDtoAsync` 中自动填充 Vehicle → 手动批量查询车辆并映射

4. **自定义方法迁移**
   - `GetListAsync` → `GetFilteredListAsync`（参数简化为 filter/pageIndex/pageSize）
   - `GetOilCostComparisonAsync` → 完整保留，核心业务逻辑不变
     - 配置获取逻辑保持不变
     - 电车成本计算逻辑保持不变
     - 里程计算逻辑保持不变
     - 油车成本对比逻辑保持不变
     - `IGasolinePriceRepository.GetLatestPriceAsync` → 改用通用仓储查询替代

5. **查询方式变更**
   - `AsyncExecuter.ToListAsync()` → `.ToListAsync()` / `.ToList()`
   - `ReadOnlyRepository.GetListAsync()` → `Repository.GetListAsync()`
   - `_vehicleRepository.GetAsync()` → `_vehicleRepository.GetByIdAsync()`
   - `_chargingRecordRepository.GetQueryableAsync()` → `_chargingRecordRepository.GetQueryable()`

6. **异常类型变更**
   - `UserFriendlyException` → `BusinessException`

7. **移除内容**
   - `[Authorize]` 特性
   - 权限策略名称设置（GetPolicyName 等）
   - `ReadOnlyRepository` 引用（改用主 Repository）

---

### 2. ExternalLinkService

**原文件**：`src/DFApp.Application/Media/ExternalLink/ExternalLinkService.cs`  
**新文件**：`src/DFApp.Web/Services/Media/ExternalLinkService.cs`

#### 主要变更：

1. **基类变更**
   - 从 `CrudAppService<MediaExternalLink, ExternalLinkDto, long, PagedAndSortedResultRequestDto, CreateUpdateExternalLinkDto>` 迁移到
   - `CrudServiceBase<MediaExternalLink, long, ExternalLinkDto, CreateUpdateExternalLinkDto, CreateUpdateExternalLinkDto>`

2. **仓储注入变更**
   - `IRepository<MediaExternalLink>` → `ISqlSugarRepository<MediaExternalLink, long>`
   - 新增 `IConfigurationInfoRepository` 依赖注入

3. **后台任务处理**
   - `IBackgroundTaskQueue` 保留（未迁移的依赖）
   - 后台任务中的 `IRepository<T>` → `ISqlSugarRepository<T, TKey>`
   - 后台任务中的 `IServiceProvider.GetRequiredService` 保持不变

4. **自定义方法迁移**
   - `CreateAsync` → 抛出 `BusinessException("此接口不允许使用")`
   - `UpdateAsync` → 抛出 `BusinessException("此接口不允许使用")`
   - `DeleteAsync` → 保留先移除文件再删除记录的逻辑
   - `GetExternalLink` → 完整保留后台任务逻辑
   - `RemoveFileAsync` → 完整保留后台任务逻辑

5. **查询方式变更**
   - `ReadOnlyRepository.FirstAsync()` → `Repository.GetFirstOrDefaultAsync()`
   - 后台任务中的仓储操作改为使用 `ISqlSugarRepository`

6. **移除内容**
   - `[Authorize]` 特性
   - `Check.NotNullOrWhiteSpace` → `BusinessException`
   - `ConcurrencyStamp` 赋值（由 SqlSugar AOP 自动处理）

---

### 3. Aria2Service

**原文件**：`src/DFApp.Application/Aria2/Aria2Service.cs`  
**新文件**：`src/DFApp.Web/Services/Aria2/Aria2Service.cs`

#### 主要变更：

1. **基类变更**
   - 从 `CrudAppService<TellStatusResult, TellStatusResultDto, long, FilterAndPagedAndSortedResultRequestDto, TellStatusResultDto>` 迁移到
   - `CrudServiceBase<TellStatusResult, long, TellStatusResultDto, TellStatusResultDto, TellStatusResultDto>`

2. **仓储注入变更**
   - `ITellStatusResultRepository` → `ISqlSugarRepository<TellStatusResult, long>`（去除自定义仓储，改用通用仓储）
   - 新增 `ISqlSugarRepository<FilesItem, int>` 依赖（替代导航查询）
   - `IConfigurationInfoRepository` → 保留
   - `IQueueManagement` → 保留（未迁移的依赖）
   - `IKeywordFilterRuleRepository` → 保留（未迁移的依赖）
   - 新增 `ILogger<Aria2Service>` 依赖（替代 ABP 的 `Logger` 属性）

3. **导航查询改为外键查询**
   - `ReadOnlyRepository.WithDetailsAsync()` → `Repository.GetQueryable()` + `_filesItemRepository.GetListAsync(f => resultIds.Contains(f.ResultId))`
   - 所有 `data.Files` 导航属性访问 → 通过 `_filesItemRepository` 外键查询 `f.ResultId == data.Id`
   - `GetAllExternalLinksAsync` 中批量获取文件 → 预加载所有文件并按 ResultId 分组

4. **自定义方法迁移**
   - `GetListAsync` → `GetFilteredListAsync`（参数简化为 filter/pageIndex/pageSize）
     - 保留文件路径截断为文件名的逻辑
     - 保留文件名过滤逻辑
   - `GetExternalLink` → `GetExternalLinkAsync`（方法名统一加 Async 后缀）
   - `GetAllExternalLinks` → `GetAllExternalLinksAsync`
   - `DeleteAsync` → 保留删除关联文件的逻辑
   - `DeleteAllAsync` → 完整保留
   - `ClearDownloadDirectoryAsync` → 完整保留
   - `AddDownloadAsync` → 完整保留（最复杂的方法）
     - torrent 文件解析逻辑保持不变
     - VideoOnly 和关键词过滤逻辑保持不变
     - Aria2 RPC 请求构建逻辑保持不变
     - 队列添加逻辑保持不变

5. **查询方式变更**
   - `ReadOnlyRepository.AnyAsync()` → `Repository.GetQueryable().Any()`
   - `ReadOnlyRepository.GetListAsync(true)` → `Repository.GetListAsync()`（移除软删除参数）
   - `Repository.GetAsync(id)` → `Repository.GetByIdAsync(id)`
   - `Logger.LogWarning` → `_logger.LogWarning`（注入的 ILogger 替代 ABP Logger 属性）

6. **移除内容**
   - `[Authorize]` 特性（包括方法级别的 `[Authorize(DFAppPermissions.Aria2.Link)]` 和 `[Authorize(DFAppPermissions.Aria2.Delete)]`）
   - 权限策略名称设置
   - `ITellStatusResultRepository` 自定义仓储引用

---

## 编译问题（预期）

以下编译问题是预期的，将在后续阶段统一解决：

### 1. `required` 成员约束
- `MediaExternalLink` 和 `MediaInfo` 实体有 `required` 属性，不满足 `new()` 约束
- 影响文件：`ExternalLinkService.cs`
- 解决方案：后续修改实体定义或调整仓储泛型约束

### 2. `IKeywordFilterRuleRepository` 接口不兼容
- `IKeywordFilterRuleRepository` 继承自 ABP 的 `IRepository<KeywordFilterRule, long>`
- 新服务中注入此接口时，ABP 的 `IRepository` 可能无法解析
- 解决方案：后续迁移 `IKeywordFilterRuleRepository` 到新架构

### 3. `IBackgroundTaskQueue` 和 `IQueueManagement` 未迁移
- 这些接口仍在 `DFApp.Application.Contracts` 中
- 解决方案：后续迁移到新架构

### 4. `Aria2Consts` 类在 `DFApp.Domain.Shared` 中
- `Aria2Service` 引用了 `Aria2Consts.AddTorrent` 和 `Aria2Consts.AddUri`
- 这些常量在 `DFApp.Domain.Shared` 项目中，需要确保项目引用正确

---

## 未迁移的依赖

| 依赖 | 类型 | 说明 |
|------|------|------|
| `IKeywordFilterRuleRepository` | 自定义仓储 | 继承自 ABP IRepository，需要迁移到新架构 |
| `IBackgroundTaskQueue` | 后台任务队列 | 在 Application.Contracts 中定义，需要迁移 |
| `IQueueManagement` | 队列管理 | 在 Application.Contracts 中定义，需要迁移 |
| `Aria2Request` | 请求类 | 在 Domain 项目中，需要确保引用正确 |
| `Aria2Consts` | 常量类 | 在 Domain.Shared 项目中，需要确保引用正确 |
| `SpaceHelper` | 工具类 | 在 Domain.Shared 项目中，需要确保引用正确 |
| `BencodeNET` | 第三方库 | torrent 文件解析，需要确保 NuGet 包引用 |
| Mapperly 映射器 | 映射 | 所有映射使用 `// TODO: 使用 Mapperly 映射` 伪代码 |
| 权限特性 | 授权 | 所有 `[Authorize]` 已移除，待后续添加 |

---

## 迁移统计

| 指标 | 数量 |
|------|------|
| 迁移服务数 | 3 |
| 涉及实体数 | 6（ElectricVehicleCost, ElectricVehicle, GasolinePrice, ElectricVehicleChargingRecord, MediaExternalLink, TellStatusResult） |
| 导航查询替代数 | 4（Vehicle→VehicleId, Files→ResultId, MediaIds→MediaExternalLinkId, ChargingRecord→VehicleId） |
| 自定义方法迁移数 | 10 |
| 后台任务保留数 | 2（GetExternalLink, RemoveFileAsync） |
