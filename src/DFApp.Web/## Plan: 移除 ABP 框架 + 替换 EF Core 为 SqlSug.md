## Plan: 移除 ABP 框架 + 替换 EF Core 为 SqlSugar + 合并 csproj

将当前基于 ABP Framework 10.0.1 的 DDD 分层架构（7个 csproj、44+ ABP NuGet 包、EF Core），**一次性重写**为单一 csproj 的轻量 ASP.NET Core 10.0 项目，使用 SqlSugar ORM、传统 Controller、直接 Quartz.NET 调度，保留完整的 RBAC 权限系统和现有数据库数据。

---

### 当前架构概况

| 维度 | 现状 |
|------|------|
| 项目数 | 7 个 csproj |
| ABP 包数 | 44+ 个 NuGet 包 |
| 实体数 | 25+ 自定义实体 + ABP 系统表 |
| 应用服务数 | 34 个（16 CrudAppService + 15 ApplicationService + 3 其他） |
| 权限项 | 10 组 80+ 权限定义 |
| 后台任务 | 4 个 HostedService + 3 个 Quartz 定时任务 |
| 自定义控制器 | 4 个（Aria2, FileUpload, FileDownload, LogViewer） |
| DTO 映射 | Mapperly（30+ 映射器，基于 ABP MapperBase 封装） |
| 数据库 | SQLite（DFApp.db） |

---

### Steps

#### Phase 1: 新项目搭建与基础设施（无依赖）

**1.1 创建新的单一项目结构** — 在 DFApp.Web 上重构为唯一项目，新建目录：
```
DFApp.Web/
  Domain/              ← 实体（原 Domain + Domain.Shared）
  Services/            ← 应用服务（原 Application）
  Controllers/         ← API 控制器（新写）
  DTOs/                ← DTO（原 Application.Contracts）
  Permissions/         ← 权限定义
  Background/          ← 后台任务
  Hubs/                ← SignalR
  Mapping/             ← Mapperly 映射器
  Data/                ← SqlSugar 配置
  Infrastructure/      ← 中间件、过滤器
```

**1.2 配置新的 csproj** — 移除全部 44+ ABP 包，添加：
- `SqlSugarCore`、`Quartz` + `Quartz.Extensions.Hosting`、`Microsoft.AspNetCore.Authentication.JwtBearer`、`Swashbuckle.AspNetCore`、`Serilog.AspNetCore`、`Riok.Mapperly`、`WTelegram` 等

**1.3 重写 Program.cs** — 移除 ABP 模块系统 + Autofac，使用原生 ASP.NET Core DI，配置 JWT/CORS/Serilog/Swagger/SignalR/SqlSugar

**1.4 配置 SqlSugar** — 连接 SQLite，AOP 自动填充审计字段，全局软删除过滤器，CreatorId 数据过滤器

---

#### Phase 2: 实体层迁移（*依赖 Phase 1*）

**2.1 创建自定义实体基类** — 替代 ABP 的 `AuditedAggregateRoot`、`Entity`、`FullAuditedAggregateRoot`、`CreationAuditedAggregateRoot`：
- `EntityBase<TKey>` — Id + ConcurrencyStamp
- `AuditedEntity<TKey>` — + CreationTime, LastModificationTime, CreatorId, LastModifierId
- `FullAuditedEntity<TKey>` — + IsDeleted, DeletionTime, DeleterId
- `CreationAuditedEntity<TKey>` — + CreationTime, CreatorId
- 保留 `ISoftDelete`、`ICreatorId`、`IHasCreationTime` 等接口

**2.2 迁移 25+ 实体** — 从 ABP 基类改为自定义基类，添加 `[SugarTable]`/`[SugarColumn]` 属性，保持数据库列名完全一致

**2.3 创建自定义 User/Role/Permission 实体** — 替代 ABP Identity 表（`User`, `Role`, `UserRole`, `PermissionGrant`），表结构兼容旧数据

---

#### Phase 3: 数据访问层迁移（*依赖 Phase 1, 2*）

**3.1 创建 SqlSugar 通用仓储** — `Repository<T, TKey>` 封装 CRUD + 分页，`ReadOnlyRepository<T, TKey>` 只读版

**3.2 迁移 6 个自定义仓储** — 保留业务方法（如 `GetAllParametersInModule`），用 SqlSugar 的 `.Includes()` 替代 EF Core 的 `.Include()`

**3.3 替换所有服务中的仓储注入** — `AsyncExecuter.ToListAsync()` → SqlSugar `.ToListAsync()`，`GetQueryableAsync()` → SqlSugar 查询，`IUnitOfWorkManager` → SqlSugar 事务

---

#### Phase 4: 服务层迁移（*依赖 Phase 3*）

**4.1 创建新的服务基类** — `AppServiceBase`（提供 CurrentUserId、异常辅助），`CrudServiceBase<...>`（封装标准 CRUD）

**4.2 迁移 16 个 CrudAppService** — 改继承 `CrudServiceBase`

**4.3 迁移 15+ ApplicationService** — 改继承 `AppServiceBase`，替换 ABP 特有依赖

**4.4 迁移 DTO 映射** — 移除 ABP `MapperBase` 封装，改为纯 Mapperly `[Mapper]` partial class

**4.5 迁移账户服务** — `IRepository<IdentityUser>` → `IRepository<User>`，保持密码哈希和 JWT 逻辑

---

#### Phase 5: 控制器层（*依赖 Phase 4*）

