# 下载器补漏同步：获取服务器已下载完成的媒体

> 适用模块：`DFApp.Downloader`（下载器） + `DFApp.Web`（后端媒体 API）
> 创建日期：2026-08-12

## 一、背景与问题

下载器非 24 小时运行。离线期间，远程服务器仍在持续下载 Telegram 媒体并标记完成（`MediaInfo.IsDownloadCompleted = true`），完成后通过 SignalR 推送 `DownloadCompleted` 通知。下载器离线时无人接收这些通知 → 这批文件下载器永远不会下载（SignalR 通知是"即发即忘"，不持久化）。

## 二、方案（仅 Telegram 媒体，手动触发）

新增"补漏同步"能力：下载器调用后端 API 拉取所有"已下载完成"的媒体，与本地 `DownloadItems` 表比对去重，把本地缺失的补入下载队列。

### 后端 API（DFApp.Web）

扩展现有 `MediaInfoController`（不新增控制器文件），新增查询接口：

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/app/media-info/completed?sinceId=0&pageIndex=1&pageSize=100` | 分页返回 `IsDownloadCompleted=true && IsExternalLinkGenerated=false`（已到服务器、未取回本地）的媒体，含 `downloadUrl` |
| POST | `/api/app/media-info/{id}/mark-external-link-generated` | 下载器取回本地后回写，置 `IsExternalLinkGenerated=true`（移出补漏集合） |
| DELETE | `/api/app/media-info/{id}/file` | 仅删除 `SavePath` 指向的物理文件（不删 DB 记录、不改字段）。下载器取回本地后调用，释放服务器存储空间 |

**返回结构**（复用 `MediaDownloadNotificationDto`，字段与推送通知一致，便于下载器复用入队逻辑）：
```json
{ "success": true, "data": { "items": [ { "fileName","fileSize","mimeType","downloadUrl","sourceType":"Telegram","sourceId","chatId","chatTitle","completedAt" } ], "totalCount": N } }
```

**DownloadUrl 生成**（与 `ListenTelegramService` 推送通知时同款逻辑）：
```
downloadUrl = Path.Combine(ReturnDownloadUrlPrefix, SavePath.Replace(ReplaceUrlPrefix, "").Replace("\\","/"))
```
两个配置通过 `IConfigurationInfoRepository.GetConfigurationInfoValue(name, "DFApp.Web.Background.ListenTelegramService")` 读取。

**权限**：`DFAppPermissions.Medias.Download`（下载器账号 `dfapp-download` 已具备）。

### 下载器（DFApp.Downloader）

- `DownloadNotificationClient` 暴露 `AccessToken`（登录后的 JWT，供调后端 API）。
- `DownloadManager.SyncMissedDownloadsAsync()`：
  1. 校验已登录（有 AccessToken）。
  2. 分页循环调 `GET {DfAppUrl}/api/app/media-info/completed`，带 `Authorization: Bearer {token}`。
  3. 每条按 `SourceType="Telegram" && SourceId` 与本地 `DownloadItems` 比对。
  4. 缺失项构造为 `MediaDownloadNotification`，调用现有 `OnNotificationReceived` 入队（复用全部既有落盘/下载逻辑）。
  5. 返回 `(Scanned, Added)` 统计。
- `DownloadsController` 加 `POST /api/downloads/sync-missed`（手动触发），返回 `{ scanned, added }`。

### 下载器前端（DFApp.Downloader/web）
- `Settings.vue` 增加"同步遗漏下载"按钮，调用 `syncMissed()`，`ElMessage` 提示"扫描 X 项，新增 Y 项"。
- `api/downloader.ts` 增加 `syncMissed()`。

## 三、文件变更清单

**新增**
- `DFApp/docs/downloader/missed-downloads-sync.md`（本文档）

**修改 - 后端**
- `DFApp.Web/Services/Media/MediaInfoService.cs`（加 `GetDownloadCompletedAsync` + 注入 `IConfigurationInfoRepository`）
- `DFApp.Web/Controllers/MediaInfoController.cs`（加 `completed` action）

**修改 - 下载器**
- `DFApp.Downloader.Core/SignalR/DownloadNotificationClient.cs`（暴露 `AccessToken`）
- `DFApp.Downloader.Core/DownloadManager.cs`（加 `SyncMissedDownloadsAsync`）
- `DFApp.Downloader.App/Controllers/DownloadsController.cs`（加 `sync-missed` action）

**修改 - 下载器前端**
- `DFApp.Downloader/web/src/api/downloader.ts`（加 `syncMissed`）
- `DFApp.Downloader/web/src/views/Settings.vue`（加同步按钮）
- `DFApp.Downloader/README.md`（API 端点表补充）

## 四、约束遵循
- AGENTS.md "不在 DFApp.Web 加控制器"：扩展现有 `MediaInfoController` 加 action，不新增控制器文件，符合项目所有自定义 API 均用手动控制器的既有模式。
- 数据访问用 SqlSugar 仓储（`Repository.GetPagedListAsync`），只读查询；不涉及 ef 迁移。
- 中文注释、非必要不加；文档先行并回填。

## 五、验证与部署

### 本地验证（已通过）
- 本地数据库 `AppMediaInfo` 有 29195 条 `IsDownloadCompleted=true` 记录，配置表 `AppConfigurationInfo` 含 `ReturnDownloadUrlPrefix`/`ReplaceUrlPrefix`。
- `BuildDownloadUrl` 逻辑验证：`../Telegram/Photo/x.jpg` + 前缀 → `http://localhost:8081/telegram/Photo/x.jpg`，与推送通知一致。
- 后端 `MediaInfoService` / `MediaInfoController` 编译通过（0 错误）。

