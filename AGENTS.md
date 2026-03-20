# AGENTS.md

本文件为在此仓库中工作的开发者（或 AI 助手）提供操作与约束说明。

## 项目概览

这是一个全栈彩票管理 Web 应用：
- **后端**：基于 ASP.NET Core 10.0 与 ABP Framework（领域驱动设计的单体应用）
- **前端**：Vue 3 + Element Plus 管理后台（Pure Admin Thin 模板）
- **附加服务**：用于访问中国福利彩票网站的 Lottery proxy 服务（运行在端口 5000）

## 架构

### 后端结构（ABP 分层架构）
- `src/DFApp.Domain` - 实体、领域服务
- `src/DFApp.Application` - 应用服务、DTO（控制器由此自动生成）
- `src/DFApp.EntityFrameworkCore` - EF Core 数据访问
- `src/DFApp.Web` - ASP.NET Core MVC Web 应用（用于托管前端静态文件）
- `src/DFApp.HttpApi` - HTTP API 层（自动生成）
- `src/DFApp.Application.Contracts` - 服务契约与 DTO
- `src/DFApp.DbMigrator` - 数据库迁移控制台应用
- `test/` - xUnit 测试项目

### 前端结构（Vue 3）
- `src/views/` - 页面组件
- `src/layout/` - 布局组件
- `src/components/` - 可复用组件
- `src/style/` - 全局样式（Tailwind CSS）

### 关键集成点
- 前端通过 Vite 代理将 API 请求代理到后端（`/api` → `VITE_API_BASE_URL`）
- 使用 JWT Bearer 进行认证
- 使用 SignalR 提供实时功能（`@microsoft/signalr`）
- 使用 SQLite 数据库（后端根目录的 `DFApp.db`）
- 彩票数据通过代理服务（端口 5000）从 `https://www.cwl.gov.cn` 获取
- 运行dotnet命令时应当在/home/df/dfapp/DFApp下面
- 运行pnpm命令时应当在/home/df/dfapp/DFApp.Vue下面
- 前端的端口是8848
- 后端的端口是44369

## 重要约束

### 被禁止的操作
- **不要在** `src/DFApp.HttpApi` 或 `src/DFApp.Web` 目录中添加控制器（controllers）
- **不要添加** Razor 页面（`.cshtml` 文件）
- **不要执行** ef迁移数据库命令

### 必须遵循的模式
- 控制器由 `src/DFApp.Application` 中的应用服务自动生成：
  - 继承自 `DFAppAppService`、`ApplicationService` 或 `CrudAppService` 的服务将自动生成控制器
  - 非特殊要求不允许自定义 Repository
  - 只读查询操作使用 `IReadOnlyRepository<TEntity>` 或 `IReadOnlyRepository<TEntity, TKey>`
  - 需要修改的操作使用 `IRepository<TEntity>` 或 `IRepository<TEntity, TKey>`
  - 优先使用 ABP 提供的 `IRepository` 和 `IReadOnlyRepository`
- 前端页面位于 `DFApp.Vue` 项目中
- 数据库操作在application层进行，使用linq查询表达式配合GetQueryableAsync和AsyncExecuter
- 所有数据库修改生成sql文件

### 代码注释
- **注释语言**: 所有注释必须使用中文
- **注释原则**: 非必要不添加注释，代码本身应足够清晰
- **注释维护**: 当注释与代码逻辑不符合时，必须修改注释使其符合代码逻辑
- **注释时机**: 仅在代码逻辑复杂、业务逻辑特殊或需要特别说明的情况下添加注释

## 文档管理

- 前端文档在DFApp.Vue/docs
- 后端文档在DFApp/docs
- 每次修改模块时检查是否存在文件，存在读取
- 每次修改对应模块时要更新内容到文档
- 缺失的文件在修改时添加