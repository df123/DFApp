# DFApp.Downloader

媒体文件自动下载子程序，从 DFApp 下载已完成的 Telegram 媒体文件和 Aria2 下载文件到本地 Windows 电脑。

## 快速开始

### 前置条件

- .NET 10.0 SDK
- Node.js 18+ (pnpm)

### 1. 构建前端

```bash
cd DFApp.Downloader/web
pnpm install
pnpm run build
```

构建产物会自动复制到 `src/DFApp.Downloader.App/wwwroot/`（如果已存在则覆盖）。

### 2. 运行

```bash
cd DFApp.Downloader
dotnet run --project src/DFApp.Downloader.App
```

首次启动后打开浏览器访问 `http://localhost:9550` 进行配置。

### 3. 发布（Windows 单文件）

```bash
dotnet publish src/DFApp.Downloader.App -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

发布后运行 `publish/DFApp.Downloader.App.exe`。

## 项目结构

```
DFApp.Downloader/
├── DFApp.Downloader.slnx              # 解决方案
├── src/
│   ├── DFApp.Downloader.Core/         # 核心库
│   │   ├── Configuration/             # 配置模型
│   │   ├── Data/                      # SQLite 数据库
│   │   ├── Engine/                    # 下载引擎
│   │   ├── Entities/                  # 实体
│   │   ├── Models/                    # DTO
│   │   └── SignalR/                   # SignalR 客户端
│   └── DFApp.Downloader.App/          # 主应用
│       ├── Controllers/               # REST API
│       ├── Program.cs                 # 入口
│       └── wwwroot/                   # 前端静态文件（构建生成）
└── web/                               # 前端源码（Vue 3）
    ├── src/views/                     # 页面
    ├── src/api/                       # API 封装
    └── src/router/                    # 路由
```

## 开发

### 前端开发

```bash
cd DFApp.Downloader/web
pnpm run dev    # 开发服务器 http://localhost:9551
```

Vite 会将 `/api` 请求代理到 `http://localhost:9550`。

### 后端开发

```bash
cd DFApp.Downloader
dotnet run --project src/DFApp.Downloader.App
```

## API 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| GET | `/api/downloads` | 下载列表（分页） |
| GET | `/api/downloads/{id}` | 下载详情 |
| GET | `/api/downloads/active` | 活跃下载 |
| GET | `/api/downloads/queue` | 等待队列 |
| POST | `/api/downloads/{id}/pause` | 暂停 |
| POST | `/api/downloads/{id}/resume` | 恢复 |
| DELETE | `/api/downloads/{id}` | 取消/删除 |
| GET | `/api/settings` | 获取设置 |
| PUT | `/api/settings` | 更新设置 |
| GET | `/api/status` | 全局状态 |
| GET | `/api/connection` | 连接状态 |
| POST | `/api/connection/reconnect` | 重新连接 |

## 运行时文件

程序运行时会在同目录生成以下文件：

- `settings.json` — 配置文件
- `downloader.db` — SQLite 数据库
- `logs/` — 日志目录

## 工作流程

1. DFApp 后端下载 Telegram/Aria2 文件完成
2. 通过 SignalR 推送通知到 Downloader
3. Downloader 通过 Apache HTTP 下载文件到本地
4. 支持多线程分片下载、断点续传

## 端口

- Downloader Web 管理：`9550`（可配置）
- 前端开发服务器：`9551`
- DFApp 后端：`44369`
