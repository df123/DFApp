# DFApp Media Downloader — 设计文档

**日期**: 2026-05-10
**状态**: 已批准

## 概述

新增一个独立的子程序 `DFApp.Downloader`，用于自动从 DFApp 下载已完成的 Telegram 媒体文件和 Aria2 订阅下载的媒体文件到本地 Windows 电脑。

文件通过 Apache HTTP 下载服务器获取。DFApp 侧已有 `ExternalLinkService` 生成 Apache 可访问的 URL（配置项 `ReturnDownloadUrlPrefix`），Downloader 复用此 URL 体系进行下载。

## 需求

1. **实时同步**：DFApp 侧 Telegram 媒体下载完成或 Aria2 下载完成时，自动通知子程序
2. **多线程下载**：支持将文件分片并行下载，提高下载速度
3. **断点续传**：下载中断后可从上次位置继续，无需重新下载
4. **进度查看**：支持查看每个下载任务的进度、速度、剩余时间
5. **队列管理**：支持查看、暂停、恢复、取消下载任务
6. **保存位置**：支持配置文件下载保存路径
7. **系统托盘**：Windows 系统托盘常驻运行
8. **Apache 认证**：文件通过 Apache HTTP 下载服务器获取，支持 Basic Auth

## 架构

### 方案选择

**SignalR 实时推送 + 独立 WinForms 托盘应用 + 内嵌 Kestrel Web 服务器**

选择理由：
- 与 DFApp 现有 SignalR Hub（`Aria2Hub`，路径 `/hubs/aria2`）架构一致
- 实时性好，下载完成即推送
- 技术栈一致（.NET C#，SqlSugar ORM）

### 技术栈对齐

DFApp 后端使用以下技术，Downloader 需保持一致：
- ASP.NET Core 10.0
- SqlSugar ORM + SQLite
- JWT Bearer 认证
- SignalR 实时通信
- Serilog 日志

### 整体数据流

```
DFApp 后端（src/DFApp.Web/）
  │
  ├── ListenTelegramService.DownloadMediaAsync
  │     │ mediaInfo.IsDownloadCompleted = true
  │     └── IHubContext<DownloadNotificationHub>.PushMediaCompleted()
  │
  └── Aria2Manager.DownloadCompleteHandlerAsync
        │ SaveTellStatusResultFromDtoAsync 完成后
        └── IHubContext<DownloadNotificationHub>.PushAria2Completed()
              │
              ▼
        DownloadNotificationHub (SignalR, /hubs/download-notification)
              │ JWT 认证
              ▼
        DFApp.Downloader (独立子程序)
        ├── SignalR 客户端接收通知
        ├── 通过 DFApp API 查询文件详情（Apache URL）
        ├── 加入本地下载队列（SQLite）
        └── 多线程下载引擎
              ├── HttpClient GET + Range header
              ├── Apache Basic Auth
              └── 写入本地文件系统
```

### 项目结构

```
DFApp/                              ← 仓库根目录
├── src/DFApp.Web/                  ← 后端项目（需要修改）
│   ├── Hubs/
│   │   ├── Aria2Hub.cs             ← 已有
│   │   └── DownloadNotificationHub.cs  ← 新增
│   ├── DTOs/
│   │   └── Media/
│   │       └── DownloadNotificationDto.cs  ← 新增
│   └── Services/
│       ├── Media/
│       │   ├── MediaInfoService.cs        ← 修改：添加推送逻辑
│       │   └── ExternalLinkService.cs     ← 已有：URL 生成
│       └── Aria2/
│           └── Aria2Manager.cs            ← 修改：添加推送逻辑
├── DFApp.LotteryProxy/             ← 彩票代理服务（不改动）
├── DFApp.Downloader/               ← 新增：下载子程序
│   ├── DFApp.Downloader.sln
│   ├── src/
│   │   ├── DFApp.Downloader.Core/      ← 核心库（下载引擎、SignalR 客户端）
│   │   └── DFApp.Downloader.App/       ← WinForms 托盘应用
│   └── web/                         ← Web 管理界面（Vue 3）
├── test/DFApp.Web.Tests/           ← 单元测试
├── client/                         ← 前端项目（不改动）
├── docs/
└── sql/                            ← 数据库变更脚本
```

## DFApp 侧改动

### 1. 新建 SignalR Hub：DownloadNotificationHub

路径：`src/DFApp.Web/Hubs/DownloadNotificationHub.cs`
URL：`/hubs/download-notification`
认证方式：JWT Bearer（与 Aria2Hub 一致）

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DFApp.Web.Hubs;

