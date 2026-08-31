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
- **标记保护**：媒体下载完成推送下载器通知时（`NotifyDownloaderAsync`）与补漏同步下发时（`GetDownloadCompletedAsync`）调用 `MarkPending`，进入取回保护期。
- **解除保护（提前清除）**：下载器确认取回回写 `mark-external-link-generated` 时立即调用 `ClearPending`——正常流程下保护只持续"通知→下载完成"的几分钟，**不等到超时**。
- **保护时长**：默认 **2 小时**，可配置 `RetrievalProtectionHours`（小时，sql/20-add-retrieval-protection-config.sql）。超时仍未确认取回（如下载器失联）则自动失效，防止磁盘永远无法回收。
- **清理跳过**：`DeleteOldestMediaUntilSpaceAvailableAsync` 只删除**非保护期**的媒体；全部处于保护期时跳过本轮清理（记 WARN 日志）。
- **重启重建**：`ListenTelegramService` 启动时为存量 `IsDownloadCompleted && !IsExternalLinkGenerated` 媒体统一重建保护，避免后端重启后清理误删。
- **兜底**：清理后磁盘空间仍不足时（可删项均处于保护期）跳过本次 Telegram 下载，避免写盘失败。

> 保护期内文件不参与空间回收，空间不足时优先阻塞新的 Telegram 下载，保证下载器在途任务不被破坏。

## 四、一键清理已取回媒体文件（2026-08-14 新增）

**背景**：下载器确认取回（`IsExternalLinkGenerated=true`）后应删除远程文件，但删除失败（网络中断、HTTP 错误）时文件会残留在服务器上，占用磁盘且界面上无法区分。

**功能**：媒体管理页面新增"清理已取回文件"按钮，调用 `POST /api/app/media-info/cleanup-retrieved-files`（`MediaInfoService.CleanupRetrievedFilesAsync`）：
- 扫描所有 `IsExternalLinkGenerated=true` 的媒体；
- 跳过仍被**有效（未移除）外链**引用的媒体（删除会破坏外链内容）；
- 其余媒体若物理文件仍存在则删除，返回统计：`Deleted`（删除数）、`Skipped`（被外链引用跳过数）、`NoFile`（路径为空或文件已不存在，无需处理）。

> 仅删除物理文件，不删 DB 记录、不改任何字段——与下载器取回后的单文件删除（`DELETE /file`）行为一致，可重复执行。

## 五、2026-08-13 变更：移除"时间密度算法"

**原行为**：`IsLoopDownload=true` 时曾用 `GetHighDensityMediaToDelete`（时间密度算法）选择删除目标——优先删同一时间窗口（默认 2 分钟，配置 `TimeDensityWindowMinutes`）内聚集的旧文件。

**现行为**：已删除该方法及其配置读取，统一改为**删除最旧的文件**（按 `CreationTime` 升序）。

> 注：磁盘空间不足时仍会删除媒体（仅选择方式由"密度优先"改为"最旧优先"）。下载器在途/未取回媒体由取回保护机制（见第三节）排除在清理范围之外。

## 六、2026-08-14 变更：聊天消息（Message）随下载通知下发

**背景**：下载器媒体库页面需要展示每条媒体的聊天消息文本，此前 `MediaDownloadNotificationDto` 只下发 `ChatTitle` 不含 `Message`。

**变更**：
- `MediaDownloadNotificationDto` 新增 `Message` 字段（对应 `MediaInfo.Message`）；
- `MediaInfoService.GetDownloadCompletedAsync` 映射时填充 `Message = e.Message`；
- 下载器 `DownloadItem`/`MediaDownloadNotification` 同步新增 `Message`，旧记录由下载器按 mediaId 从 `media-info/paged` 回填（详见 `docs/downloader/media-gallery.md`）。

## 七、2026-08-14 变更：移除 DownloaderEnabled 开关

**背景**：`DownloaderEnabled` 配置（SQL 种子默认 `false`）控制是否向 Downloader 子程序推送下载完成通知。实际部署后从未置为 `true`，导致通知一直静默不推送——下载器只能靠手动/启动补漏同步拉取新任务，表现为"远程有新媒体但本地不自动下载"。

**变更**：
- 删除 `ListenTelegramService.NotifyDownloaderAsync` 与 `Aria2Manager.NotifyDownloaderAsync` 中的 `DownloaderEnabled` 检查，**改为无条件推送**——下载器是本项目配套组件，通知推送为必需功能，无需开关（避免"忘配置 → 静默不推送"的坑）；
- 保留 `ReturnDownloadUrlPrefix`/`ReplaceUrlPrefix`（Telegram）与 `Aria2ApachePathPrefix`（Aria2）的前缀配置检查，并改为 WARN 日志（不再静默）；
- `sql/19-add-downloader-config.sql` 移除该配置种子；新增 `sql/21-remove-downloader-enabled-config.sql` 供远程清理存量配置。

## 八、2026-08-25 变更：Telegram 登录接口权限收紧

- `/api/app/tg-login/status`、`/config`、`/chats` 均要求 `DFApp.TelegramManagement` 权限。
- 新增 `sql/25-add-telegram-management-permission.sql`，为 `admin` 角色授予该权限。
