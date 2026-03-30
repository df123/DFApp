# Phase 4.2 Batch 2 迁移总结 - 3 个中等复杂度 CrudAppService

**完成时间**：2026-03-30 | **状态**：已完成

## 概述

本次迁移完成了 Phase 4.2（迁移 CrudAppService）的第二批次，包含 3 个中等复杂度的 CrudAppService。同时完成了 Phase 3.3（替换仓储注入）的工作。

## 迁移服务列表

| # | 服务名 | 原文件 | 新文件 | 主键类型 |
|---|--------|--------|--------|---------|
| 1 | FileUploadInfoService | `src/DFApp.Application/FileUploadDownload/FileUploadInfoService.cs` | `src/DFApp.Web/Services/FileUploadDownload/FileUploadInfoService.cs` | `long` |
| 2 | ElectricVehicleChargingRecordService | `src/DFApp.Application/ElectricVehicle/ElectricVehicleChargingRecordService.cs` | `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleChargingRecordService.cs` | `Guid` |
| 3 | BookkeepingExpenditureService | `src/DFApp.Application/Bookkeeping/Expenditure/BookkeepingExpenditureService.cs` | `src/DFApp.Web/Services/Bookkeeping/BookkeepingExpenditureService.cs` | `long` |

## 各服务详细变更

### 1. FileUploadInfoService

**原文件**: `src/DFApp.Application/FileUploadDownload/FileUploadInfoService.cs`  
**新文件**: `src/DFApp.Web/Services/FileUploadDownload/FileUploadInfoService.cs`

#### 主要变更：
1. **基类变更**：`CrudAppService<FileUploadInfo, FileUploadInfoDto, long, PagedAndSortedResultRequestDto, CreateUpdateFileUploadInfoDto>` → `CrudServiceBase<FileUploadInfo, long, FileUploadInfoDto, CreateUpdateFileUploadInfoDto, CreateUpdateFileUploadInfoDto>`
2. **仓储变更**：`IRepository<FileUploadInfo, long>` → `ISqlSugarRepository<FileUploadInfo, long>`
3. **移除**：
   - `[Authorize(DFAppPermissions.FileUploadDownload.Default)]` 特性
   - 所有权限策略名称设置（GetPolicyName 等）
   - `IDataFilter<ISoftDelete>` 软删除过滤器
   - 软删除恢复逻辑（`IsDeleted = false`）
4. **新增**：
   - `ICurrentUser` 和 `IPermissionChecker` 构造函数参数
   - 三个映射方法（`MapToGetOutputDto`、`MapToEntity`、`MapToEntity` 重载）
5. **业务逻辑变更**：
   - `CreateAsync`：移除软删除过滤器逻辑，改为简单的 SHA1 去重检查。如果已存在相同 SHA1 的文件，直接更新文件信息
   - `DeleteAsync`：保留物理文件删除逻辑
   - `GetConfigurationValue`：改用 `IConfigurationInfoRepository`（新架构）
   - `GetCustomFileTypeDtoAsync`：改用手动映射替代 `ObjectMapper.Map`

#### 特殊处理：
- 使用 `using IConfigurationInfoRepository = DFApp.Web.Data.Configuration.IConfigurationInfoRepository` 解决命名空间歧义

---

### 2. ElectricVehicleChargingRecordService

**原文件**: `src/DFApp.Application/ElectricVehicle/ElectricVehicleChargingRecordService.cs`  
**新文件**: `src/DFApp.Web/Services/ElectricVehicle/ElectricVehicleChargingRecordService.cs`

#### 主要变更：
1. **基类变更**：`CrudAppService<ElectricVehicleChargingRecord, ElectricVehicleChargingRecordDto, Guid, FilterAndPagedAndSortedResultRequestDto, CreateUpdateElectricVehicleChargingRecordDto>` → `CrudServiceBase<ElectricVehicleChargingRecord, Guid, ElectricVehicleChargingRecordDto, CreateUpdateElectricVehicleChargingRecordDto, CreateUpdateElectricVehicleChargingRecordDto>`
2. **仓储变更**：
   - `IRepository<ElectricVehicleChargingRecord, Guid>` → `ISqlSugarRepository<ElectricVehicleChargingRecord, Guid>`
   - `IRepository<ElectricVehicleCost, Guid>` → `ISqlSugarRepository<ElectricVehicleCost, Guid>`
   - `IRepository<ElectricVehicle, Guid>` → `ISqlSugarRepository<ElectricVehicle, Guid>`