[Authorize]
public class DownloadNotificationHub : Hub
{
    public const string HubUrl = "/hubs/download-notification";

    /// <summary>
    /// 加入下载通知组
    /// </summary>
    public async Task JoinDownloadGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "DownloadNotify");
    }

    /// <summary>
    /// 离开下载通知组
    /// </summary>
    public async Task LeaveDownloadGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "DownloadNotify");
    }
}
```

在 `Program.cs` 中注册：

```csharp
app.MapHub<DownloadNotificationHub>(DownloadNotificationHub.HubUrl);
```

### 2. 事件数据结构

路径：`src/DFApp.Web/DTOs/Media/DownloadNotificationDto.cs`

```csharp
namespace DFApp.Web.DTOs.Media;

/// <summary>
/// 下载通知基类
/// </summary>
public class DownloadNotificationDto
{
    /// <summary>文件名</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long FileSize { get; set; }

    /// <summary>MIME 类型</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Apache 下载链接（由 ReturnDownloadUrlPrefix 生成）</summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>来源类型：Telegram | Aria2</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>原始记录 ID（MediaInfo.Id 或 TellStatusResult.Id）</summary>
    public long SourceId { get; set; }

    /// <summary>完成时间</summary>
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Telegram 媒体下载通知
/// </summary>
public class MediaDownloadNotificationDto : DownloadNotificationDto
{
    /// <summary>聊天 ID</summary>
    public long ChatId { get; set; }

    /// <summary>聊天标题</summary>
    public string ChatTitle { get; set; } = string.Empty;
}

/// <summary>
/// Aria2 下载通知
/// </summary>
public class Aria2DownloadNotificationDto : DownloadNotificationDto
{
    /// <summary>Aria2 GID</summary>
    public string Gid { get; set; } = string.Empty;

    /// <summary>种子名称</summary>
    public string TorrentName { get; set; } = string.Empty;
}
```

### 3. 推送触发点

#### 3.1 Telegram 媒体下载完成

修改文件：`src/DFApp.Web/Background/ListenTelegramService.cs`

在 `DownloadMediaAsync` 方法中，`UpdateIsDownloadCompletedAsync` 调用成功后，注入 `IHubContext<DownloadNotificationHub>` 并推送通知。

```csharp
// 在 DownloadMediaAsync 中，UpdateIsDownloadCompletedAsync 之后添加：
var downloaderEnabled = await GetConfigurationInfoAsync("DownloaderEnabled");
if (bool.TryParse(downloaderEnabled, out var enabled) && enabled)
{
    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DownloadNotificationHub>>();
    var returnDownloadUrlPrefix = await GetConfigurationInfoAsync("ReturnDownloadUrlPrefix");
    var replaceUrlPrefix = await GetConfigurationInfoAsync("ReplaceUrlPrefix");
    var downloadUrl = $"{Path.Combine(returnDownloadUrlPrefix,
        mediaInfo.SavePath.Replace(replaceUrlPrefix, string.Empty).Replace("\\", "/"))}";

    var notification = new MediaDownloadNotificationDto
    {
        FileName = Path.GetFileName(mediaInfo.SavePath),
        FileSize = mediaInfo.Size,
        MimeType = mediaInfo.MimeType,
        DownloadUrl = downloadUrl,
        SourceType = "Telegram",
        SourceId = mediaInfo.Id,
        ChatId = mediaInfo.ChatId,
        ChatTitle = mediaInfo.ChatTitle,
        CompletedAt = DateTime.UtcNow
    };
    await hubContext.Clients.Group("DownloadNotify")
        .SendAsync("DownloadCompleted", notification);
}
```

**URL 构建逻辑**：复用现有 `ExternalLinkService` 中的 `ReturnDownloadUrlPrefix` 配置，将本地路径转换为 Apache URL。

#### 3.2 Aria2 下载完成

修改文件：`src/DFApp.Web/Services/Aria2/Aria2Manager.cs`

在 `SaveTellStatusResultFromDtoAsync` 方法末尾，推送 Aria2 下载完成通知。

**Aria2 URL 映射规则**：Aria2 下载的文件存储在 `taskDto.Dir` 目录下。需要在 Apache 配置中将 Aria2 下载目录映射为虚拟路径（如 `/aria2/`），或在 DFApp 配置中新增 `Aria2ApachePathPrefix` 配置项，用于将本地路径转换为 Apache URL。

```csharp
// 在 SaveTellStatusResultFromDtoAsync 末尾添加：
var downloaderEnabled = await configurationInfoRepository
    .GetConfigurationInfoValue("DownloaderEnabled", MediaBackgroudConst.ModuleName);