### ⚠️ 远程部署要求（功能可用的前提）
下载器连接的是远程 `cc.bdbfbp.top`，补漏 API（`GET /api/app/media-info/completed`）必须**部署到远程后端**才能工作。否则下载器点"同步遗漏下载"会收到 `404`。

部署步骤（在远程服务器执行）：
1. 同步本次后端代码改动（`MediaInfoService.cs` + `MediaInfoController.cs`）。
2. `dotnet build` / 重新发布后端。
3. 重启远程后端服务。

部署完成后，下载器无需任何改动，直接在设置页点"同步遗漏下载"即可。

## 六、配置 ModuleName 历史遗留兼容性修复

排查补漏 400 时发现：`AppConfigurationInfo` 表里部分配置的 `ModuleName` 是旧模块名，与代码中的 `ModuleName` 常量不一致，导致 `GetConfigurationInfoValue`（按 module 精确匹配）查不到而抛 `BusinessException("配置参数不存在")`。

实测不匹配的配置（本地库）：
| 配置名 | 实际 ModuleName | 代码期望 |
|--------|-----------------|----------|
| `api_id` / `api_hash` / `phone_number` | `DFApp.Background.ListenTelegramService` | `DFApp.Web.Background.ListenTelegramService`（多 `.Web`） |
| `ReplaceUrlPrefix` | `DFApp.Background.MediaBackgroudService` | 同上 |

### 修复（两处，均已编译通过）
1. **`MediaInfoService.GetDownloadCompletedAsync`**：直接按配置名查询（`GetFirstOrDefaultAsync(x => x.ConfigurationName == name)`，忽略 module），从源头规避不匹配。
2. **`ListenTelegramService.GetConfigurationInfoAsync`**：加回退策略——先按 module 查，捕获 `BusinessException` 后回退到按配置名查询（忽略 module），仍查不到则重新抛出。既修复 `ReplaceUrlPrefix`（推送通知的 downloadUrl）等不匹配配置，又不影响正常匹配的配置读取。

> 该回退策略修复了实时推送通知链路：此前 `ReplaceUrlPrefix` 读不到会抛异常，被推送通知的 `try-catch` 吞掉（日志报"推送下载完成通知失败"），导致通知里的 downloadUrl 一直异常。修复后实时通知的 downloadUrl 也能正确生成。

## 七、增量同步游标已移除（2026-08-14 修复）

**原设计**：初版补漏同步每次都全量拉取（数万条），后加了 `sinceId` 增量游标——后端只返回 `Id > sinceId`（下载器本地最大 SourceId）的记录。

**问题**：`sinceId = 本地最大 SourceId` 是"高水位"游标，会跳过中间所有空洞。一旦出现以下情况，空洞里的媒体**永远无法被补漏同步命中**：
- 下载失败后用户手动删除了下载器本地记录；
- 通知丢失（下载器离线/推送失败）；
- 任何 `Id` 小于本地最大 SourceId 但未取回的媒体。

实测（本地库）：后端未取回媒体最大 Id=53586，下载器本地最大 SourceId=67548 → `Id > 67548` 一条都不返回，**全部未取回媒体同步不到**。