3. **移除**：
   - `[Authorize]` 特性和所有权限策略名称设置
   - `IUnitOfWorkManager` 依赖（不再需要手动管理工作单元）
   - `CreateFilteredQueryAsync` 重写方法
   - `AsyncExecuter` 调用
4. **新增**：
   - `ICurrentUser` 和 `IPermissionChecker` 构造函数参数
   - `GetFilteredListAsync(string? filter, int pageIndex, int pageSize)` — 替代原 `GetListAsync` 的分页查询
   - `MapVehicleToDto` 私有方法 — 车辆实体到 DTO 的映射
   - 三个基类映射方法
5. **业务逻辑变更**：
   - `GetListAsync` → `GetFilteredListAsync`：移除 `AsyncExecuter`，使用 `Repository.GetQueryable()` + SqlSugar 分页
   - `CreateAsync`：移除 `base.CreateAsync` 调用，直接使用 `Repository.InsertAsync`；移除 `IUnitOfWorkManager.Begin()` 工作单元
   - `UpdateAsync`：同上，移除工作单元管理
   - `GetAsync`：使用 `Repository.GetByIdAsync` 替代 `Repository.GetAsync`
   - `DeleteAsync`：保留删除关联成本记录逻辑
   - `CreateOrUpdateCostRecordAsync`：移除 `IUnitOfWorkManager.Begin(requiresNew: true)` 工作单元，直接操作仓储
   - `DeleteRelatedCostRecordAsync`：同上
   - `UpdateVehicleTotalMileageAsync`：同上
   - 导航属性查询改为外键查询：通过 `_vehicleRepository.GetByIdAsync()` 获取车辆信息

#### 特殊处理：
- 使用 `using ElectricVehicleEntity = DFApp.ElectricVehicle.ElectricVehicle` 解决命名空间与类名冲突
- 原始代码中 `ElectricVehicle` 既是命名空间又是类名，在新架构中需要类型别名

---

### 3. BookkeepingExpenditureService

**原文件**: `src/DFApp.Application/Bookkeeping/Expenditure/BookkeepingExpenditureService.cs`  
**新文件**: `src/DFApp.Web/Services/Bookkeeping/BookkeepingExpenditureService.cs`

#### 主要变更：
1. **基类变更**：`CrudAppService<BookkeepingExpenditure, BookkeepingExpenditureDto, long, GetExpendituresRequestDto, CreateUpdateBookkeepingExpenditureDto>` → `CrudServiceBase<BookkeepingExpenditure, long, BookkeepingExpenditureDto, CreateUpdateBookkeepingExpenditureDto, CreateUpdateBookkeepingExpenditureDto>`
2. **仓储变更**：
   - `IRepository<BookkeepingExpenditure, long>` → `ISqlSugarRepository<BookkeepingExpenditure, long>`
   - `IRepository<BookkeepingCategory, long>` → `ISqlSugarRepository<BookkeepingCategory, long>`
3. **移除**：
   - `[Authorize]` 特性和所有权限策略名称设置
   - `CreateFilteredQueryAsync` 重写方法
   - `Repository.WithDetailsAsync()` 导航查询
   - `AsyncExecuter` 调用
   - `ReadOnlyRepository` 引用
4. **新增**：
   - `ICurrentUser` 和 `IPermissionChecker` 构造函数参数
   - `GetFilteredListAsync(string? filter, long? categoryId, bool? isBelongToSelf, int pageIndex, int pageSize)` — 替代原 `CreateFilteredQueryAsync` + `GetListAsync`
   - `MapCategoryToDto` 私有方法 — 分类实体到 DTO 的映射
   - 三个基类映射方法