**5.1 创建控制器基类** — `DFAppControllerBase : ControllerBase` + 全局 `ExceptionFilter`

**5.2 为 16 个 CRUD 服务创建控制器** — 路由必须保持 `/api/app/{kebab-case-entity}` 模式，确保前端零修改：

| 服务 | 路由 |
|------|------|
| LotteryService | `/api/app/lottery` |
| BookkeepingExpenditureService | `/api/app/bookkeeping-expenditure` |
| BookkeepingCategoryService | `/api/app/bookkeeping-category` |
| DynamicIPService | `/api/app/dynamic-ip` |
| ElectricVehicleService | `/api/app/electric-vehicle` |
| Aria2Service | `/api/app/aria2` |
| ConfigurationInfoService | `/api/app/configuration-info` |
| ...（共 16 个） | |

**5.3 为 15+ 作服务创建控制器** — AccountController、RSSController 等

**5.4 迁移现有 4 个 HttpApi 自定义控制器** — Aria2Controller, FileUploadInfoController, FileDownloadController, LogViewerController

---

#### Phase 6: 权限与认证系统（*并行于 Phase 4-5*）

**6.1 自定义权限系统** — 保持 10 组 80+ 权限定义，实现 `IAuthorizationHandler` 从 JWT Permission Claims 检查权限

**6.2 JWT 认证** — 保持现有配置不变

**6.3 数据迁移脚本** — `AbpUsers → AppUsers`，`AbpRoles → AppRoles`，`AbpPermissionGrants → AppPermissionGrants`

---

#### Phase 7: 基础设施迁移（*并行于 Phase 4-5*）

**7.1 Quartz.NET** — 移除 ABP 封装，直接 `AddQuartzHostedService()`，迁移 3 个定时任务为 `IJob`

**7.2 SignalR** — 保持 `Aria2Hub` 不变

**7.3 全局异常处理** — 自定义 `BusinessException` 替代 `UserFriendlyException`

**7.4 中间件精简** — 移除 7 个 ABP 中间件，保留标准 ASP.NET Core 管道

---

#### Phase 8: 数据库迁移脚本（*并行于 Phase 2-7*）

**8.1** 创建用户/角色/权限数据迁移 SQL
**8.2** 创建 ABP 系统表（30+张）清理 SQL
**8.3** 密码哈希兼容 — 保留 `PasswordHasher<T>` 用法，ASP.NET Core Identity 的哈希算法与泛型类型参数无关，可直接兼容

---

#### Phase 9: 项目清理（*依赖所有 Phase*）

**9.1** 删除 6 个旧项目目录（Domain, Domain.Shared, Application, Application.Contracts, EntityFrameworkCore, HttpApi）
**9.2** 更新 DFApp.sln 只保留 `DFApp.Web` + DFApp.LotteryProxy
**9.3** 更新 AGENTS.md 和 docs/ 文档

---

### Relevant Files

**需要完全重写：**
- DFApp.Web.csproj — 移除 ABP 包，添加 SqlSugar 等
- DFAppWebModule.cs — 替换为标准 Program.cs 配置
- Program.cs — 标准 ASP.NET Core 启动
- DFAppDbContext.cs — 完全移除，改用 SqlSugar

**需要修改基类（25+ 实体）：** 所有 DFApp.Domain 下的实体类

**需要修改（34 服务）：** 所有 DFApp.Application 下的服务类

**需要新建（~20 控制器）：** 每个应用服务对应一个 Controller

**需要新建基础设施：** SqlSugar配置、自定义实体基类、全局异常过滤器、权限授权处理器

**SQL 脚本：** 用户数据迁移脚本 + ABP 表清理脚本

---

### Verification

1. `dotnet build` 编译无错误
2. 启动后 `/swagger` 对比所有 API 路由与迁移前完全一致
3. 现有用户名密码登录 → JWT token → 权限声明验证
4. 每个实体 CRUD 增删改查数据正确性
5. Vue 前端所有页面功能正常（无 API 404/500）
6. Quartz 定时任务正常调度（RSS、油价、磁盘检查）
7. SignalR Aria2 Hub 正常连接和推送
8. SQLite 数据库全部业务数据完好
9. 无权限用户无法访问受保护端点

---

### Decisions

- **保持 `/api/app/` 路由前缀** — 前端零修改
- **保留 `PasswordHasher<T>`** — 现有用户密码不失效
- **保留 `DFApp.Web` 项目名** — 避免修改发布配置
- **SqlSugar** — SQLite 支持好、LINQ 友好、国内社区活跃
- **保留 Mapperly** — 编译时源码生成器，去除 ABP 基类封装后直接可用
- **不保留 ABP 审计日志表** — 改用 Serilog 文件日志
- **保留软删除机制** — SqlSugar 全局过滤器实现

---

### Further Considerations

1. **密码哈希兼容性**：ASP.NET Core Identity 的 `PasswordHasher<T>` 内部 PBKDF2 算法与泛型类型参数无关，迁移后可直接兼容，但需测试确认。

2. **表名精确映射**：现有业务表名可能带有 `App` 前缀（ABP 约定），SqlSugar 的 `[SugarTable("表名")]` 需要精确匹配。**建议迁移前先导出完整数据库 schema**。

3. **ConcurrencyStamp 乐观并发**：ABP 用字符串 UUID 作为 ConcurrencyStamp，SqlSugar 用数字版本号做乐观锁。需要决定是保持字符串形式（手动实现）还是切换到 SqlSugar 的版本号机制（需要数据迁移）。