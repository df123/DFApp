# Phase 4.4 迁移总结：DTO 映射迁移（Mapperly）

**完成时间**：2026-04-01 | **状态**：已完成（部分命名空间冲突待解决） | **涉及模块数**：11

---

## 1. 概述

### 1.1 迁移目标

Phase 4.4 的核心目标是完成 DTO 映射层的迁移，具体包括：

1. **创建 DTO 基类**：替代 ABP 框架的 `EntityDto<TKey>`、`AuditedEntityDto<TKey>` 等基类
2. **迁移 DTO 文件**：将所有 DTO 从 `src/DFApp.Application.Contracts/` 迁移到 `src/DFApp.Web/DTOs/`
3. **创建 Mapperly 映射器**：使用 Mapperly 源码生成器替代 ABP 的 `MapperBase`/`TwoWayMapperBase` 封装
4. **集成映射器到服务层**：将 Mapperly 映射器集成到所有应用服务中

### 1.2 总体统计

| 指标 | 数量 |
|------|------|
| DTO 基类文件 | 4 |
| DTO 文件（按模块） | ~80 |
| Mapperly 映射器文件 | 11 |
| 成功集成映射器的服务 | 18 |
| 命名空间冲突保留手动映射的服务 | 7 |
| 无需修改的服务 | 5 |

---

## 2. DTO 基类迁移

为替代 ABP 框架提供的 DTO 基类，在 `src/DFApp.Web/DTOs/` 下创建了以下自定义基类：

| 基类名称 | 文件路径 | 说明 |
|----------|---------|------|
| `EntityDto<TKey>` | `src/DFApp.Web/DTOs/EntityDto.cs` | 通用实体 DTO 基类，包含 `Id` 属性 |
| `AuditedEntityDto<TKey>` | `src/DFApp.Web/DTOs/AuditedEntityDto.cs` | 审计实体 DTO 基类，继承 `EntityDto`，增加审计字段 |
| `CreationAuditedEntityDto<TKey>` | `src/DFApp.Web/DTOs/CreationAuditedEntityDto.cs` | 创建审计 DTO 基类，继承 `EntityDto`，增加创建审计字段 |
| `PagedAndSortedResultRequestDto` | `src/DFApp.Web/DTOs/PagedAndSortedResultRequestDto.cs` | 分页排序请求 DTO 基类 |

---

## 3. DTO 文件迁移

所有 DTO 文件从 `src/DFApp.Application.Contracts/` 迁移到 `src/DFApp.Web/DTOs/`，按模块组织：

### 3.1 按模块统计

| 模块 | DTO 文件数 | 文件列表 |
|------|-----------|---------|
| **Configuration** | 2 | `ConfigurationInfoDto`, `CreateUpdateConfigurationInfoDto` |
| **IP** | 2 | `DynamicIPDto`, `CreateUpdateDynamicIPDto` |
| **FileUploadDownload** | 3 | `FileUploadInfoDto`, `CreateUpdateFileUploadInfoDto`, `CustomFileTypeDto` |
| **FileFilter** | 2 | `KeywordFilterRuleDto`, `CreateUpdateKeywordFilterRuleDto` |
| **Common** | 1 | `FilterAndPagedAndSortedResultRequestDto` |
| **Bookkeeping** | 6 | `BookkeepingCategoryDto`, `CreateUpdateBookkeepingCategoryDto`, `BookkeepingCategoryLookupDto`, `BookkeepingExpenditureDto`, `CreateUpdateBookkeepingExpenditureDto`, `GetExpendituresRequestDto` |
| **ElectricVehicle** | 9 | `ElectricVehicleDto`, `ElectricVehicleCostDto`, `ElectricVehicleChargingRecordDto`, `GasolinePriceDto`, `OilCostComparisonDto`, `GetGasolinePricesDto` 及对应的 `CreateUpdate*` DTO |
| **Lottery** | ~20 | `LotteryDto`, `LotteryResultDto`, `LotteryPrizegradesDto`, `LotteryGroupDto`, `ResultItemDto`, `CompoundLotteryResultDto`, `PrizegradesItemDto`, `Consts/ConstsDto`, Statistics（`LotteryStructure`, `StatisticsInputDto`, `StatisticsWinDto`, `StatisticsWinItemDto`, `StatisticsWinItemRequestDto`）, Simulation SSQ（`LotterySimulationDto`, `CreateUpdateLotterySimulationDto`）, Simulation KL8（`LotterySimulationDto`, `CreateUpdateLotterySimulationDto`） |
| **Media** | 3 | `MediaInfoDto`, `ExternalLinkDto`, `CreateUpdateExternalLinkDto` |
| **Aria2** | 12 | `ResponseBaseDto`, `AddDownloadDto`, `Aria2ManageDto`, `IpGeolocationDto`, `Aria2NotificationDto`, `ParamsItemDto`, `Aria2RequestDto`, `Aria2ResponseDto`, `FilesItemDto`, `TellStatusResponseDto`, `TellStatusResultDto`, `UrisItemDto` |
| **Account** | 9 | `UserDto`, `LoginDto`, `LoginResultDto`, `CreateUserDto`, `UpdateUserDto`, `ChangePasswordDto`, `ResetPasswordDto`, `SendPasswordResetCodeDto`, `VerifyPasswordResetTokenDto` |
| **Rss** | 12+ | `RssSourceDto`, `CreateUpdateRssSourceDto`, `RssSubscriptionDto`, `CreateUpdateRssSubscriptionDto`, `RssSubscriptionDownloadDto`, `RssMirrorItemDto`, `RssWordSegmentDto`, `RssWordSegmentWithItemDto`, `RssFetchRequestDto`, `RssItemDto`, `RssFetchResponseDto`, `WordSegmentStatisticsDto`, `GetRssMirrorItemsRequestDto`, `GetRssSubscriptionsRequestDto`, `GetRssSubscriptionDownloadsRequestDto`, `GetRssWordSegmentsRequestDto` |