5. **业务逻辑变更**：
   - **导航查询替代**：原始代码使用 `Repository.WithDetailsAsync()` 加载 `Category` 导航属性，现改为通过 `_categoryRepository` 外键查询
   - **过滤查询变更**：原始代码通过 `x.Category.Category.Contains(filter)` 导航属性过滤，现改为先查询匹配的分类 ID 列表，再用 `matchingCategoryIds.Contains(x.CategoryId)` 过滤
   - `GetTotalExpenditureAsync`：`AsyncExecuter.SumAsync()` → `query.Sum()`（SqlSugar 的 ISugarQueryable）
   - `GetChartJSDto`：`ReadOnlyRepository.GetListAsync(expression, true)` → `Repository.GetListAsync(expression)`
   - `PopulateChartJSDatasetsItemDto`：`temp.Category.Category`（导航属性）→ 通过 `categoryNameMap.TryGetValue(item.Key, ...)` 外键查询
   - `BuildExpression`：保留 `expression.And()` 链式调用
   - `GetMonthlyExpenditureAsync`：`Repository.GetListAsync()` → `Repository.GetListAsync(expression)`
   - `ManipulateDate`：`switch/case` → `switch` 表达式

#### 特殊处理：
- `PopulateChartJSDatasetsItemDto` 方法签名从同步改为 `async Task`，因为需要异步查询分类信息
- 原始代码中 `GetChartJSDto` 方法有 `[Authorize(DFAppPermissions.BookkeepingExpenditure.Analysis)]` 特性，已移除

---

## 通用迁移模式

| 变更项 | 原模式 | 新模式 |
|--------|--------|--------|
| 基类 | `CrudAppService<...>` | `CrudServiceBase<...>` |
| 仓储 | `IRepository<T, TKey>` | `ISqlSugarRepository<T, TKey>` |
| 权限 | `[Authorize]` + PolicyName | 移除（待后续添加） |
| 映射 | `ObjectMapper.Map<...>` | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| 构造函数 | `IRepository` | `ICurrentUser` + `IPermissionChecker` + `ISqlSugarRepository` |
| 工作单元 | `IUnitOfWorkManager.Begin(requiresNew: true)` | 移除（SqlSugar 自带事务管理） |
| 导航查询 | `.WithDetailsAsync()` / `.Include()` | 外键查询（先获取 ID 列表，再批量查询） |
| 异步执行 | `AsyncExecuter.ToListAsync()` | `.ToListAsync()` / `.ToPageListAsync()` |
| 软删除 | `IDataFilter<ISoftDelete>` | 移除 |

## 编译问题（预期）

以下编译问题是预期的，将在后续阶段统一解决：

1. **`IEntity<TKey>` 接口不匹配**：实体继承 `AuditedEntity<TKey>` 而非直接实现 `IEntity<TKey>` 接口
2. **`required` 成员约束**：部分实体有 `required` 属性，不满足 `new()` 约束
3. **DTO 类引用 ABP 基类**：`FileUploadInfoDto` 和 `BookkeepingExpenditureDto` 继承 `AuditedEntityDto<long>`（ABP 类）
4. **`expression.And()` 扩展方法**：`BookkeepingExpenditureService.BuildExpression` 中使用了 `And()` 扩展方法，可能需要引入对应的命名空间

## 未迁移的依赖

1. **Mapperly 映射器**：所有映射使用 `// TODO: 使用 Mapperly 映射` 伪代码，待 Phase 4.4 统一处理
2. **权限特性**：所有 `[Authorize]` 已移除，待 Phase 6 统一添加权限控制
3. **Controller**：尚未创建对应的 API Controller，待 Phase 5 处理
4. **DTO 类**：仍在 `src/DFApp.Application.Contracts/` 中，部分引用了 ABP 的 `AuditedEntityDto` 等基类
5. **`IConfigurationInfoRepository`**：`FileUploadInfoService` 依赖的配置信息仓储接口（已迁移）
6. **`ElectricVehicleCost` 实体**：`ElectricVehicleChargingRecordService` 依赖的成本记录实体（已迁移到 Domain）
7. **`CostType` 枚举**：`ElectricVehicleChargingRecordService` 使用的成本类型枚举（在 Domain.Shared 中）

## 下一步

继续迁移剩余的 CrudAppService：
- ElectricVehicleCostService
- GasolinePriceService
- KeywordFilterRuleService
- 其他复杂服务
