# 🚀 DFApp

多功能 Web 应用，采用 Monorepo 结构，包含 ASP.NET Core 后端与 Vue 3 前端管理后台。

## 📋 目录

- [技术栈](#技术栈)
- [解决方案结构](#解决方案结构)
- [端口配置](#端口配置)
- [快速开始](#快速开始)
- [关键集成说明](#关键集成说明)
- [迁移历史](#迁移历史)
- [文档](#文档)
- [许可证](#许可证)

## 🛠 技术栈

### 后端

| 技术 | 说明 |
|------|------|
| ASP.NET Core 10.0 | 轻量级单体架构 |
| SqlSugar ORM | 数据访问层 + SQLite 数据库 |
| JWT Bearer | 认证与授权 |
| Quartz.NET | 定时任务调度 |
| SignalR | 实时通信 |
| Mapperly | 对象映射 |
| Serilog | 结构化日志 |
| Swagger | API 文档 |
| WTelegramClient | Telegram 集成 |
| HtmlAgilityPack / AngleSharp | HTML 解析 |
| SixLabors.ImageSharp | 图像处理 |
| BencodeNET | BitTorrent 编码 |

### 前端（client/ 目录）

| 技术 | 说明 |
|------|------|
| Vue 3 | Composition API |
| Element Plus | UI 组件库 |
| Pure Admin Thin v6.1.0 | 管理后台模板 |
| Pinia | 状态管理 |
| Vue Router | 路由管理 |
| Vite 7.x | 构建工具 |
| Tailwind CSS 4.x | 样式框架 |
| TypeScript | 类型安全 |
| @microsoft/signalr | 实时通信客户端 |
| ECharts / Chart.js | 数据可视化 |
| Playwright | E2E 测试 |

### 附加服务

- **DFApp.LotteryProxy** — 彩票代理服务，用于访问中国福利彩票网站（`https://www.cwl.gov.cn`）

## 📁 解决方案结构

```
DFApp/
├── src/DFApp.Web/              ← 后端主项目（ASP.NET Core 10.0）
├── DFApp.LotteryProxy/         ← 彩票代理服务
├── test/DFApp.Web.Tests/       ← 单元测试
├── client/                     ← 前端项目（Vue 3）
├── docs/                       ← 后端文档
├── client/docs/                ← 前端文档
├── sql/                        ← 数据库变更脚本
├── plans/                      ← 计划文档
├── start.sh                    ← 一键启动脚本
├── stop.sh                     ← 一键停止脚本
├── DFApp.sln                   ← 解决方案文件
├── common.props                ← 共享 MSBuild 属性
└── AGENTS.md                   ← AI 助手操作指南
```

### 后端项目结构（src/DFApp.Web/）

```
src/DFApp.Web/
├── Domain/          ← 实体和自定义基类
├── Services/        ← 应用服务
├── Controllers/     ← API 控制器（路由：/api/app/{kebab-case-entity}）
├── DTOs/            ← 数据传输对象
├── Permissions/     ← 权限定义与授权处理器
├── Background/      ← Quartz.NET 后台任务
├── Hubs/            ← SignalR Hub
├── Mapping/         ← Mapperly 映射器
├── Data/            ← SqlSugar 配置与仓储
├── Infrastructure/  ← 中间件、过滤器、异常处理、密码哈希
└── Utilities/       ← 工具类
```

### 前端项目结构（client/src/）

```
client/src/
├── views/           ← 页面组件
├── layout/          ← 布局组件
├── components/      ← 可复用组件
├── style/           ← 全局样式（Tailwind CSS）
├── store/           ← Pinia 状态管理
├── router/          ← Vue Router 路由配置
├── api/             ← API 请求封装
└── utils/           ← 工具函数
```

## 🔌 端口配置

| 服务 | 端口 |
|------|------|
| 前端（Vue 3） | 9949 |
| 后端（ASP.NET Core） | 44369 |
| 彩票代理服务 | 5000 |

## 🏁 快速开始

### 环境要求

- **.NET** 10.0 SDK
- **Node.js** ^20.19.0 \|\| >=22.12.0
- **pnpm** >=9

### 一键启动

```bash
# 启动后端和前端
./start.sh

# 启动所有服务（含彩票代理）
./start.sh all

# 停止服务
./stop.sh
```

### 手动启动

**后端**：

```bash
cd /home/df/dfapp/DFApp
dotnet run --project src/DFApp.Web --urls "https://0.0.0.0:44369"
```

**前端**：

```bash
cd /home/df/dfapp/DFApp/client
pnpm install
pnpm dev
```

**彩票代理**：

```bash
cd /home/df/dfapp/DFApp
dotnet run --project DFApp.LotteryProxy
```

## 🔗 关键集成说明

- 前端通过 Vite 代理将 API 请求代理到后端（`/api` → 后端服务）
- 使用 JWT Bearer 进行认证
- 使用 SignalR 提供实时功能
- 使用 SQLite 数据库（后端项目目录下的 `DFApp.db`）
- 彩票数据通过代理服务从 `https://www.cwl.gov.cn` 获取

## 📜 迁移历史

项目已完成从 ABP Framework 到轻量级 ASP.NET Core 的全面迁移（Phase 1-9），迁移详情参见 `docs/` 目录中的相关文档。

## 📚 文档

| 文档类型 | 路径 |
|----------|------|
| 后端文档 | `docs/` |
| 前端文档 | `client/docs/` |
| 数据库迁移脚本 | `sql/` |
| AI 助手操作指南 | `AGENTS.md` |

## 📄 许可证

[MIT License](LICENSE) © 2023 df123