### 3.2 DTO 命名空间变更

| 原命名空间 | 新命名空间 |
|-----------|-----------|
| `DFApp.Configuration` | `DFApp.Web.DTOs.Configuration` |
| `DFApp.IP` | `DFApp.Web.DTOs.IP` |
| `DFApp.FileUploadDownload` | `DFApp.Web.DTOs.FileUploadDownload` |
| `DFApp.FileFilter` | `DFApp.Web.DTOs.FileFilter` |
| `DFApp.Bookkeeping` | `DFApp.Web.DTOs.Bookkeeping` |
| `DFApp.ElectricVehicle` | `DFApp.Web.DTOs.ElectricVehicle` |
| `DFApp.Lottery` | `DFApp.Web.DTOs.Lottery` |
| `DFApp.Media` | `DFApp.Web.DTOs.Media` |
| `DFApp.Aria2` | `DFApp.Web.DTOs.Aria2` |
| `DFApp.Account` | `DFApp.Web.DTOs.Account` |
| `DFApp.Rss` | `DFApp.Web.DTOs.Rss` |

---

## 4. Mapperly 映射器创建

共创建 **11 个** Mapperly 映射器文件，位于 `src/DFApp.Web/Mapping/` 目录下。

### 4.1 映射器总览

| 映射器 | 文件路径 | 方法数 | 说明 |
|--------|---------|--------|------|
| ConfigurationMapper | `Mapping/ConfigurationMapper.cs` | 3 | 配置信息映射 |
| IPMapper | `Mapping/IPMapper.cs` | 2 | 动态 IP 映射 |
| FileUploadDownloadMapper | `Mapping/FileUploadDownloadMapper.cs` | 2 | 文件上传下载映射 |
| FileFilterMapper | `Mapping/FileFilterMapper.cs` | 2 | 文件过滤映射 |
| BookkeepingMapper | `Mapping/BookkeepingMapper.cs` | 7 | 记账模块映射 |
| ElectricVehicleMapper | `Mapping/ElectricVehicleMapper.cs` | 7 | 电动车模块映射 |
| LotteryMapper | `Mapping/LotteryMapper.cs` | 27 | 彩票模块映射（含旧命名空间兼容） |
| MediaMapper | `Mapping/MediaMapper.cs` | 3 | 媒体信息映射 |
| Aria2Mapper | `Mapping/Aria2Mapper.cs` | 16 | Aria2 模块映射 |
| AccountMapper | `Mapping/AccountMapper.cs` | 3 | 账户模块映射 |
| RssMapper | `Mapping/RssMapper.cs` | 8 | RSS 模块映射 |

### 4.2 各映射器方法详情

#### ConfigurationMapper（3 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `ConfigurationInfo` | `ConfigurationInfoDto` | 实体 → DTO |
| `MapToEntity` | `CreateUpdateConfigurationInfoDto` | `ConfigurationInfo` | DTO → 实体（忽略 ConcurrencyStamp） |
| `MapToCustomFileTypeDto` | `ConfigurationInfo` | `CustomFileTypeDto` | 实体 → 自定义文件类型 DTO |

#### IPMapper（2 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `DynamicIP` | `DynamicIPDto` | 实体 → DTO |
| `MapToEntity` | `CreateUpdateDynamicIPDto` | `DynamicIP` | DTO → 实体（忽略 ConcurrencyStamp） |