if (!bool.TryParse(downloaderEnabled, out var enabled) || !enabled)
{
    return;  // Downloader 推送未启用
}

var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<DownloadNotificationHub>>();
var filePath = taskDto.Files.FirstOrDefault()?.Path ?? "";
var fileName = Path.GetFileName(filePath);

// 构建 Apache URL：Aria2 下载目录 → Apache 虚拟路径
var aria2ApachePrefix = await configurationInfoRepository
    .GetConfigurationInfoValue("Aria2ApachePathPrefix", MediaBackgroudConst.ModuleName);
var downloadUrl = $"{aria2ApachePrefix}/{fileName}";

var notification = new Aria2DownloadNotificationDto
{
    FileName = fileName,
    FileSize = taskDto.TotalLength,
    MimeType = "application/octet-stream",
    DownloadUrl = downloadUrl,
    SourceType = "Aria2",
    SourceId = tellStatusResult.Id,
    Gid = taskDto.Gid,
    TorrentName = Path.GetFileName(taskDto.Dir),
    CompletedAt = DateTime.UtcNow
};
await hubContext.Clients.Group("DownloadNotify")
    .SendAsync("DownloadCompleted", notification);
```

### 4. 配置新增

在 `ConfigurationInfo` 表中新增以下配置项（模块名：`DFApp.Media`）：

| 配置项 | 说明 | 示例值 |
|--------|------|--------|
| `ApacheBaseUrl` | Apache 下载服务器基础 URL | `http://192.168.1.100:8080` |
| `ApacheUsername` | Apache Basic Auth 用户名 | `admin` |
| `ApachePassword` | Apache Basic Auth 密码 | `***` |
| `DownloaderEnabled` | 是否启用 Downloader 推送 | `true` |
| `Aria2ApachePathPrefix` | Aria2 下载目录的 Apache 虚拟路径 | `http://192.168.1.100:8080/aria2` |

**注意**：
- 现有的 `ReturnDownloadUrlPrefix` 配置已包含 Telegram 媒体的 Apache URL 前缀
- 新增的 `Aria2ApachePathPrefix` 用于 Aria2 下载文件的 Apache URL 映射
- `DownloaderEnabled` 控制是否向 Downloader 子程序推送通知

### 5. 媒体外链 URL 映射

现有 `ExternalLinkService` 已实现本地路径到 Apache URL 的转换逻辑：

```csharp
// ExternalLinkService.cs 第168行
$"{Path.Combine(returnDownloadUrlPrefix, mediaInfo.SavePath.Replace(replaceUrlPrefix, string.Empty).Replace("\\", "/"))}"
```

Downloader 子程序需要：
1. 通过 DFApp API 或 SignalR 通知获取 `DownloadUrl`（已转换为 Apache URL）
2. 直接使用该 URL 进行 HTTP 下载

## DFApp.Downloader 子程序

### 核心库（DFApp.Downloader.Core）

#### 配置模型

```csharp
public class DownloaderSettings
{
    // DFApp 连接
    public string DfAppUrl { get; set; } = "http://localhost:44369";
    public string DfAppUsername { get; set; } = string.Empty;
    public string DfAppPassword { get; set; } = string.Empty;

    // Apache 下载服务器（从 DFApp 配置同步或手动设置）
    public string ApacheBaseUrl { get; set; } = string.Empty;
    public string ApacheUsername { get; set; } = string.Empty;
    public string ApachePassword { get; set; } = string.Empty;

    // 下载配置
    public string DownloadPath { get; set; } = @"%USERPROFILE%\Downloads\DFApp";
    public int MaxConcurrentDownloads { get; set; } = 3;
    public int MaxSegmentsPerFile { get; set; } = 4;
    public long SegmentSize { get; set; } = 4 * 1024 * 1024; // 4MB

    // Web 服务器
    public int WebServerPort { get; set; } = 9550;

    // 开机自启
    public bool AutoStart { get; set; } = false;
}
```

#### SignalR 客户端

```csharp
public class DownloadNotificationClient
{
    private HubConnection _connection;

    public event Action<DownloadNotificationDto>? OnDownloadCompleted;

    public async Task StartAsync(string dfAppUrl, string jwtToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{dfAppUrl}{DownloadNotificationHub.HubUrl}", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(jwtToken);
            })
            .WithAutomaticReconnect(new[] {
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        _connection.On<MediaDownloadNotificationDto>("DownloadCompleted", notification =>
        {
            OnDownloadCompleted?.Invoke(notification);
        });

        _connection.Reconnected += async _ =>
        {
            // 重新加入通知组
            await _connection.SendAsync("JoinDownloadGroup");
        };

        await _connection.StartAsync();
        await _connection.SendAsync("JoinDownloadGroup");
    }
}
```

