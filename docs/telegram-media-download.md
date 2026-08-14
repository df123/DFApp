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

## 三、下载器取回保护（2026-08-14 新增）

**背景**：空间清理按 `CreationTime` 最旧优先删除，而下载器（DFApp.Downloader）也是按通知/队列顺序取回——清理命中的往往正是下载器**正在下载**的那个文件，导致下载中途源文件消失、任务失败（且清理后媒体被标记为已取回，补漏同步不会再下发，内容永久丢失）。

**机制**（新增 `Services/Media/MediaRetrievalTracker.cs`，内存态、单例）：
- **标记保护**：媒体下载完成推送下载器通知时（`NotifyDownloaderAsync`）与补漏同步下发时（`GetDownloadCompletedAsync`）调用 `MarkPending`，进入取回保护期（**6 小时** TTL，超时自动失效，防止下载器失联导致磁盘永远无法回收）。
- **解除保护**：下载器确认取回回写 `mark-external-link-generated` 时调用 `ClearPending`。
- **清理跳过**：`DeleteOldestMediaUntilSpaceAvailableAsync` 只删除**非保护期**的媒体；全部处于保护期时跳过本轮清理（记 WARN 日志）。
- **重启重建**：`ListenTelegramService` 启动时为存量 `IsDownloadCompleted && !IsExternalLinkGenerated` 媒体统一重建保护，避免后端重启后清理误删。
- **兜底**：清理后磁盘空间仍不足时（可删项均处于保护期）跳过本次 Telegram 下载，避免写盘失败。

> 保护期内文件不参与空间回收，空间不足时优先阻塞新的 Telegram 下载，保证下载器在途任务不被破坏。

## 四、2026-08-13 变更：移除"时间密度算法"

**原行为**：`IsLoopDownload=true` 时曾用 `GetHighDensityMediaToDelete`（时间密度算法）选择删除目标——优先删同一时间窗口（默认 2 分钟，配置 `TimeDensityWindowMinutes`）内聚集的旧文件。

**现行为**：已删除该方法及其配置读取，统一改为**删除最旧的文件**（按 `CreationTime` 升序）。

> 注：磁盘空间不足时仍会删除媒体（仅选择方式由"密度优先"改为"最旧优先"）。下载器在途/未取回媒体由取回保护机制（见第三节）排除在清理范围之外。
