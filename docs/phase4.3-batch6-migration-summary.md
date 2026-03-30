# Phase 4.3 Batch 6 - RssSubscriptionService 和 WordSegmentService 迁移摘要

## 迁移日期
2026-03-30

## 迁移范围
迁移 RSS 订阅处理服务和分词服务，这两个服务不是继承自 `ApplicationService` 的应用服务，而是实现特定接口的内部服务类。

## 迁移的文件

### 1. RssSubscriptionService
- **原文件**: `src/DFApp.Application/Rss/RssSubscriptionService.cs`
- **新文件**: `src/DFApp.Web/Services/Rss/RssSubscriptionService.cs`

### 2. WordSegmentService
- **原文件**: `src/DFApp.Application/Rss/WordSegmentService.cs`
- **新文件**: `src/DFApp.Web/Services/Rss/WordSegmentService.cs`

## 迁移变更详情

### RssSubscriptionService

| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类/接口 | `IRssSubscriptionService, ITransientDependency` | `IRssSubscriptionService`（移除 `ITransientDependency`） |
| 仓储 | `IRepository<T, long>` (ABP) | `ISqlSugarRepository<T, long>` (SqlSugar) |
| IAria2Service | 直接注入使用 | 可选注入（`IAria2Service?`），核心逻辑用 TODO 伪代码标记 |
| 异常处理 | 隐式依赖 ABP | 使用 `?? throw new InvalidOperationException()` |
| 命名空间 | `DFApp.Rss` | `DFApp.Web.Services.Rss` |

**方法迁移**:
- `MatchSubscriptionsAsync` - 订阅匹配逻辑完整迁移，包含 RSS 源、日期范围、关键词、质量、字幕组、做种者/下载者/完成数范围等过滤条件
- `CreateDownloadTaskAsync` - 下载任务创建逻辑迁移，Aria2 调用部分用 TODO 标记
- `ProcessPendingDownloadsAsync` - 暂存下载处理逻辑迁移，增加了空值检查优化
- `GetAvailableDiskSpace` - 磁盘空间检查私有方法完整迁移

**优化改进**:
- `ProcessPendingDownloadsAsync` 中增加了订阅和镜像条目的空值检查，避免处理已删除的关联数据
- `CreateDownloadTaskAsync` 使用 `?? throw` 模式替代隐式异常

### WordSegmentService

| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类/接口 | `IWordSegmentService, ITransientDependency` | `IWordSegmentService`（移除 `ITransientDependency`） |
| 依赖注入 | 仅 `ILogger` | 仅 `ILogger`（无变化） |
| 命名空间 | `DFApp.Rss` | `DFApp.Web.Services.Rss` |

**方法迁移**:
- `Segment` - 分词主入口，使用 `switch` 表达式替代 `switch` 语句
- `SegmentAndCount` - 分词并统计词频
- `DetectLanguage` - 语言检测（中文/英文/日文）
- `SegmentChinese` - 中文分词
- `SegmentEnglish` - 英文分词
- `SegmentJapanese` - 日文分词
- `SegmentMixed` - 混合语言分词
- `IsChinese` / `SplitChineseText` - 中文文本处理辅助方法

**优化改进**:
- `Segment` 方法中使用 `switch` 表达式替代 `switch` 语句，代码更简洁

## 待处理事项

1. **IAria2Service 依赖**: `RssSubscriptionService` 中的 Aria2 下载调用仍为伪代码，需等待 `IAria2Service` 迁移完成后替换
2. **接口定义**: `IRssSubscriptionService` 和 `IWordSegmentService` 仍在 `src/DFApp.Domain/Rss/` 中，引用了旧的 ABP 命名空间，后续需要在新项目中重新定义接口
3. **服务注册**: 移除了 `ITransientDependency`，需要在 `Program.cs` 中手动注册这两个服务

## 编译说明

当前迁移的文件可能存在编译问题，这是预期行为：
- `IRssSubscriptionService` 接口在旧 ABP Domain 项目中，引用了旧的 `RssMirrorItem` 类型
- `IWordSegmentService` 接口同样在旧项目中
- 根据迁移规则，不需要为解决编译问题而修改其他代码