#### JWT 认证流程

Downloader 通过 DFApp 的 `/api/account/login` 接口获取 JWT Token：
1. 使用配置的 `DfAppUsername` / `DfAppPassword` 登录
2. 保存 Token，过期前自动刷新
3. SignalR 连接和 API 调用均使用此 Token

#### 下载引擎

```
DownloadNotificationClient (SignalR)
       │ 收到通知
       ▼
  DownloadQueue (ConcurrentQueue + SemaphoreSlim 控制并发)
       │ 出队
       ▼
  DownloadEngine (调度器)
       │ 检查 Apache 是否支持 Range（HEAD 请求）
       ▼
  SegmentDownloader (分段下载器)
       ├── 分片1 → HttpClient.GetAsync(url, Range header)
       ├── 分片2 → HttpClient.GetAsync(url, Range header)
       ├── ...
       │ 写入同一文件的不同偏移位置
       ▼
  本地文件系统
```

**多线程下载**：
- 文件按 SegmentSize 分片
- 每个分片独立 HTTP Range 请求（`Range: bytes=start-end`）
- 最大并发数由 MaxSegmentsPerFile 控制
- 小文件（< SegmentSize）单线程下载

**断点续传**：
- 每个分片的已下载字节数保存到 SQLite
- 程序重启后检查未完成任务（Status = "Downloading"），自动恢复
- 利用 HTTP Range 请求头续传

**Apache Basic Auth**：
- `HttpClientHandler` 配置 `NetworkCredential`
- 设置页面可配置用户名密码

**Range 不支持回退**：
- 首次下载前发送 HEAD 请求检查 `Accept-Ranges` 头
- 若返回 416 或不支持 Range，回退为单线程下载

#### 本地数据库（SQLite + SqlSugar）

```csharp
[SugarTable("DownloadItems")]
public class DownloadItem
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>来源类型：Telegram | Aria2</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>原始记录 ID</summary>
    public long SourceId { get; set; }

    /// <summary>文件名</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long FileSize { get; set; }

    /// <summary>Apache 下载链接</summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>本地保存路径</summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>状态：Pending|Downloading|Paused|Completed|Failed</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>已下载字节数</summary>
    public long DownloadedBytes { get; set; }

    /// <summary>MIME 类型</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>聊天标题（Telegram 来源）</summary>
    public string? ChatTitle { get; set; }

    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

[SugarTable("DownloadSegments")]
public class DownloadSegment
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long DownloadItemId { get; set; }
    public int SegmentIndex { get; set; }
    public long StartOffset { get; set; }
    public long EndOffset { get; set; }
    public long DownloadedBytes { get; set; }

    /// <summary>状态：Pending|Downloading|Completed|Failed</summary>
    public string Status { get; set; } = "Pending";
}
```

### 应用层（DFApp.Downloader.App）

#### 系统托盘
- 右键菜单：打开管理界面、暂停所有、恢复所有、退出
- 图标提示显示状态（已连接/未连接、活跃下载数）
- 下载完成时 Windows 系统通知（`NotifyIcon.ShowBalloonTip`）

#### Web 服务器（Kestrel）
端口：9550（可配置）

启动流程：
1. 初始化 SqlSugar + SQLite（本地数据库）
2. 初始化 HttpClient（配置 Basic Auth）
3. 初始化 SignalR 客户端连接 DFApp
4. 启动 Kestrel Web 服务器
5. 加载系统托盘