#### FileUploadDownloadMapper（2 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `FileUploadInfo` | `FileUploadInfoDto` | 实体 → DTO |
| `MapToEntity` | `CreateUpdateFileUploadInfoDto` | `FileUploadInfo` | DTO → 实体（忽略 ConcurrencyStamp） |

#### FileFilterMapper（2 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `KeywordFilterRule` | `KeywordFilterRuleDto` | 实体 → DTO |
| `MapToEntity` | `CreateUpdateKeywordFilterRuleDto` | `KeywordFilterRule` | DTO → 实体（忽略 ConcurrencyStamp） |

#### BookkeepingMapper（7 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `BookkeepingCategory` | `BookkeepingCategoryDto` | 分类实体 → DTO |
| `MapToEntity` | `BookkeepingCategoryDto` | `BookkeepingCategory` | DTO → 分类实体（双向映射） |
| `MapToEntity` | `CreateUpdateBookkeepingCategoryDto` | `BookkeepingCategory` | 创建/更新 DTO → 分类实体 |
| `MapToLookupDto` | `BookkeepingCategory` | `BookkeepingCategoryLookupDto` | 分类 → Lookup DTO（Id → CategoryId） |
| `MapToExpenditureDto` | `BookkeepingExpenditure` | `BookkeepingExpenditureDto` | 支出实体 → DTO |
| `MapToEntity` | `BookkeepingExpenditureDto` | `BookkeepingExpenditure` | DTO → 支出实体（双向映射） |
| `MapToEntity` | `CreateUpdateBookkeepingExpenditureDto` | `BookkeepingExpenditure` | 创建/更新 DTO → 支出实体 |

#### ElectricVehicleMapper（7 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `ElectricVehicle` | `ElectricVehicleDto` | 车辆实体 → DTO（忽略 Costs 导航属性） |
| `MapToEntity` | `CreateUpdateElectricVehicleDto` | `ElectricVehicle` | DTO → 车辆实体 |
| `MapToCostDto` | `ElectricVehicleCost` | `ElectricVehicleCostDto` | 费用实体 → DTO（忽略 Vehicle 导航属性） |
| `MapToEntity` | `CreateUpdateElectricVehicleCostDto` | `ElectricVehicleCost` | DTO → 费用实体 |
| `MapToChargingDto` | `ElectricVehicleChargingRecord` | `ElectricVehicleChargingRecordDto` | 充电记录 → DTO（忽略 Vehicle 导航属性） |
| `MapToEntity` | `CreateUpdateElectricVehicleChargingRecordDto` | `ElectricVehicleChargingRecord` | DTO → 充电记录实体 |
| `MapToDto` | `GasolinePrice` | `GasolinePriceDto` | 油价实体 → DTO |

#### LotteryMapper（27 个方法）

LotteryMapper 是最复杂的映射器，包含新命名空间和旧命名空间两套映射方法：

**LotteryInfo 映射（4 个方法）：**

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `LotteryInfo` | `LotteryDto` | 实体 → DTO |
| `MapToEntity` | `CreateUpdateLotteryDto` | `LotteryInfo` | DTO → 实体 |
| `MapToCreateUpdateDto` | `LotteryDto` | `CreateUpdateLotteryDto` | DTO → 创建/更新 DTO |
| `MapToEntity` (重载) | `CreateUpdateLotteryDto, LotteryInfo` | `void` | 更新已有实体 |

**LotteryResult 映射（4 个方法）：**

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `LotteryResult` | `LotteryResultDto` | 实体 → DTO（手动处理 Prizegrades 集合） |
| `MapToEntity` | `CreateUpdateLotteryResultDto` | `LotteryResult` | DTO → 实体 |
| `MapToEntity` (重载) | `CreateUpdateLotteryResultDto, LotteryResult` | `void` | 更新已有实体 |
| `MapToCreateUpdateDto` | `LotteryResultDto` | `CreateUpdateLotteryResultDto` | DTO → 创建/更新 DTO |

**LotteryPrizegrades 映射（3 个方法）：**

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `LotteryPrizegrades` | `LotteryPrizegradesDto` | 实体 → DTO |
| `MapToEntity` | `CreateUpdateLotteryPrizegradesDto` | `LotteryPrizegrades` | DTO → 实体 |
| `MapToCreateUpdateDto` | `LotteryPrizegradesDto` | `CreateUpdateLotteryPrizegradesDto` | DTO → 创建/更新 DTO |

