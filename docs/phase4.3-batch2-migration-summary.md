# Phase 4.3 Batch 2 迁移摘要

## 迁移日期
2026-03-30

## 迁移范围
本次迁移了 2 个服务：`Aria2ManageService` 和 `TGLoginService`。

---

## 1. Aria2ManageService

### 文件位置
- **原文件**：`src/DFApp.Application/Aria2/Aria2ManageService.cs`
- **新文件**：`src/DFApp.Web/Services/Aria2/Aria2ManageService.cs`

### 主要变更
| 变更项 | 原代码 | 新代码 |
|--------|--------|--------|
| 基类 | `ApplicationService` (ABP) | `AppServiceBase` |
| 命名空间 | `DFApp.Aria2` | `DFApp.Web.Services.Aria2` |
| 授权特性 | `[Authorize(DFAppPermissions.Aria2.Default)]` | 已移除 |
| 日志 | `Logger.LogError(...)` (ABP内置) | `_logger.LogError(...)` (注入 `ILogger<T>`) |
| 异常类型 | `ArgumentException` | `BusinessException` |
| 仓储 | `IConfigurationInfoRepository` (ABP版) | `IConfigurationInfoRepository` (`DFApp.Web.Data.Configuration`) |
| 构造函数 | 3个参数 | 6个参数（增加 `ICurrentUser`, `IPermissionChecker`, `ILogger`） |

### 迁移的方法（18个）
- `GetGlobalStatAsync` - 获取全局状态
- `GetActiveTasksAsync` - 获取活跃任务
- `GetWaitingTasksAsync` - 获取等待任务
- `GetStoppedTasksAsync` - 获取停止任务
- `GetTaskStatusAsync` - 获取任务状态
- `GetTaskDetailAsync` - 获取任务详情
- `AddUriAsync` - 添加 URI 下载
- `BatchAddUriAsync` - 批量添加 URI 下载
- `AddTorrentAsync` - 添加种子下载
- `BatchAddTorrentAsync` - 批量添加种子下载
- `PauseTasksAsync` - 暂停任务
- `PauseAllTasksAsync` - 暂停所有任务
- `UnpauseTasksAsync` - 恢复任务
- `UnpauseAllTasksAsync` - 恢复所有任务
- `StopTasksAsync` - 停止任务
- `RemoveTasksAsync` - 删除任务
- `PurgeDownloadResultAsync` - 清空停止任务
- `GetConnectionStatusAsync` - 获取连接状态
- `GetIpGeolocationAsync` - 获取 IP 地理位置

### 未迁移的依赖
- **`Aria2RpcClient`**：未迁移，已用 `// TODO: Aria2RpcClient 未迁移` 标注，注入保留但编译时会报错

### 优化
- `ArgumentException` → `BusinessException`，统一异常处理
- 所有方法添加了中文 XML 文档注释

---

## 2. TGLoginService

### 文件位置
- **原文件**：`src/DFApp.Application/TG/Login/TGLoginService.cs`
- **新文件**：`src/DFApp.Web/Services/TG/TGLoginService.cs`

### 主要变更
| 变更项 | 原代码 | 新代码 |
|--------|--------|--------|
| 基类 | `ApplicationService` (ABP) | `AppServiceBase` |
| 命名空间 | `DFApp.TG.Login` | `DFApp.Web.Services.TG` |
| 授权特性 | `[Authorize(DFAppPermissions.Medias.Default)]` | 已移除 |
| 构造函数 | 1个参数（`IServiceProvider`） | 3个参数（增加 `ICurrentUser`, `IPermissionChecker`） |

### 迁移的方法（3个）
- `Status()` - 获取登录状态
- `Config(string value)` - 配置登录
- `Chats()` - 获取聊天列表

### 未迁移的依赖
- **`ListenTelegramService`**：未迁移，已用 `// TODO: ListenTelegramService 未迁移` 标注，编译时会报错

### 优化
- 所有方法添加了中文 XML 文档注释

---

## 编译状态
两个服务均存在编译错误，原因是依赖的 `Aria2RpcClient` 和 `ListenTelegramService` 尚未迁移。根据迁移规则 #7，这些编译错误将在后续批次中解决。

## 后续工作
1. 迁移 `Aria2RpcClient` 以解决 `Aria2ManageService` 的编译错误
2. 迁移 `ListenTelegramService` 以解决 `TGLoginService` 的编译错误
3. 为两个服务创建对应的 Controller
