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
| GET | `/api/app/media-info/completed?pageIndex=1&pageSize=100` | 分页返回 `IsDownloadCompleted=true` 的媒体，含 `downloadUrl` |

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

## 七、增量同步优化（避免每次全量拉取）

初版补漏同步每次都从第 1 页拉取全部已完成媒体（数万条），即便本地已下载大部分——绝大部分流量和请求只用于比对后跳过，不合理。

### 优化：sinceId 增量游标
- **后端** `GET /completed` 新增 `sinceId` 参数（默认 0）：只返回 `Id > sinceId` 的记录（按 Id 升序）。
- **下载器** `SyncMissedDownloadsAsync`：先查本地 `DownloadItems` 中 `SourceType=Telegram` 的最大 `SourceId` 作为 `sinceId`，请求时带上；后端只返回比它新的。
- 保留 `HashSet` 内存去重作为双保险（防止增量边界处的重复）。

### 效果
| 场景 | 拉取量 |
|------|--------|
| 首次同步（本地空） | sinceId=0，拉全部（一次性，必然） |
| 后续同步 | sinceId=本地最大，只拉新增量（通常几条到几十条） |

> 局限：sinceId 只补"比本地最新的"，不补"中间空洞"。正常按顺序下载场景无空洞。