**外部数据中间 DTO 映射（4 个方法）：**

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapResultItemToCreateUpdateDto` | `ResultItemDto` | `CreateUpdateLotteryResultDto` | 外部数据 → 内部 DTO |
| `MapPrizegradesItemToCreateUpdateDto` | `PrizegradesItemDto` | `CreateUpdateLotteryPrizegradesDto` | 外部数据 → 内部 DTO |
| `MapToEntityFromResultItem` | `ResultItemDto` | `LotteryResult` | 外部数据 → 实体 |
| `MapToEntityFromPrizegradesItem` | `PrizegradesItemDto` | `LotteryPrizegrades` | 外部数据 → 实体 |

**Simulation SSQ 映射（3 个方法）：**

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToSSQDto` | `LotterySimulation` | `SSQ LotterySimulationDto` | 实体 → SSQ DTO |
| `MapToEntityFromSSQ` | `SSQ CreateUpdateLotterySimulationDto` | `LotterySimulation` | SSQ DTO → 实体 |
| `MapToEntityFromSSQ` (重载) | `SSQ CreateUpdateLotterySimulationDto, LotterySimulation` | `void` | 更新已有实体 |

**Simulation KL8 映射（3 个方法）：**

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToKL8Dto` | `LotterySimulation` | `KL8 LotterySimulationDto` | 实体 → KL8 DTO |
| `MapToEntityFromKL8` | `KL8 CreateUpdateLotterySimulationDto` | `LotterySimulation` | KL8 DTO → 实体 |
| `MapToEntityFromKL8` (重载) | `KL8 CreateUpdateLotterySimulationDto, LotterySimulation` | `void` | 更新已有实体 |

**旧命名空间兼容映射（6 个方法）：**

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToExternalLotteryDto` | `LotteryInfo` | `DFApp.Lottery.LotteryDto` | 实体 → 旧命名空间 DTO |
| `MapToExternalSSQDto` | `LotterySimulation` | `DFApp.Lottery.Simulation.SSQ.LotterySimulationDto` | 实体 → 旧命名空间 SSQ DTO |
| `MapToEntityFromExternalSSQ` | `DFApp.Lottery.Simulation.SSQ.CreateUpdateLotterySimulationDto` | `LotterySimulation` | 旧命名空间 SSQ DTO → 实体 |
| `MapToEntityFromExternalSSQ` (重载) | 旧 SSQ DTO, `LotterySimulation` | `void` | 更新已有实体 |
| `MapToExternalKL8Dto` | `LotterySimulation` | `DFApp.Lottery.Simulation.KL8.LotterySimulationDto` | 实体 → 旧命名空间 KL8 DTO |
| `MapToEntityFromExternalKL8` | `DFApp.Lottery.Simulation.KL8.CreateUpdateLotterySimulationDto` | `LotterySimulation` | 旧命名空间 KL8 DTO → 实体 |
| `MapToEntityFromExternalKL8` (重载) | 旧 KL8 DTO, `LotterySimulation` | `void` | 更新已有实体 |

#### MediaMapper（3 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `MediaInfo` | `MediaInfoDto` | 实体 → DTO（MediaId long→string 转换） |
| `MapToDto` | `MediaExternalLink` | `ExternalLinkDto` | 外链实体 → DTO |
| `MapToEntity` | `CreateUpdateExternalLinkDto` | `MediaExternalLink` | DTO → 实体（忽略审计字段） |

#### Aria2Mapper（16 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `TellStatusResult` | `TellStatusResultDto` | 实体 → DTO |
| `MapToEntity` | `TellStatusResultDto` | `TellStatusResult` | DTO → 实体 |
| `MapToDto` | `FilesItem` | `FilesItemDto` | 实体 → DTO |
| `MapToEntity` | `FilesItemDto` | `FilesItem` | DTO → 实体（忽略导航属性） |
| `MapToDto` | `UrisItem` | `UrisItemDto` | 实体 → DTO |
| `MapToEntity` | `UrisItemDto` | `UrisItem` | DTO → 实体（忽略导航属性） |
| `MapToDto` | `Aria2Notification` | `Aria2NotificationDto` | 实体 → DTO |
| `MapToEntity` | `Aria2NotificationDto` | `Aria2Notification` | DTO → 实体 |
| `MapToDto` | `ParamsItem` | `ParamsItemDto` | 实体 → DTO |
| `MapToEntity` | `ParamsItemDto` | `ParamsItem` | DTO → 实体 |
| `MapToDto` | `Aria2Request` | `Aria2RequestDto` | 实体 → DTO |
| `MapToDto` | `Aria2Response` | `Aria2ResponseDto` | 实体 → DTO |
| `MapToDto` | `ResponseBase` | `ResponseBaseDto` | 实体 → DTO |
| `MapToEntity` | `ResponseBaseDto` | `ResponseBase` | DTO → 实体 |
| `MapToDto` | `TellStatusResponse` | `TellStatusResponseDto` | 实体 → DTO |
| `MapToEntity` | `TellStatusResponseDto` | `TellStatusResponse` | DTO → 实体 |

