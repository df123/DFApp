# 下载引擎：基于 Downloader 库的分片下载

> 适用模块：`DFApp.Downloader.Core/Engine/DownloadEngine.cs` + `DownloadManager.cs`
> 创建日期：2026-08-13（2026-08-13 改用成熟下载库）

## 一、实现

下载引擎**不再自行实现分片/Range 逻辑**，改用 NuGet 库 [`Downloader`](https://www.nuget.org/packages/Downloader)（bezzad）：
- 多分片并行下载，分片直接写入最终文件（无临时文件）；
- 自动探测服务器是否支持 Range，不支持时自动回退单连接；
- 自带断点续传、分片级失败重试（`MaxTryAgainOnFailure`）；
- 通过 `DownloadProgressChanged` 事件提供**已下载字节**与**瞬时速度**。

包引用在 `DFApp.Downloader.Core.csproj`：`<PackageReference Include="Downloader" Version="5.9.5" />`。

## 二、配置映射（`DownloadEngine.BuildConfiguration`）

| 库配置 | 取值 | 说明 |
|--------|------|------|
| `ChunkCount` | `MaxSegmentsPerFile` | 文件切分片数 |
| `ParallelDownload` / `ParallelCount` | `true` / `MaxSegmentsPerFile` | 分片并行下载 |
| `MaxTryAgainOnFailure` | `3` | 分片失败自动重试 |
| `CustomHttpMessageHandlerFactory` | `SocketsHttpHandler { UseProxy = false }` | **禁用系统代理**，绕过本地 `http(s)_proxy=127.0.0.1:10079`，与原注入 HttpClient 的 `UseProxy=false` 一致 |
| `RequestConfiguration.Authorization` | `Basic(ApacheUsername:ApachePassword)` | Apache Basic Auth（非空时设置） |

> 历史教训：旧实现用 `MaxSegmentsPerFile` 封顶分片数后**未重算每片大小**，导致大文件只下载前 `MaxSegmentsPerFile × SegmentSize` 字节（默认恰好 16MB）即被标记完成。改用库后此问题彻底消除。

## 三、设置项

| 配置项（settings.json） | 现用途 |
|------------------------|--------|
| `MaxSegmentsPerFile` | 映射到库 `ChunkCount` / `ParallelCount` |
| `MaxConcurrentDownloads` | 文件级并发（`DownloadEngine` 的 `SemaphoreSlim`），与库的分片并行正交 |
| `SegmentSize` | **已废弃**（库按 `ChunkCount` 均分，不再使用）；字段保留以免破坏旧配置 |

## 四、进度与速度来源

- **进度**：库 `DownloadProgressChanged.ReceivedBytesSize` → `OnProgress` → `DownloadManager.OnProgressReceived` **节流（每项 ≤1s）写回 DB `DownloadedBytes`**，界面进度条据此实时更新。
- **速度**：库 `DownloadProgressChanged.BytesPerSecondSpeed` → 直接存入内存 `_activeSpeeds`，`GetStatus` 求和得总速度，单条速度由 `GetItemSpeed` 返回。
- **完成结果**：库以 `DownloadFileCompleted` 事件交付成败，引擎用 `TaskCompletionSource` 桥接到 `await`，映射为 `OnDownloadCompleted` / `OnDownloadFailed`。

## 五、完成完整性校验（兜底）

`DownloadManager.OnDownloadCompleted` 在标记 `Completed` 前，校验本地文件实际大小 == `FileSize`；不匹配则标记 `Failed`，**不**回写"已取回"、**不**删远程文件，避免任何原因的下载不完整被误判为完成。

## 六、卡死看门狗（2026-08-13 新增）

**背景**：多分片并行下载时，个别分片连接可能被服务器/Cloudflare 静默掐断——连接看似存在但不再有数据流，下载库的超时机制（`BlockTimeout` 等）覆盖不到此场景，导致任务**永远卡在 Downloading**（实测卡死 8 小时+），既不完成也不失败。

**机制**（`DownloadManager.StallWatchdogAsync`，随管理器启动，每 60s 检查一次）：
- 每个下载项维护"最近一次进度事件时间"（`_lastProgressAt`，每次进度事件都更新，不节流）；
- 若某 Downloading 任务超过 **5 分钟**无任何进度事件 → 判定卡死 → 自动：`PauseDownload`（取消当前下载）→ 删除 `.download` 临时文件 → 重置 `Status=Pending / DownloadedBytes=0` → 重新入队。
- 全程记录 WARN 日志"下载疑似卡死…自动重启"。

**效果**：分片连接挂起后最多 ~6 分钟即被自动重启（5 分钟阈值 + 1 分钟检查间隔），不再无限挂起。
