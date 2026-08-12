# 下载器：日志查看功能 + 连接反馈增强

> 适用模块：`DFApp.Downloader`（独立子项目）
> 创建日期：2026-08-11

## 一、背景与问题根因

下载器服务运行时，连接 DFApp 后端失败后前端只显示一个红色的"未连接"标签，无法看到失败原因。经排查根因如下：

1. **失败原因未透传**：后端 `DownloadManager.GetStatus()` 只返回 `isConnected: bool`，从不暴露失败原因；`DownloadNotificationClient.LoginAsync` 使用 `EnsureSuccessStatusCode()` 直接抛异常，丢弃了响应体中的业务消息（如"用户名或密码错误""登录尝试次数过多"）。
2. **无自动重试**：`DownloadManager.TryConnectAsync()` 仅在启动时尝试一次，失败只记 Warning 日志，之后不再重连。
3. **改密码不即时生效**：进程启动时一次性把 `settings.json` 加载到内存，用户在设置页或文件中改密码后，必须重启进程或手动触发重连才会用新密码登录。
4. **远程后端限流**：旧密码多次失败累积触发后端登录限流（"请 15 分钟后再试"）。

## 二、功能设计

### 1. 日志查看功能（核心）

在 Web 管理界面新增"日志"页面，可直接查看下载器运行日志（Serilog 输出到 `logs/downloader-*.log`），从而定位登录失败、下载异常等问题。

#### 后端 API

新增 `LogsController`（`Route("api/logs")`），仿照现有 `DownloadsController` 风格：

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/logs` | 列出所有日志文件，返回 `{ fileName, sizeBytes, lastWriteTime }[]`，按修改时间倒序 |
| GET | `/api/logs/{fileName}?lines=800&order=tail` | 读取日志内容，返回 `{ fileName, content, returnedLines, order }` |

**参数约定**：
- `lines`：返回行数，默认 800，上限 5000。
- `order`：`tail`（默认，读末尾 N 行）/ `head`（读前 N 行）。

**路径安全**：
- fileName 禁止包含 `..`、`/`、`\`。
- 扩展名限定 `.log`。
- `Path.GetFullPath` 组合后，校验结果路径仍位于 logs 目录内，防止目录穿越。

#### 前端页面

新增 `Logs.vue`（路由 `/logs`，导航菜单新增"日志"）：
- 左侧：日志文件列表（文件名、大小、修改时间），点击加载内容，带刷新按钮。
- 右侧：日志内容区，`<pre>` 等宽显示。
  - 工具栏：行数选择（200/500/1000/2000）、head/tail 切换、自动刷新开关（tail 模式默认开，间隔 5s）。
  - 按 Serilog 级别着色：ERR/FAT→红、WRN→黄、INF→绿、其余默认。

### 2. 连接反馈增强（解决"没有提示"）

#### 后端
- `DownloadNotificationClient` 新增 `LastConnectionError` 属性；`LoginAsync` 改为先读响应体再判定，把业务错误消息（凭证错误/限流/网络异常）写入该属性；登录成功时清空。
- `DownloadManager.GetStatus()` 增加 `lastError` 字段。
- `DownloadsController`：修正 `GetConnection`（原 `new { isConnected = GetStatus() }` 嵌套结构错误）为扁平 `{ isConnected, lastError }`；`Reconnect` 同样返回 `{ isConnected, lastError }`。

#### 前端
- `Dashboard.vue`：连接状态卡片下方，未连接时用 `el-alert(type=error)` 显示 `lastError`；卡片增加"重新连接"按钮（调 reconnect API）。
- `GlobalStatus` 类型增加 `lastError?: string`。

## 三、文件变更清单

**新增**
- `DFApp.Downloader/src/DFApp.Downloader.App/Controllers/LogsController.cs`
- `DFApp.Downloader/web/src/views/Logs.vue`
- `DFApp/docs/downloader/logs-viewer-and-connection-feedback.md`（本文档）

**修改**
- `DFApp.Downloader/src/DFApp.Downloader.Core/SignalR/DownloadNotificationClient.cs`
- `DFApp.Downloader/src/DFApp.Downloader.Core/DownloadManager.cs`
- `DFApp.Downloader/src/DFApp.Downloader.App/Controllers/DownloadsController.cs`
- `DFApp.Downloader/web/src/api/downloader.ts`
- `DFApp.Downloader/web/src/views/Dashboard.vue`
- `DFApp.Downloader/web/src/App.vue`
- `DFApp.Downloader/web/src/router/index.ts`
- `DFApp.Downloader/README.md`（API 端点表补充）

## 四、验证步骤

1. `dotnet build` 下载器项目，重启进程。
2. `pnpm build` 前端，将 `web/dist` 内容复制到 `src/DFApp.Downloader.App/wwwroot/`。
3. 浏览器访问 `http://localhost:9550/logs` 验证日志页面；`http://localhost:9550/dashboard` 验证连接失败提示。
4. 等待远程后端限流自然解除（约 15 分钟）后重启下载器加载新密码，通过日志页面观察登录结果。

## 五、实现过程发现的问题（已修复 / 待处理）

### 5.1 登录字段名不匹配（已修复）
下载器 `LoginRequest.UserName` 与后端 `LoginDto.Username` 大小写不一致，导致模型校验失败。已统一为 `Username`。

### 5.2 登录响应反序列化大小写问题（已修复，核心问题）
后端返回 camelCase（`data.accessToken`），而下载器 `JsonSerializer.Deserialize<LoginResponse>(body)` 默认区分大小写，导致 `AccessToken` 解析为空，登录看似"成功"却拿不到 token。
**修复**：新增静态 `JsonSerializerOptions` 设置 `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`。

### 5.3 SignalR 连接失败（远程反向代理配置问题，待用户处理）
登录成功后，SignalR 客户端请求 `{DfAppUrl}/hubs/download-notification/negotiate` 时，远程 `cc.bdbfbp.top` 返回 HTML（以 `<` 开头），导致 `Invalid negotiation response received`。

**根因**：远程反向代理（nginx 等）未把 `/hubs/*` 路径正确代理到后端 ASP.NET Core 应用（可能被前端 SPA fallback 拦截返回 index.html，或缺少 WebSocket 升级配置）。后端 hub 路径本身正确：`DownloadNotificationHub.HubUrl = "/hubs/download-notification"`。

**处理建议**：远程 nginx 需为 `/hubs/` 配置反向代理并启用 WebSocket：
```nginx
location /hubs/ {
    proxy_pass http://backend;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
}
```
代码侧已确保该错误通过 `lastError` 暴露到前端（`SignalR 连接失败: Invalid negotiation response received.`），无需重启即可在仪表盘看到。

### 5.4 连接错误透传链路
- 登录失败：`LoginAsync` 设置 `LastConnectionError`（区分凭证错误 / 限流 / 网络错误）。
- SignalR 失败：`StartAsync` catch 设置 `LastConnectionError`。
- `DownloadManager.GetStatus()` 通过强类型 `DownloaderStatus.LastError` 暴露。
- 前端 `Dashboard.vue` 用 `el-alert` 显示，并提供"重新连接"按钮触发 `POST /api/connection/reconnect`。