#### AccountMapper（3 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `User` | `UserDto` | 实体 → DTO |
| `MapToEntity` | `CreateUserDto` | `User` | 创建 DTO → 实体（忽略密码哈希和审计字段） |
| `MapToEntity` | `UpdateUserDto` | `User` | 更新 DTO → 实体（忽略密码哈希和审计字段） |

#### RssMapper（8 个方法）

| 方法 | 源类型 | 目标类型 | 说明 |
|------|--------|---------|------|
| `MapToDto` | `RssSource` | `RssSourceDto` | RSS 源实体 → DTO |
| `MapToEntity` | `CreateUpdateRssSourceDto` | `RssSource` | DTO → RSS 源实体（忽略审计字段） |
| `MapToDto` | `RssSubscription` | `RssSubscriptionDto` | RSS 订阅实体 → DTO（忽略 RssSourceName） |
| `MapToEntity` | `CreateUpdateRssSubscriptionDto` | `RssSubscription` | DTO → RSS 订阅实体（忽略审计字段） |
| `MapToDto` | `RssMirrorItem` | `RssMirrorItemDto` | RSS 镜像条目 → DTO（忽略 WordSegments、RssSourceName） |
| `MapToDto` | `RssSubscriptionDownload` | `RssSubscriptionDownloadDto` | RSS 下载 → DTO（忽略显示字段） |
| `MapToDto` | `RssWordSegment` | `RssWordSegmentDto` | RSS 分词 → DTO |
| `MapToWithItemDto` | `RssWordSegment` | `RssWordSegmentWithItemDto` | RSS 分词 → 带条目信息 DTO（忽略关联字段） |

---

## 5. 服务层映射器集成

### 5.1 ✅ 成功集成 Mapperly 的服务（18 个）

以下服务已成功将手动映射替换为 Mapperly 映射器调用：

| 服务 | 文件路径 | 使用的映射器 |
|------|---------|-------------|
| ConfigurationInfoService | `Services/Configuration/ConfigurationInfoService.cs` | ConfigurationMapper |
| DynamicIPService | `Services/IP/DynamicIPService.cs` | IPMapper |
| FileUploadInfoService | `Services/FileUploadDownload/FileUploadInfoService.cs` | FileUploadDownloadMapper |
| KeywordFilterRuleService | `Services/FileFilter/KeywordFilterRuleService.cs` | FileFilterMapper |
| MediaInfoService | `Services/Media/MediaInfoService.cs` | MediaMapper |
| ExternalLinkService | `Services/Media/ExternalLinkService.cs` | MediaMapper |
| BookkeepingCategoryService | `Services/Bookkeeping/BookkeepingCategoryService.cs` | BookkeepingMapper |
| BookkeepingExpenditureService | `Services/Bookkeeping/BookkeepingExpenditureService.cs` | BookkeepingMapper |
| ElectricVehicleService | `Services/ElectricVehicle/ElectricVehicleService.cs` | ElectricVehicleMapper |
| ElectricVehicleCostService | `Services/ElectricVehicle/ElectricVehicleCostService.cs` | ElectricVehicleMapper |
| ElectricVehicleChargingRecordService | `Services/ElectricVehicle/ElectricVehicleChargingRecordService.cs` | ElectricVehicleMapper |
| GasolinePriceService | `Services/ElectricVehicle/GasolinePriceService.cs` | ElectricVehicleMapper |
| LotteryResultService | `Services/Lottery/LotteryResultService.cs` | LotteryMapper |
| LotteryService | `Services/Lottery/LotteryService.cs` | LotteryMapper |
| LotteryDataFetchService | `Services/Lottery/LotteryDataFetchService.cs` | LotteryMapper |
| CompoundLotteryService | `Services/Lottery/CompoundLotteryService.cs` | LotteryMapper |
| LotteryKL8SimulationService | `Services/Lottery/Simulation/LotteryKL8SimulationService.cs` | LotteryMapper |
| LotterySSQSimulationService | `Services/Lottery/Simulation/LotterySSQSimulationService.cs` | LotteryMapper |

### 5.2 ⚠️ 命名空间冲突，保留手动映射的服务（7 个）

以下服务因新旧 DTO 命名空间冲突，暂时保留手动映射：