**修复**：**移除 `sinceId` 游标**（后端与下载器同步改）：
- 后端 `GET /api/app/media-info/completed` 返回**全部** `IsDownloadCompleted && !IsExternalLinkGenerated` 的记录（分页，按 Id 升序），不再有 `sinceId` 参数。
- 下载器 `SyncMissedDownloadsAsync` 不再计算/携带游标，全量拉取 + 本地 `SourceType+SourceId` HashSet 去重。
- 目标集合本身就是"已下载未取回"（取回后回写 `IsExternalLinkGenerated=true` 即移出），通常只有几十条，全量拉取开销可忽略。

> 向后兼容：后端忽略多余的 `sinceId` 查询参数（旧下载器仍可用）；下载器不传 `sinceId` 时旧后端默认全量（也正确）。两端一起部署后彻底移除。

### 效果
| 场景 | 拉取量 |
|------|--------|
| 首次同步（本地空） | 全部未取回（一次性） |
| 后续同步 | 全部未取回（通常几十条，取回即移出集合） |
| **存在失败/删除/通知丢失的空洞** | **可重新命中（原游标方案漏掉）** |

## 七·二、回写 401 与同步修复回写（2026-08-14 新增）

**背景**：下载器 JWT 有有效期，长时间下载（大文件、批量）后 token 过期，下载完成时的回写（`mark-external-link-generated` + `DELETE /file`）以 401 失败——此前仅记日志不重试，而补漏同步又因本地去重跳过这些项，导致**服务器永远停留在"未取回"**（实测 28 项，本地文件其实已下载完成）。

**修复（下载器 `DownloadManager`）**：
1. **回写 401 自动重试**：`MarkExternalLinkGeneratedAsync` / `DeleteRemoteFileAsync` 遇 401 时重新登录（`LoginAsync`）后重试一次，从源头避免失败。
2. **同步修复回写**：`SyncMissedDownloadsAsync` 对"服务器未取回、但本地记录已 `Completed` 且文件存在"的项，视为回写失败残留，**补一次回写并删除远程文件**（幂等，使用同步时的有效 token）。返回新增 `reconciled`（修复回写数）字段，前端提示"扫描 X 项，新增 Y 项到下载队列，修复回写 Z 项"。

> 效果：任何历史/未来的回写失败，在下一次补漏同步时自动纠正服务器状态；服务器 UI 的"是否生成外部链接"与实际取回情况保持一致。

## 八、IsExternalLinkGenerated 语义统一与下载完成回写闭环

### 字段语义澄清（2026-08-12 确认）
`MediaInfo` 的两个 bool 字段：
- `IsDownloadCompleted` = 内容已从外部（Telegram）下载到**服务器**。
- `IsExternalLinkGenerated` = 已生成外链 / 已被下载器**取回本地**（复用此字段）。

补漏同步的目标集合 = `IsDownloadCompleted=true && IsExternalLinkGenerated=false`：已到服务器、尚未取回本地的媒体。本地下载完成后回写后端置 `IsExternalLinkGenerated=true`，该媒体即移出目标集合 → **天然增量，彻底解决"每次扫描数万条"的问题**。

### 改动
1. **后端过滤**：`MediaInfoService.GetDownloadCompletedAsync` 过滤条件为 `IsDownloadCompleted && !IsExternalLinkGenerated`（游标已移除，见第七节）。
2. **回写 API**：`MediaInfoController` 新增 `POST /api/app/media-info/{id}/mark-external-link-generated`（权限 `Medias.Download`）；`MediaInfoService.MarkExternalLinkGeneratedAsync(long id)` 按 Id 查实体置 true 并更新。
3. **下载器回写**：`DownloadManager.OnDownloadCompleted` 本地写库成功后，对 `SourceType=="Telegram"` 的项 fire-and-forget 调用回写 API（Bearer token）；失败仅记日志，下次同步会再命中，本地去重兜底。

### 闭环效果
- 补漏同步只返回真正"未取回"的 → 扫描量从"所有已下载"降为"已下载未取回"。
- 下载器下载完成即回写 → 后端集合持续缩小。
- 本地 `SourceType+SourceId` 去重 + 后端 `IsExternalLinkGenerated` 标记双重保险。

### ⚠️ 对「外部链接管理」功能的影响（已确认接受）
`IsExternalLinkGenerated` 同时被 `ExternalLinkService`（前端「外部链接管理」页的"新增外部链接"按钮）使用：它查询 `!IsExternalLinkGenerated && IsDownloadCompleted` 的媒体来生成分享外链（打包 zip 或拼 apache URL），生成后置 true。下载器取回后也置 true，意味着**被下载器取回的媒体不再进入「外部链接管理」的生成队列**。

此影响已确认接受（语义统一为"已处理/已取回"）。此外该字段还被 `ListenTelegramService` 的磁盘清理复用为软删除标记（已删除的也置 true），属既有耦合。