API 端点：

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/downloads` | 下载列表（分页，支持状态过滤） |
| GET | `/api/downloads/{id}` | 下载详情 |
| GET | `/api/downloads/active` | 活跃下载（Status=Downloading） |
| GET | `/api/downloads/queue` | 等待队列（Status=Pending） |
| POST | `/api/downloads/{id}/pause` | 暂停 |
| POST | `/api/downloads/{id}/resume` | 恢复 |
| DELETE | `/api/downloads/{id}` | 取消/删除 |
| GET | `/api/settings` | 获取设置 |
| PUT | `/api/settings` | 更新设置 |
| GET | `/api/status` | 全局状态（连接数、下载速度、磁盘空间） |
| GET | `/api/connection` | SignalR 连接状态 |

### Web 管理界面

技术栈：Vue 3 + Element Plus + Vite

```
web/
├── src/
│   ├── views/
│   │   ├── Dashboard.vue            ← 仪表盘（统计、连接状态、下载速度图表）
│   │   ├── DownloadQueue.vue        ← 下载队列（进度条、操作按钮、筛选）
│   │   └── Settings.vue             ← 设置页面（DFApp 连接、Apache 配置、下载路径）
│   ├── components/
│   │   ├── DownloadProgress.vue     ← 进度条组件（百分比、速度、剩余时间）
│   │   └── TaskList.vue             ← 任务列表组件（表格、分页）
│   ├── api/
│   │   └── downloader.ts            ← API 封装（axios）
│   ├── App.vue
│   ├── main.ts
│   └── router/
│       └── index.ts
├── index.html
├── package.json
├── vite.config.ts
└── tsconfig.json
```

## 错误处理

### 连接管理
- SignalR 内置自动重连（指数退避：0s → 2s → 5s → 10s → 30s）
- 断线时暂停下载队列，重连后自动恢复
- 重连后主动查询 DFApp 获取断线期间遗漏的已完成文件列表
- JWT Token 过期前自动刷新（通过 `/api/account/refresh` 接口）

### 下载容错
- Apache 不支持 Range（416 响应）时回退为单线程下载
- 网络中断：自动重试 3 次（间隔 5s、10s、30s）后标记 Failed
- 磁盘空间不足：暂停队列并弹出系统通知
- 同名文件：自动追加序号（如 `file(1).mp4`）
- HTTP 401/403：提示检查 Apache 认证配置

### 日志
- 使用 Serilog，日志文件位于 `%APPDATA%\DFApp.Downloader\logs\`
- 记录下载开始/完成/失败、连接状态变化、错误信息

### 首次启动
- 首次运行自动打开浏览器访问 `http://localhost:9550`
- 引导式配置流程：
  1. DFApp 地址（默认 `http://localhost:44369`）
  2. 登录凭证（用户名/密码）
  3. Apache 地址和凭证（可从 DFApp 自动同步）
  4. 保存路径（默认 `%USERPROFILE%\Downloads\DFApp`）

## 运行环境

- 目标框架：`net10.0-windows`（需要 WinForms 和系统托盘支持）
- 发布方式：单文件发布（`PublishSingleFile=true`，`SelfContained=false`）
- 支持开机自启动（通过注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`）
- 默认端口：Web 管理 9550
- 最低系统要求：Windows 10

## 与现有功能的关系

### ExternalLinkService 对比

| 特性 | ExternalLinkService（现有） | Downloader（新增） |
|------|---------------------------|-------------------|
| 目的 | 生成 Apache URL 供外部访问 | 自动下载到本地 Windows 电脑 |
| 触发方式 | 手动/定时任务 | 实时（SignalR 推送） |
| 下载方式 | 用户手动点击链接 | 多线程自动下载 |
| 断点续传 | 不支持 | 支持 |
| 文件格式 | 可打包为 ZIP | 原始文件 |

两者互补：ExternalLinkService 负责 URL 生成，Downloader 负责自动拉取。

### 配置复用

Downloader 复用 DFApp 中已有的配置：
- `ReturnDownloadUrlPrefix`：Apache URL 前缀（用于构建下载链接）
- `ReplaceUrlPrefix`：本地路径替换前缀（用于路径映射）
- `SaveVideoPathPrefix` / `SavePhotoPathPrefix`：文件保存目录（用于判断文件是否存在）

## 实施步骤概要

1. **Phase 1**：DFApp 侧改动
   - 创建 `DownloadNotificationHub`（Hub + 注册）
   - 创建 `DownloadNotificationDto`（DTO 类）
   - 修改 `ListenTelegramService` 添加推送逻辑
   - 修改 `Aria2Manager` 添加推送逻辑
   - 添加 Apache 相关配置项
   - 编写单元测试

2. **Phase 2**：DFApp.Downloader.Core
   - SignalR 客户端（连接、认证、重连）
   - 下载引擎（分段下载、断点续传）
   - 队列管理（并发控制、优先级）
   - 本地 SQLite 数据库（SqlSugar）
   - 配置管理（JSON 文件）

3. **Phase 3**：DFApp.Downloader.App
   - WinForms 托盘应用（系统托盘、通知）
   - Kestrel Web 服务器（API 端点）
   - 首次启动引导流程

4. **Phase 4**：Web 管理界面
   - Dashboard 页面（统计、状态）
   - 下载队列页面（列表、进度、操作）
   - 设置页面（连接配置、下载配置）

5. **Phase 5**：集成测试和优化
   - 端到端测试（Telegram → DFApp → Downloader → 本地文件）
   - 断线重连测试
   - 大文件下载测试
   - 性能优化