| 服务 | 文件路径 | 冲突类型 |
|------|---------|---------|
| Aria2Service | `Services/Aria2/Aria2Service.cs` | `DFApp.Aria2.Response.TellStatus.TellStatusResultDto` vs `DFApp.Web.DTOs.Aria2.TellStatusResultDto` |
| UserManagementAppService | `Services/Account/UserManagementAppService.cs` | `DFApp.Account.UserDto` vs `DFApp.Web.DTOs.Account.UserDto` |
| RssSourceAppService | `Services/Rss/RssSourceAppService.cs` | `DFApp.Rss.RssSourceDto` vs `DFApp.Web.DTOs.Rss.RssSourceDto` |
| RssSubscriptionAppService | `Services/Rss/RssSubscriptionAppService.cs` | `DFApp.Rss.RssSubscriptionDto` vs `DFApp.Web.DTOs.Rss.RssSubscriptionDto` |
| RssMirrorItemAppService | `Services/Rss/RssMirrorItemAppService.cs` | `DFApp.Rss.RssMirrorItemDto` vs `DFApp.Web.DTOs.Rss.RssMirrorItemDto` |
| RssSubscriptionDownloadAppService | `Services/Rss/RssSubscriptionDownloadAppService.cs` | `DFApp.Rss.RssSubscriptionDownloadDto` vs `DFApp.Web.DTOs.Rss.RssSubscriptionDownloadDto` |
| RssWordSegmentAppService | `Services/Rss/RssWordSegmentAppService.cs` | `DFApp.Rss.RssWordSegmentWithItemDto` vs `DFApp.Web.DTOs.Rss.RssWordSegmentWithItemDto` |

### 5.3 🔘 无需修改的服务（5 个）

以下服务不涉及实体↔DTO 映射，无需修改：

| 服务 | 文件路径 | 说明 |
|------|---------|------|
| Aria2ManageService | `Services/Aria2/Aria2ManageService.cs` | 无 TODO 标记，无手动映射 |
| AccountAppService | `Services/Account/AccountAppService.cs` | 无 TODO 标记，无手动映射 |
| RssFetchService | `Services/Rss/RssFetchService.cs` | 无 TODO 标记，不涉及实体↔DTO 映射 |
| RssSubscriptionService | `Services/Rss/RssSubscriptionService.cs` | 无 TODO 标记，不涉及实体↔DTO 映射 |
| WordSegmentService | `Services/Rss/WordSegmentService.cs` | 纯文本处理服务，不涉及映射 |

---

## 6. 关键问题：命名空间冲突

### 6.1 问题描述

在 Phase 4.4 迁移过程中发现了一个关键的命名空间冲突问题，导致 7 个服务无法完全集成 Mapperly 映射器。

**冲突根源：**

- **新 DTO** 位于 `DFApp.Web.DTOs.*` 命名空间（Phase 4.4 新创建）
- **旧 DTO** 位于 `DFApp.*` 命名空间（来自 `DFApp.Application.Contracts` 项目）
- 服务层通过 `using` 导入旧 DTO 类型作为方法签名参数和返回值
- Mapperly 映射器返回新 DTO 类型，无法直接赋值给方法签名中的旧 DTO 类型

**示例：**

```csharp
// 服务方法签名使用旧 DTO 类型
public async Task<DFApp.Rss.RssSourceDto> GetAsync(long id)

// Mapperly 映射器返回新 DTO 类型
var mapper = new RssMapper();
return mapper.MapToDto(entity);  // 返回 DFApp.Web.DTOs.Rss.RssSourceDto，类型不匹配
```

### 6.2 约束条件

根据项目迁移约束，不允许修改 `DFApp.Application.Contracts` 中的旧 DTO 文件。

### 6.3 解决方案建议

| 方案 | 描述 | 优点 | 缺点 |
|------|------|------|------|
| **方案 A** | 将服务层的 `using` 从旧 DTO 命名空间切换到新 DTO 命名空间 | 最干净的解决方案，彻底消除旧依赖 | 需要修改服务方法签名，可能影响 Controller 层 |
| **方案 B** | 删除 `DFApp.Application.Contracts` 中的旧 DTO 文件 | 从根本上解决冲突 | 需要确保所有引用已迁移，风险较高 |
| **方案 C** | 在 Mapperly 映射器中添加旧 DTO → 新 DTO 的转换方法 | 不需要修改服务层代码 | 增加不必要的映射层，性能开销 |

**推荐方案**：方案 A，在 Phase 5 创建 Controller 层时一并处理，将服务层的方法签名统一切换到新 DTO 命名空间。