## 九、下载完成删除远程物理文件（释放服务器空间）

### 背景
远程服务器只是 Telegram 媒体的**中转站**：媒体先下载到服务器（`IsDownloadCompleted=true`），再由下载器取回本地。取回本地后，服务器上的物理文件即失去意义，长期堆积会耗尽磁盘。

### 删除范围（2026-08-12 确认）
下载器取回本地后，**仅删除远程物理文件，保留 `MediaInfo` DB 记录并保持 `IsExternalLinkGenerated=true`**。
- 物理文件删除后，该媒体的下载链接（`GET /download`）将失效（文件已不在）——这是可接受的，因为本地已有副本。
- DB 记录保留作为历史索引；补漏同步因 `IsExternalLinkGenerated=true` 不再返回该记录。
- **不删 DB 记录**：保留可追溯性，且避免 `ExternalLinkService` 等关联查询出现悬挂引用。

### 改动
1. **后端 `MediaInfoService.DeletePhysicalFileAsync(long id)`**：按 Id 查实体 → `SpaceHelper.DeleteFile(entity.SavePath)`（忽略错误，文件不存在视为已删除）→ 不删 DB、不改字段。实体不存在返回 false。
2. **后端 `MediaInfoController` 新增 `DELETE /api/app/media-info/{id}/file`**：权限 `Medias.Download`（下载器账号已具备），调上述服务方法，返回 `Success/Fail`。路由与现有 `DELETE /{id}`（删 DB）和 `DELETE /invalid` 不冲突（`{id:long}/file` 多一段）。
3. **下载器 `DownloadManager.NotifyRetrievedAndCleanupAsync(long mediaInfoId)`**：`OnDownloadCompleted` 中对 `SourceType=="Telegram"` 的项 fire-and-forget 调用，**串行**执行：
   1. `MarkExternalLinkGeneratedAsync`（先标记 `IsExternalLinkGenerated=true`，让补漏同步不再返回）
   2. `DeleteRemoteFileAsync`（`DELETE {DfAppUrl}/api/app/media-info/{id}/file`）
   - 串行保证：即使删文件失败，标记已成功则不会重复下载；删文件失败仅记日志，下次同步命中本地去重兜底。

### 容错
- 删除远程文件是 fire-and-forget，任何异常（网络/权限/接口未部署）仅记日志，不影响本地下载流程。
- 远程后端未部署该接口时返回 `404`，日志记录"删除远程文件失败：HTTP 404"，标记步骤仍成功（补漏闭环不受影响）。

### ⚠️ 远程部署要求
`DELETE /api/app/media-info/{id}/file` 必须**部署到远程后端**（`cc.bdbfbp.top`）才能生效。部署前，下载器下载完成时删除调用会收到 `404`，物理文件不会被删除（但回写标记正常）。

## 十、开机自动补漏同步（2026-09-04）

### 背景
电脑重启后下载器随系统自启，此前补漏同步只能手动在设置页点按钮触发——忘记点就漏掉离线期间的媒体。

### 实现
- `DownloaderSettings` 新增 `SyncMissedOnStartup`（默认 `true`），设置页"其他"区新增"开机补漏同步"开关。
- `DownloadManager.StartAsync` 启动后台任务 `RunStartupSyncAsync`：
  - 每 10 秒轮询登录状态（Token），上限等待 15 分钟——电脑重启时后端可能尚未就绪，SignalR 重连循环会持续重试登录，登录成功即触发；
  - 调用 `SyncMissedDownloadsAsync` 执行一次同步并记录结果日志（"开机补漏同步完成：扫描 X 项，新增 Y 项，修复回写 Z 项"）；
  - 任何异常仅告警不阻断启动，可稍后手动同步。
- `SyncMissedDownloadsAsync` 加 `SemaphoreSlim` 互斥：开机自动同步与设置页手动同步不会并发执行。
- 修复存量缺陷：补漏拉到的历史媒体 `message` 为空时，入队触发 `DownloadItems.Message` 非空约束崩溃——`OnNotificationReceived` 入库前对 `ChatTitle`/`Message` 空值兜底。

### 验证（隔离实例，端口 9560）
| 场景 | 结果 |
|------|------|
| 后端无未取回媒体 | 开机同步执行，扫描 0 新增 0，日志正常 |
| 一条"未取回"媒体（含空 message） | 扫描 1 新增 1，成功入队，下载失败自动重试（本地 Apache 未运行属预期） |
| `SyncMissedOnStartup=false` | 登录连接正常，不执行任何补漏 |


