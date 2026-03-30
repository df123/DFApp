# Phase 4.3 Batch 7 - LotteryDataFetchService 和 CompoundLotteryService 迁移摘要

## 迁移日期
2026-03-30

## 迁移的服务

### 1. LotteryDataFetchService
- **原文件**: `src/DFApp.Application/Lottery/LotteryDataFetchService.cs`
- **新文件**: `src/DFApp.Web/Services/Lottery/LotteryDataFetchService.cs`

#### 迁移变更
| 项目 | 原实现 | 新实现 |
|------|--------|--------|
| 基类 | `DFAppAppService` (ABP) | `AppServiceBase` |
| 仓储 | `IRepository<LotteryResult, long>` (ABP) | `ISqlSugarRepository<LotteryResult, long>` |
| 映射 | `IObjectMapper` (ABP) | 手动映射 + `// TODO: 使用 Mapperly 映射` |
| 工作单元 | `IUnitOfWorkManager.Begin()` / `CompleteAsync()` / `RollbackAsync()` | `BeginTran()` / `CommitTran()` / `RollbackTran()` |
| 日志 | `Logger` (ABP内置) | `ILogger<LotteryDataFetchService>` (注入) |
| 日志格式 | 字符串插值 `$"..."` | 结构化日志 `{ParamName}` |
| 异常 | `UserFriendlyException` (ABP) | `BusinessException` |
| 授权 | `[Authorize]` 属性 | 通过 `AppServiceBase` 权限检查 |

#### 新增依赖
- `ISqlSugarRepository<LotteryPrizegrades, long>` - 用于保存开奖奖金等级数据（原代码中通过导航属性隐式处理）

#### 公共方法（4个）
1. `FetchLotteryData(LotteryDataFetchRequestDto)` - 获取彩票数据
2. `FetchSSQLatestData()` - 获取双色球最新数据
3. `FetchKL8LatestData()` - 获取快乐8最新数据
4. `TestLotteryApiConnection(string)` - 测试彩票API连接

#### 优化点
- 日志从字符串插值改为结构化日志（`_logger.LogInformation("消息 {Param}", value)`）
- Prizegrades 保存逻辑从导航属性改为显式分步保存（先保存 LotteryResult，再保存 Prizegrades）
- 移除了导航属性 `Result` 的设置，改用 `LotteryResultId` 外键关联

### 2. CompoundLotteryService
- **原文件**: `src/DFApp.Application/Lottery/CompoundLotteryService.cs`
- **新文件**: `src/DFApp.Web/Services/Lottery/CompoundLotteryService.cs`

#### 迁移变更
| 项目 | 原实现 | 新实现 |
|------|--------|--------|
| 基类 | `ApplicationService` (ABP) | `AppServiceBase` |
| 仓储 | `IRepository<LotteryInfo, long>` (ABP) | `ISqlSugarRepository<LotteryInfo, long>` |
| 工作单元 | `IUnitOfWorkManager.Begin()` / `CompleteAsync()` / `RollbackAsync()` | `BeginTran()` / `CommitTran()` / `RollbackTran()` |
| 映射 | `ObjectMapper.Map<>()` (ABP) | 手动映射 `MapToLotteryDto()` + `// TODO: 使用 Mapperly 映射` |
| 异常 | `UserFriendlyException` (ABP) | `BusinessException` |
| 授权 | `[Authorize]` 属性 | 通过 `AppServiceBase` 权限检查 |
| 验证方法 | `async Task<string>` | `string`（移除不必要的 async） |

#### 公共方法（1个）
1. `CalculateCompoundCombination(CompoundLotteryInputDto)` - 计算复式投注组合

#### 优化点
- `ValidateCompoundInput` 从 `async Task<string>` 改为同步 `string`（原方法内无异步操作）
- `ObjectMapper.Map<>()` 替换为显式手动映射方法 `MapToLotteryDto()`
- 移除了不必要的 `using` 引用（`Volo.Abp.*`）

## 待处理事项
- [ ] 使用 Mapperly 替换手动映射（`LotteryDataFetchService.MapResultItemToLotteryResult`、`CompoundLotteryService.MapToLotteryDto`）
- [ ] 注册服务到 DI 容器
- [ ] 创建对应的 Controller（路由：`/api/app/lottery-data-fetch`、`/api/app/compound-lottery`）