---

## 7. Mapperly 使用模式

### 7.1 基本模式

所有 Mapperly 映射器遵循以下统一模式：

```csharp
[Mapper]
public partial class XxxMapper
{
    // 实体 → DTO
    public partial XxxDto MapToDto(XxxEntity entity);

    // 创建/更新 DTO → 实体
    [MapperIgnoreTarget(nameof(XxxEntity.ConcurrencyStamp))]
    public partial XxxEntity MapToEntity(CreateUpdateXxxDto dto);
}
```

### 7.2 使用方式

Mapperly 映射器是无状态的，直接 `new()` 创建实例使用：

```csharp
var mapper = new XxxMapper();
var dto = mapper.MapToDto(entity);
```

### 7.3 关键特性

| 特性 | 用途 | 示例 |
|------|------|------|
| `[Mapper]` | 标记映射器类，触发源码生成 | `[Mapper] public partial class XxxMapper` |
| `partial` 方法 | 由源码生成器自动实现 | `public partial XxxDto MapToDto(Xxx entity);` |
| `[MapperIgnoreTarget]` | 忽略目标类型的指定字段 | `[MapperIgnoreTarget(nameof(Xxx.ConcurrencyStamp))]` |
| `[MapperIgnoreSource]` | 忽略源类型的指定字段 | `[MapperIgnoreSource(nameof(Xxx.NavigationProp))]` |
| `[MapProperty]` | 自定义属性映射规则 | `[MapProperty(nameof(Src.Id), nameof(Dst.CategoryId))]` |
| 手动实现方法 | 处理复杂集合映射等场景 | 手动遍历集合并调用子映射方法 |

### 7.4 特殊处理

- **集合映射**：对于包含子对象集合的映射（如 `LotteryResult.Prizegrades`），采用手动实现方法遍历集合并调用子映射方法
- **类型转换**：对于需要特殊类型转换的字段（如 `long` → `string`），使用私有辅助方法
- **导航属性忽略**：使用 `[MapperIgnoreSource]` 忽略 EF Core 导航属性，避免循环引用

---

## 8. 文件结构

### 8.1 DTO 目录结构

```
src/DFApp.Web/DTOs/
├── EntityDto.cs
├── AuditedEntityDto.cs
├── CreationAuditedEntityDto.cs
├── PagedAndSortedResultRequestDto.cs
├── Account/
│   ├── UserDto.cs
│   ├── LoginDto.cs
│   ├── LoginResultDto.cs
│   ├── CreateUserDto.cs
│   ├── UpdateUserDto.cs
│   ├── ChangePasswordDto.cs
│   ├── ResetPasswordDto.cs
│   ├── SendPasswordResetCodeDto.cs
│   └── VerifyPasswordResetTokenDto.cs
├── Aria2/
│   ├── ResponseBaseDto.cs
│   ├── AddDownloadDto.cs
│   ├── Aria2ManageDto.cs
│   ├── IpGeolocationDto.cs
│   ├── Aria2NotificationDto.cs
│   ├── ParamsItemDto.cs
│   ├── Aria2RequestDto.cs
│   ├── Aria2ResponseDto.cs
│   ├── FilesItemDto.cs
│   ├── TellStatusResponseDto.cs
│   ├── TellStatusResultDto.cs
│   ├── UrisItemDto.cs
│   ├── Notifications/
│   ├── Request/
│   └── Response/
│       └── TellStatus/
├── Bookkeeping/
│   ├── BookkeepingCategoryDto.cs
│   ├── BookkeepingCategoryLookupDto.cs
│   ├── CreateUpdateBookkeepingCategoryDto.cs
│   ├── BookkeepingExpenditureDto.cs
│   ├── CreateUpdateBookkeepingExpenditureDto.cs
│   ├── GetExpendituresRequestDto.cs
│   ├── Category/
│   ├── Expenditure/
│   │   └── Lookup/
├── Common/
│   └── FilterAndPagedAndSortedResultRequestDto.cs
├── Configuration/
│   ├── ConfigurationInfoDto.cs
│   └── CreateUpdateConfigurationInfoDto.cs
├── ElectricVehicle/
│   ├── ElectricVehicleDto.cs
│   ├── ElectricVehicleCostDto.cs
│   ├── ElectricVehicleChargingRecordDto.cs
│   ├── GasolinePriceDto.cs
│   ├── OilCostComparisonDto.cs
│   └── GetGasolinePricesDto.cs
├── FileFilter/
│   ├── KeywordFilterRuleDto.cs
│   └── CreateUpdateKeywordFilterRuleDto.cs
├── FileUploadDownload/
│   ├── FileUploadInfoDto.cs
│   ├── CreateUpdateFileUploadInfoDto.cs
│   └── CustomFileTypeDto.cs
├── IP/
│   ├── DynamicIPDto.cs
│   └── CreateUpdateDynamicIPDto.cs
├── Lottery/
│   ├── LotteryDto.cs
│   ├── LotteryResultDto.cs
│   ├── LotteryPrizegradesDto.cs
│   ├── LotteryGroupDto.cs
│   ├── ResultItemDto.cs
│   ├── CompoundLotteryResultDto.cs
│   ├── PrizegradesItemDto.cs
│   ├── CreateUpdateLotteryDto.cs
│   ├── CreateUpdateLotteryResultDto.cs
│   ├── CreateUpdateLotteryPrizegradesDto.cs
│   ├── BatchCreate/
│   ├── Consts/
│   │   └── ConstsDto.cs
│   ├── Simulation/
│   │   ├── SSQ/
│   │   │   ├── LotterySimulationDto.cs
│   │   │   └── CreateUpdateLotterySimulationDto.cs
│   │   └── KL8/
│   │       ├── LotterySimulationDto.cs
│   │       └── CreateUpdateLotterySimulationDto.cs
│   └── Statistics/
│       ├── LotteryStructure.cs
│       ├── StatisticsInputDto.cs
│       ├── StatisticsWinDto.cs
│       ├── StatisticsWinItemDto.cs
│       └── StatisticsWinItemRequestDto.cs
├── Media/
│   ├── MediaInfoDto.cs
│   ├── ExternalLinkDto.cs
│   ├── CreateUpdateExternalLinkDto.cs
│   └── ExternalLink/
└── Rss/
    ├── RssSubscriptionDto.cs
    ├── RssMirrorDto.cs
    └── RssFetchDto.cs
```

