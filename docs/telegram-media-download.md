# Telegram 媒体下载与空间回收

> 适用模块：`src/DFApp.Web/Background/ListenTelegramService.cs`
> 创建日期：2026-08-13

## 一、下载主循环

`DownloadMediaAsync` 从内部队列取出媒体任务逐个下载（图片走 `Photo`，视频/文件走 `Document`）。每个任务下载前依次检查：

1. **磁盘空间**（`IsSpaceUpperLimitAsync`）：`(盘可用MB - 文件MB) < 配置 AvailableFreeSpace` 时判定空间不足。
2. **日流量限制**（`CheckBandwidthLimitAsync`）：今日已下载总量超 `Bandwidth` 配置则等待到次日。

## 二、空间不足时的处理（`IsLoopDownload` 配置）

| `IsLoopDownload` | 空间不足时行为 |
|------------------|---------------|
| `false` | 直接删除**当前**这条媒体的 DB 记录并跳过（不下载） |
| `true`  | 调用 `DeleteOldestMediaUntilSpaceAvailableAsync`，**删除最旧的已下载文件**腾出空间后再下载当前 |

`DeleteOldestMediaUntilSpaceAvailableAsync` 在循环中反复判断空间，每次删除一个最旧的 `IsDownloadCompleted && !IsExternalLinkGenerated` 媒体（删物理文件 + 置 `IsExternalLinkGenerated=true`），直到空间足够或无可删项。

## 三、2026-08-13 变更：移除"时间密度算法"

**原行为**：`IsLoopDownload=true` 时曾用 `GetHighDensityMediaToDelete`（时间密度算法）选择删除目标——优先删同一时间窗口（默认 2 分钟，配置 `TimeDensityWindowMinutes`）内聚集的旧文件。

**现行为**：已删除该方法及其配置读取，统一改为**删除最旧的文件**（按 `CreationTime` 升序）。

> 注：磁盘空间不足时仍会删除媒体（仅选择方式由"密度优先"改为"最旧优先"）。如需保证下载器取回全部媒体，应确保服务器磁盘空间充足，或另行调整删除目标。
