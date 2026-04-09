# AGENTS.md

本文件为在此仓库中工作的开发者（或 AI 助手）提供操作与约束说明。

## 项目概览

这是一个多功能 Web 应用（Monorepo 结构）：
- **后端**：基于 ASP.NET Core 10.0 的轻量级单体应用
- **前端**：Vue 3 + Element Plus 管理后台（Pure Admin Thin 模板），位于 `client/` 目录
- **附加服务**：Lottery proxy 服务（端口 5000），用于访问中国福利彩票网站
- **ORM**：SqlSugar（已替代 EF Core）
- **解决方案**：包含 3 个项目 — DFApp.Web、DFApp.LotteryProxy、DFApp.Web.Tests

## 技术栈

### 后端
- ASP.NET Core 10.0
- SqlSugar ORM + SQLite
- JWT Bearer 认证
- Quartz.NET 定时任务
- SignalR 实时通信
- Mapperly 对象映射
- Serilog 日志
- Swagger API 文档

### 前端
- Vue 3（Composition API）
- Element Plus UI 组件库
- Pure Admin Thin 管理后台模板
- Pinia 状态管理
- Vue Router 路由
- Vite 构建工具
- Tailwind CSS 样式
- @microsoft/signalr 实时通信客户端
- Playwright E2E 测试

## 已完成迁移

项目已完成从 ABP Framework 到轻量级 ASP.NET Core 的全面迁移（Phase 1-9）。迁移详情参见：
- `docs/framework-migration-plan.md` — 迁移总计划
- `docs/framework-migration-summary-phase-1~9.md` — 各阶段迁移总结

## 架构

### Monorepo 目录结构
```
DFApp/                          ← 仓库根目录
├── AGENTS.md                   ← 本文件
├── DFApp.Web/                  ← 后端项目
├── DFApp.LotteryProxy/         ← 彩票代理服务
├── test/DFApp.Web.Tests/       ← 单元测试
├── client/                     ← 前端项目（Vue 3）
├── docs/                       ← 后端文档
└── sql/                        ← 数据库变更脚本
```

### 后端结构（轻量级单体架构）
- `DFApp.Web/` ← 唯一后端项目
  - `Domain/` - 实体和自定义基类
  - `Services/` - 应用服务
  - `Controllers/` - API 控制器（路由模式：`/api/app/{kebab-case-entity}`）
  - `DTOs/` - 数据传输对象
  - `Permissions/` - 权限定义与授权处理器
  - `Background/` - Quartz.NET 后台任务
  - `Hubs/` - SignalR Hub
  - `Mapping/` - Mapperly 映射器
  - `Data/` - SqlSugar 配置与仓储
  - `Infrastructure/` - 中间件、过滤器、异常处理、密码哈希
  - `Utilities/` - 工具类
- `DFApp.LotteryProxy/` ← 彩票代理服务（端口 5000）
- `test/DFApp.Web.Tests/` ← 单元测试

### 前端结构（Vue 3）
- `client/src/views/` - 页面组件
- `client/src/layout/` - 布局组件
- `client/src/components/` - 可复用组件
- `client/src/style/` - 全局样式（Tailwind CSS）
- `client/src/store/` - Pinia 状态管理
- `client/src/router/` - Vue Router 路由配置
- `client/src/api/` - API 请求封装
- `client/src/utils/` - 工具函数

### 关键集成点
- 前端通过 Vite 代理将 API 请求代理到后端（`/api` → `VITE_API_BASE_URL`）
- 使用 JWT Bearer 进行认证
- 使用 SignalR 提供实时功能（`@microsoft/signalr`）
- 使用 SQLite 数据库（后端根目录的 `DFApp.db`）
- 彩票数据通过代理服务（端口 5000）从 `https://www.cwl.gov.cn` 获取
- 运行 dotnet 命令时应当在 `/home/df/dfapp/DFApp` 下面
- 运行 pnpm 命令时应当在 `/home/df/dfapp/DFApp/client` 下面
- 前端的端口是 8848
- 后端的端口是 44369
- **启动后端服务时，请务必使用 0.0.0.0 作为绑定地址**：这是因为开发环境采用 VS Code 远程开发模式，需要确保服务能够被远程访问

## 重要约束

### 被禁止的操作
- **不要添加** Razor 页面（`.cshtml` 文件）

### 必须遵循的模式
- 每个应用服务需要手动创建对应的 Controller，路由采用 `/api/app/{kebab-case-entity}` 模式
- 使用 SqlSugar 仓储（`ISqlSugarRepository` 和 `ISqlSugarReadOnlyRepository`），特殊业务需求可创建自定义仓储
- 只读查询操作使用 `ISqlSugarReadOnlyRepository<TEntity>` 或 `ISqlSugarReadOnlyRepository<TEntity, TKey>`
- 需要修改的操作使用 `ISqlSugarRepository<TEntity>` 或 `ISqlSugarRepository<TEntity, TKey>`
- 数据库操作在 Service 层进行，使用 SqlSugar 的 LINQ 表达式和 `.ToListAsync()` 等方法
- 所有数据库修改生成 sql 文件

### 代码注释
- **注释语言**: 所有注释必须使用中文
- **注释原则**: 非必要不添加注释，代码本身应足够清晰
- **注释维护**: 当注释与代码逻辑不符合时，必须修改注释使其符合代码逻辑
- **注释时机**: 仅在代码逻辑复杂、业务逻辑特殊或需要特别说明的情况下添加注释

## 文档管理

- 前端文档在 `client/docs/`
- 后端文档在 `docs/`
- 每次修改模块时检查是否存在文件，存在读取
- 每次修改对应模块时要更新内容到文档
- 缺失的文件在修改时添加

## 可用工具

### MCP 工具

在必要时，LLM 可以使用以下 MCP（Model Context Protocol）工具来辅助完成开发任务：

#### Context7
- **用途**：检索最新的编程库和框架的文档及代码示例
- **适用场景**：
  - 需要查询特定库或框架的最新文档
  - 需要查找代码示例和最佳实践
  - 需要了解某个库的 API 使用方法
- **使用方式**：通过 `resolve-library-id` 和 `query-docs` 工具获取库文档

#### GitMCP
- **用途**：从 GitHub 仓库获取文档和代码信息
- **适用场景**：
  - 需要查询特定 GitHub 仓库的文档
  - 需要在仓库代码中搜索特定功能或实现
  - 需要获取仓库中的参考资源
- **使用方式**：通过 `fetch-repo-documentation`、`search-repo-documentation`、`search-repo-code` 等工具获取信息

**使用原则**：
- 仅在需要查询外部文档或代码示例时使用
- 优先使用项目内的现有文档和代码
- 使用前确保已明确需要查询的库名称或仓库地址