### 8.2 映射器目录结构

```
src/DFApp.Web/Mapping/
├── AccountMapper.cs
├── Aria2Mapper.cs
├── BookkeepingMapper.cs
├── ConfigurationMapper.cs
├── ElectricVehicleMapper.cs
├── FileFilterMapper.cs
├── FileUploadDownloadMapper.cs
├── IPMapper.cs
├── LotteryMapper.cs
├── MediaMapper.cs
└── RssMapper.cs
```

---

## 9. 下一步工作

### 9.1 解决命名空间冲突

- 评估并实施命名空间冲突解决方案（推荐方案 A）
- 将 7 个保留手动映射的服务切换到 Mapperly 映射器
- 清理 `DFApp.Application.Contracts` 中的旧 DTO 引用

### 9.2 Phase 5：创建 Controller 层

为每个服务创建对应的 API Controller：

- 路由采用 `/api/app/{kebab-case-entity}` 模式
- 添加参数验证
- 添加 Swagger 文档注释
- 统一使用新 DTO 命名空间

### 9.3 Phase 6：添加权限控制

- 为每个服务的公共方法添加权限特性
- 定义相应的权限名称
- 确保权限检查逻辑正确实现

---

## 10. 相关文档

| 文档 | 说明 |
|------|------|
| [框架迁移计划](framework-migration-plan.md) | 整体迁移计划 |
| [Phase 1 迁移总结](phase1-migration-summary.md) | Phase 1 迁移详情 |
| [Phase 2.1 迁移总结](phase2.1-migration-summary.md) | Phase 2.1 迁移详情 |
| [Phase 2.2 迁移总结](phase2.2-migration-summary.md) | Phase 2.2 迁移详情 |
| [Phase 2.3 迁移总结](phase2.3-migration-summary.md) | Phase 2.3 迁移详情 |
| [Phase 3.1 迁移总结](phase3.1-migration-summary.md) | Phase 3.1 迁移详情 |
| [Phase 3.2 迁移总结](phase3.2-migration-summary.md) | Phase 3.2 仓储迁移详情 |
| [Phase 3.3 + 4.1 迁移总结](phase3.3-4.1-migration-summary.md) | Phase 3.3 + 4.1 迁移详情 |
| [Phase 3.3 + 4.2 最终迁移总结](phase3.3-4.2-final-migration-summary.md) | Phase 3.3 + 4.2 CrudAppService 迁移详情 |
| [Phase 3.3 + 4.3 最终迁移总结](phase3.3-4.3-final-migration-summary.md) | Phase 3.3 + 4.3 ApplicationService 迁移详情 |
| [Phase 4.5 迁移总结](phase4.5-migration-summary.md) | Phase 4.5 账户服务迁移详情 |
| [执行进度](执行进度.md) | 迁移执行进度跟踪 |
