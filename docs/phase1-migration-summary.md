# Phase 1 迁移总结文档

## 1. 概述

### 1.1 Phase 1 目标和范围

Phase 1 的主要目标是从 ABP Framework 迁移到纯 ASP.NET Core + SqlSugar，实现以下核心功能：

- 移除 ABP Framework 的依赖，使用纯 ASP.NET Core
- 将 EF Core 替换为 SqlSugar ORM
- 从 Autofac 迁移到原生 .NET 依赖注入
- 实现自定义权限系统替代 ABP 权限系统
- 实现自定义实体基类和审计功能
- 实现全局异常处理机制
- 实现软删除和数据过滤器

### 1.2 完成时间

Phase 1 于 2026 年 3 月完成。

### 1.3 主要变更内容

- 创建了自定义实体基类体系
- 实现了 SqlSugar 配置和仓储模式
- 实现了基于 JWT 的自定义权限系统
- 实现了全局异常过滤器
- 实现了 AOP 自动填充审计字段
- 实现了软删除和数据过滤器
- 创建了应用服务基类和 CRUD 服务基类

## 2. 项目结构变更

### 2.1 新创建的目录结构

```
src/DFApp.Web/
├── Background/              # 后台服务
├── Components/              # 组件
├── Controllers/             # 控制器
├── Data/                    # 数据访问层
│   ├── ISqlSugarReadOnlyRepository.cs
│   ├── ISqlSugarRepository.cs
│   ├── SqlSugarConfig.cs
│   ├── SqlSugarReadOnlyRepository.cs
│   └── SqlSugarRepository.cs
├── Domain/                  # 领域层（自定义实体基类）
│   ├── IEntity.cs
│   ├── EntityBase.cs
│   ├── Entity.cs
│   ├── AuditedEntity.cs
│   ├── FullAuditedEntity.cs
│   ├── CreationAuditedEntity.cs
│   ├── IAuditedObject.cs
│   ├── IFullAuditedObject.cs
│   ├── ICreationAuditedObject.cs
│   ├── IHasCreationTime.cs
│   ├── IHasModificationTime.cs
│   ├── IHasDeletionTime.cs
│   ├── ICreatorId.cs
│   ├── IModifierId.cs
│   ├── IDeleterId.cs
│   └── ISoftDelete.cs
├── DTOs/                    # 数据传输对象
├── Hubs/                    # SignalR Hub
├── Infrastructure/          # 基础设施
│   ├── BusinessException.cs
│   ├── GlobalExceptionFilter.cs
│   ├── NotFoundException.cs
│   └── ValidationException.cs
├── Mapping/                 # 对象映射
├── Permissions/             # 权限系统
│   ├── IPermissionChecker.cs
│   ├── PermissionAttribute.cs
│   ├── PermissionAuthorizationHandler.cs
│   ├── PermissionChecker.cs
│   ├── PermissionPolicyProvider.cs
│   └── PermissionRequirement.cs
├── Services/                # 应用服务
│   ├── AppServiceBase.cs
│   └── CrudServiceBase.cs
└── Utilities/               # 工具类
```

### 2.2 目录用途说明

| 目录 | 用途 |
|------|------|
| `Background/` | 存放后台服务（如 Aria2 监控、Telegram 监听等） |
| `Components/` | 存放可复用的 UI 组件 |
| `Controllers/` | 存放 API 控制器 |
| `Data/` | 存放数据访问层代码（SqlSugar 配置和仓储实现） |
| `Domain/` | 存放自定义实体基类和接口 |
| `DTOs/` | 存放数据传输对象 |
| `Hubs/` | 存放 SignalR Hub |
| `Infrastructure/` | 存放基础设施代码（异常处理、验证等） |
| `Mapping/` | 存放对象映射配置 |
| `Permissions/` | 存放权限系统相关代码 |
| `Services/` | 存放应用服务基类 |
| `Utilities/` | 存放工具类 |

## 3. 依赖变更

### 3.1 移除的 ABP 包

Phase 1 移除了以下 ABP 相关包（从 `DFApp.Web.csproj` 中移除）：

- Volo.Abp.AspNetCore.Mvc
- Volo.Abp.AspNetCore.Mvc.UI.MultiTenancy
- Volo.Abp.AspNetCore.Serilog
- Volo.Abp.Autofac
- Volo.Abp.AutoMapper
- Volo.Abp.BackgroundJobs.Quartz
- Volo.Abp.Modularity
- Volo.Abp.PermissionManagement
- Volo.Abp.SettingManagement
- Volo.Abp.Timing
- Volo.Abp.Uow
- Volo.Abp.Validation
- Volo.Abp.AspNetCore.Authentication.JwtBearer
- Volo.Abp.AspNetCore.SignalR
- Volo.Abp.OpenIddict
- Volo.Abp.Swashbuckle
- Volo.Abp.EntityFrameworkCore
- Volo.Abp.EntityFrameworkCore.Sqlite
- 其他 ABP 相关包

### 3.2 添加的新包

Phase 1 添加了以下新包：

| 包名 | 版本 | 用途 |
|------|------|------|
| SqlSugarCore | 5.1.4.160 | ORM 框架，替代 EF Core |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.0 | JWT 认证 |
| Serilog.AspNetCore | 9.0.0 | 日志记录 |
| Serilog.Sinks.Async | 2.1.0 | 异步日志写入 |
| Swashbuckle.AspNetCore | 8.0.0 | Swagger/OpenAPI 文档 |
| Riok.Mapperly | 4.3.0 | 对象映射 |
| Quartz | 3.15.0 | 定时任务 |
| Quartz.Extensions.Hosting | 3.15.0 | Quartz 托管服务 |
| WTelegramClient | 4.3.12 | Telegram 客户端 |
| Microsoft.AspNetCore.SignalR.Client | 10.0.0 | SignalR 客户端 |
| HtmlAgilityPack | 1.11.71 | HTML 解析 |
| AngleSharp | 1.1.2 | HTML 解析 |
| SixLabors.ImageSharp | 3.1.6 | 图像处理 |
| SixLabors.ImageSharp.Drawing | 2.1.4 | 图像绘制 |
| SixLabors.Fonts | 2.0.8 | 字体处理 |

### 3.3 保留的包

Phase 1 保留了以下包：

- Microsoft.EntityFrameworkCore.Design（用于 EF Core 迁移，后续将移除）
- 其他非 ABP 的业务相关包

## 4. 核心文件变更

### 4.1 Program.cs 的主要变更

[`Program.cs`](src/DFApp.Web/Program.cs) 是应用的启动入口，经历了重大重构：

#### 主要变更点：

1. **移除 ABP 模块依赖**
   - 移除了 `builder.Services.AddApplication<DFAppWebModule>()`
   - 移除了 ABP 模块的配置

2. **配置 SqlSugar**
   ```csharp
   builder.Services.AddSingleton<SqlSugarConfig>();
   builder.Services.AddScoped<ICurrentUser, CurrentUser>();
   builder.Services.AddScoped<ISqlSugarClient>(s =>
   {
       var config = s.GetRequiredService<SqlSugarConfig>();
       return config.CreateClient();
   });
   ```

3. **配置权限系统**
   ```csharp
   builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();
   builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
   builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
   ```

4. **注册通用仓储**
   ```csharp
   builder.Services.AddScoped(typeof(ISqlSugarRepository<,>), typeof(SqlSugarRepository<,>));
   builder.Services.AddScoped(typeof(ISqlSugarReadOnlyRepository<,>), typeof(SqlSugarReadOnlyRepository<,>));
   ```

5. **配置 JWT 认证**
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = builder.Configuration["Jwt:Issuer"],
               ValidAudience = builder.Configuration["Jwt:Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
           };
       });
   ```

6. **配置全局异常过滤器**
   ```csharp
   builder.Services.AddControllers(options =>
   {
       options.Filters.Add<GlobalExceptionFilter>();
   })
   ```

7. **移除 ABP 中间件**
   - 移除了 `app.UseAbpRequestLocalization()`
   - 移除了 `app.UseAbpSecurityHeaders()`
   - 移除了其他 ABP 特定的中间件

### 4.2 DFApp.Web.csproj 的主要变更

[`DFApp.Web.csproj`](src/DFApp.Web/DFApp.Web.csproj) 的主要变更：

1. **移除 ABP 包引用**
   - 移除了所有 `Volo.Abp.*` 包引用

2. **添加新包引用**
   - 添加了 `SqlSugarCore`
   - 添加了 `Microsoft.AspNetCore.Authentication.JwtBearer`
   - 添加了 `Serilog.AspNetCore`
   - 添加了 `Swashbuckle.AspNetCore`
   - 添加了 `Riok.Mapperly`
   - 添加了其他业务相关包

3. **保留项目引用**
   - 保留了对 `DFApp.Application` 的引用
   - 保留了对 `DFApp.HttpApi` 的引用
   - 保留了对 `DFApp.EntityFrameworkCore` 的引用（临时保留，后续将移除）

### 4.3 其他重要文件的变更

1. **移除的文件**
   - `DFAppWebModule.cs` - ABP 模块定义（已备份为 `.bak` 文件）
   - `DFAppMenuContributor.cs` - ABP 菜单贡献者（已备份为 `.bak` 文件）
   - 其他 ABP 相关配置文件

2. **保留的文件**
   - `appsettings.json` - 应用配置文件
   - `appsettings.secrets.json` - 敏感配置文件
   - `web.config` - IIS 配置文件
   - 其他非 ABP 相关文件

## 5. 新创建的类

### 5.1 自定义实体基类（接口和基类）

#### 实体接口

1. **[`IEntity<TKey>`](src/DFApp.Web/Domain/IEntity.cs)**
   - 定义实体的基本标识
   - 包含 `Id` 属性

2. **[`IHasCreationTime`](src/DFApp.Web/Domain/IHasCreationTime.cs)**
   - 定义创建时间接口
   - 包含 `CreationTime` 属性

3. **[`ICreatorId`](src/DFApp.Web/Domain/ICreatorId.cs)**
   - 定义创建者 ID 接口
   - 包含 `CreatorId` 属性

4. **[`IHasModificationTime`](src/DFApp.Web/Domain/IHasModificationTime.cs)**
   - 定义修改时间接口
   - 包含 `LastModificationTime` 属性

5. **[`IModifierId`](src/DFApp.Web/Domain/IModifierId.cs)**
   - 定义修改者 ID 接口
   - 包含 `LastModifierId` 属性

6. **[`IHasDeletionTime`](src/DFApp.Web/Domain/IHasDeletionTime.cs)**
   - 定义删除时间接口
   - 包含 `DeletionTime` 属性

7. **[`IDeleterId`](src/DFApp.Web/Domain/IDeleterId.cs)**
   - 定义删除者 ID 接口
   - 包含 `DeleterId` 属性

8. **[`ISoftDelete`](src/DFApp.Web/Domain/ISoftDelete.cs)**
   - 定义软删除接口
   - 包含 `IsDeleted` 属性

9. **[`IAuditedObject`](src/DFApp.Web/Domain/IAuditedObject.cs)**
   - 定义审计对象接口
   - 组合了创建和修改相关接口

10. **[`IFullAuditedObject`](src/DFApp.Web/Domain/IFullAuditedObject.cs)**
    - 定义完整审计对象接口
    - 组合了创建、修改和删除相关接口

11. **[`ICreationAuditedObject`](src/DFApp.Web/Domain/ICreationAuditedObject.cs)**
    - 定义创建审计对象接口
    - 组合了创建相关接口

#### 实体基类

1. **[`EntityBase<TKey>`](src/DFApp.Web/Domain/EntityBase.cs)**
   - 实体基类，实现 `IEntity<TKey>`
   - 提供基本的实体功能

2. **[`Entity<TKey>`](src/DFApp.Web/Domain/Entity.cs)**
   - 简单实体类，继承自 `EntityBase<TKey>`

3. **[`AuditedEntity<TKey>`](src/DFApp.Web/Domain/AuditedEntity.cs)**
   - 审计实体类，继承自 `EntityBase<TKey>`
   - 实现了 `IAuditedObject`
   - 包含创建和修改信息：
     - `CreationTime`
     - `CreatorId`
     - `LastModificationTime`
     - `LastModifierId`

4. **[`FullAuditedEntity<TKey>`](src/DFApp.Web/Domain/FullAuditedEntity.cs)**
   - 完整审计实体类，继承自 `AuditedEntity<TKey>`
   - 实现了 `IFullAuditedObject`
   - 包含创建、修改和删除信息：
     - `IsDeleted`
     - `DeletionTime`
     - `DeleterId`

5. **[`CreationAuditedEntity<TKey>`](src/DFApp.Web/Domain/CreationAuditedEntity.cs)**
   - 创建审计实体类，继承自 `EntityBase<TKey>`
   - 实现了 `ICreationAuditedObject`
   - 仅包含创建信息

6. **[`AuditedEntity.Guid.cs`](src/DFApp.Web/Domain/AuditedEntity.Guid.cs)**
   - Guid 类型的审计实体便捷类

7. **[`FullAuditedEntity.Guid.cs`](src/DFApp.Web/Domain/FullAuditedEntity.Guid.cs)**
   - Guid 类型的完整审计实体便捷类

8. **[`CreationAuditedEntity.Guid.cs`](src/DFApp.Web/Domain/CreationAuditedEntity.Guid.cs)**
   - Guid 类型的创建审计实体便捷类

### 5.2 SqlSugar 配置类

1. **[`SqlSugarConfig`](src/DFApp.Web/Data/SqlSugarConfig.cs)**
   - SqlSugar 配置类，提供数据库连接和自动化功能
   - 主要方法：
     - `CreateClient()` - 创建并配置 SqlSugar 客户端
     - `ConfigureAop()` - 配置 AOP 自动填充审计字段
     - `ConfigureSoftDeleteFilter()` - 配置全局软删除过滤器
     - `ConfigureCreatorIdFilter()` - 配置 CreatorId 数据过滤器

2. **[`ICurrentUser`](src/DFApp.Web/Data/SqlSugarConfig.cs)**
   - 当前用户接口，用于获取当前登录用户信息
   - 包含 `Id` 和 `UserName` 属性

3. **[`CurrentUser`](src/DFApp.Web/Data/SqlSugarConfig.cs)**
   - 当前用户实现
   - 实现 `ICurrentUser` 接口

### 5.3 基础设施类（异常处理、权限系统）

#### 异常处理类

1. **[`BusinessException`](src/DFApp.Web/Infrastructure/BusinessException.cs)**
   - 业务异常类，用于处理业务逻辑中的错误
   - 包含 `Code` 和 `Details` 属性
   - 提供多个构造函数以支持不同的使用场景

2. **[`NotFoundException`](src/DFApp.Web/Infrastructure/NotFoundException.cs)**
   - 资源未找到异常类
   - 继承自 `BusinessException`

3. **[`ValidationException`](src/DFApp.Web/Infrastructure/ValidationException.cs)**
   - 验证异常类
   - 包含 `ValidationErrors` 属性
   - 继承自 `BusinessException`

4. **[`GlobalExceptionFilter`](src/DFApp.Web/Infrastructure/GlobalExceptionFilter.cs)**
   - 全局异常过滤器，用于捕获和处理所有异常
   - 根据异常类型确定 HTTP 状态码
   - 构建统一的错误响应
   - 在开发环境中包含堆栈跟踪

#### 权限系统类

1. **[`IPermissionChecker`](src/DFApp.Web/Permissions/IPermissionChecker.cs)**
   - 权限检查接口
   - 定义 `IsGrantedAsync` 方法

2. **[`PermissionChecker`](src/DFApp.Web/Permissions/PermissionChecker.cs)**
   - 权限检查实现
   - 从 JWT Token 的 Claims 中读取权限
   - 实现 `IPermissionChecker` 接口

3. **[`PermissionAttribute`](src/DFApp.Web/Permissions/PermissionAttribute.cs)**
   - 权限特性，用于标记控制器或操作需要特定权限
   - 可以应用于类或方法

4. **[`PermissionAuthorizationHandler`](src/DFApp.Web/Permissions/PermissionAuthorizationHandler.cs)**
   - 权限授权处理器，用于检查用户是否拥有所需权限
   - 实现 `AuthorizationHandler<PermissionRequirement>`

5. **[`PermissionPolicyProvider`](src/DFApp.Web/Permissions/PermissionPolicyProvider.cs)**
   - 权限策略提供者
   - 实现 `IAuthorizationPolicyProvider`

6. **[`PermissionRequirement`](src/DFApp.Web/Permissions/PermissionRequirement.cs)**
   - 权限需求类
   - 实现 `IAuthorizationRequirement`

### 5.4 服务基类

1. **[`AppServiceBase`](src/DFApp.Web/Services/AppServiceBase.cs)**
   - 应用服务基类，提供通用的应用服务功能
   - 包含 `CurrentUser` 和 `PermissionChecker` 属性
   - 提供以下辅助方法：
     - `IsGrantedAsync()` - 检查权限
     - `CheckPermissionAsync()` - 检查权限，没有权限则抛出异常
     - `EnsureLoggedIn()` - 确保用户已登录
     - `EnsureEntityExists()` - 确保实体存在

2. **[`CrudServiceBase<TEntity, TKey, TGetOutputDto, TCreateInputDto, TUpdateInputDto>`](src/DFApp.Web/Services/CrudServiceBase.cs)**
   - CRUD 服务基类，提供标准的 CRUD 操作
   - 继承自 `AppServiceBase`
   - 包含 `Repository` 属性
   - 提供以下方法：
     - `GetAsync()` - 根据 ID 获取实体
     - `GetListAsync()` - 获取实体列表
     - `GetPagedListAsync()` - 分页查询
     - `CreateAsync()` - 创建实体
     - `UpdateAsync()` - 更新实体
     - `DeleteAsync()` - 删除实体
     - `MapToGetOutputDtoAsync()` - 实体到 DTO 映射
     - `MapToEntityAsync()` - DTO 到实体映射

### 5.5 数据访问类

1. **[`ISqlSugarRepository<T, TKey>`](src/DFApp.Web/Data/ISqlSugarRepository.cs)**
   - SqlSugar 仓储接口
   - 定义标准的 CRUD 操作方法

2. **[`SqlSugarRepository<T, TKey>`](src/DFApp.Web/Data/SqlSugarRepository.cs)**
   - SqlSugar 仓储实现
   - 实现 `ISqlSugarRepository<T, TKey>` 接口
   - 提供完整的 CRUD 操作实现

3. **[`ISqlSugarReadOnlyRepository<T, TKey>`](src/DFApp.Web/Data/ISqlSugarReadOnlyRepository.cs)**
   - 只读仓储接口
   - 定义只读操作方法

4. **[`SqlSugarReadOnlyRepository<T, TKey>`](src/DFApp.Web/Data/SqlSugarReadOnlyRepository.cs)**
   - 只读仓储实现
   - 实现 `ISqlSugarReadOnlyRepository<T, TKey>` 接口

## 6. 技术栈变更

### 6.1 从 ABP Framework 迁移到纯 ASP.NET Core

#### 变更前（ABP Framework）

- 使用 ABP 的模块系统
- 使用 ABP 的依赖注入（基于 Autofac）
- 使用 ABP 的权限系统
- 使用 ABP 的审计功能
- 使用 ABP 的异常处理
- 使用 ABP 的数据过滤器
- 使用 ABP 的单元工作模式

#### 变更后（纯 ASP.NET Core）

- 使用原生 ASP.NET Core
- 使用原生 .NET 依赖注入
- 使用自定义权限系统
- 使用自定义审计功能
- 使用自定义异常处理
- 使用 SqlSugar 的数据过滤器
- 使用 SqlSugar 的事务管理

### 6.2 从 EF Core 迁移到 SqlSugar

#### 变更前（EF Core）

- 使用 `DbContext` 管理数据库上下文
- 使用 `DbSet<TEntity>` 管理实体集合
- 使用 LINQ to Entities 进行查询
- 使用 EF Core 的迁移系统
- 使用 EF Core 的数据过滤器

#### 变更后（SqlSugar）

- 使用 `ISqlSugarClient` 管理数据库连接
- 使用 `ISugarQueryable<T>` 进行查询
- 使用 SqlSugar 的 AOP 功能
- 使用 SqlSugar 的数据过滤器
- 使用 SqlSugar 的软删除功能

### 6.3 从 Autofac 迁移到原生 DI

#### 变更前（Autofac）

- 使用 Autofac 容器
- 使用 Autofac 的模块系统
- 使用 Autofac 的属性注入

#### 变更后（原生 DI）

- 使用原生 `IServiceCollection`
- 使用原生 `IServiceProvider`
- 使用构造函数注入

### 6.4 从 ABP 权限系统迁移到自定义权限系统

#### 变更前（ABP 权限系统）

- 使用 ABP 的权限定义提供者
- 使用 ABP 的权限检查器
- 使用 ABP 的权限授权处理器
- 权限存储在数据库中

#### 变更后（自定义权限系统）

- 使用自定义权限检查器
- 使用自定义授权处理器
- 使用自定义策略提供者
- 权限存储在 JWT Token 的 Claims 中

## 7. 关键技术点

### 7.1 SqlSugar 的 AOP 自动填充

[`SqlSugarConfig`](src/DFApp.Web/Data/SqlSugarConfig.cs) 中的 `ConfigureAop` 方法实现了 AOP 自动填充审计字段：

```csharp
db.Aop.DataExecuting = (oldValue, entityInfo) =>
{
    // 插入操作
    if (entityInfo.OperationType == DataFilterType.InsertByObject)
    {
        // 设置创建时间
        if (entityInfo.PropertyName == nameof(IHasCreationTime.CreationTime) && entityInfo.EntityValue is IHasCreationTime creationTimeEntity)
        {
            if (creationTimeEntity.CreationTime == default)
            {
                creationTimeEntity.CreationTime = DateTime.Now;
            }
        }

        // 设置创建者 ID
        if (entityInfo.PropertyName == nameof(ICreatorId.CreatorId) && entityInfo.EntityValue is ICreatorId creatorIdEntity)
        {
            if (creatorIdEntity.CreatorId == null)
            {
                var currentUser = _serviceProvider.GetService<ICurrentUser>();
                if (currentUser != null && currentUser.Id.HasValue)
                {
                    creatorIdEntity.CreatorId = currentUser.Id.Value;
                }
            }
        }
    }

    // 更新操作
    if (entityInfo.OperationType == DataFilterType.UpdateByObject)
    {
        // 设置最后修改时间
        if (entityInfo.PropertyName == nameof(IHasModificationTime.LastModificationTime) && entityInfo.EntityValue is IHasModificationTime modificationTimeEntity)
        {
            modificationTimeEntity.LastModificationTime = DateTime.Now;
        }

        // 设置最后修改者 ID
        if (entityInfo.PropertyName == nameof(IModifierId.LastModifierId) && entityInfo.EntityValue is IModifierId modifierIdEntity)
        {
            var currentUser = _serviceProvider.GetService<ICurrentUser>();
            if (currentUser != null && currentUser.Id.HasValue)
            {
                modifierIdEntity.LastModifierId = currentUser.Id.Value;
            }
        }
    }

    // 删除操作
    if (entityInfo.OperationType == DataFilterType.DeleteByObject)
    {
        // 设置删除时间
        if (entityInfo.PropertyName == nameof(IHasDeletionTime.DeletionTime) && entityInfo.EntityValue is IHasDeletionTime deletionTimeEntity)
        {
            if (deletionTimeEntity.DeletionTime == null)
            {
                deletionTimeEntity.DeletionTime = DateTime.Now;
            }
        }

        // 设置删除者 ID
        if (entityInfo.PropertyName == nameof(IDeleterId.DeleterId) && entityInfo.EntityValue is IDeleterId deleterIdEntity)
        {
            if (deleterIdEntity.DeleterId == null)
            {
                var currentUser = _serviceProvider.GetService<ICurrentUser>();
                if (currentUser != null && currentUser.Id.HasValue)
                {
                    deleterIdEntity.DeleterId = currentUser.Id.Value;
                }
            }
        }

        // 设置软删除标记
        if (entityInfo.PropertyName == nameof(ISoftDelete.IsDeleted) && entityInfo.EntityValue is ISoftDelete softDeleteEntity)
        {
            softDeleteEntity.IsDeleted = true;
        }
    }
};
```

### 7.2 全局软删除过滤器

[`SqlSugarConfig`](src/DFApp.Web/Data/SqlSugarConfig.cs) 中的 `ConfigureSoftDeleteFilter` 方法实现了全局软删除过滤器：

```csharp
private void ConfigureSoftDeleteFilter(ISqlSugarClient db)
{
    db.QueryFilter.Add(new TableFilterItem<ISoftDelete>(it => it.IsDeleted == false));
}
```

这个过滤器会自动应用到所有实现了 `ISoftDelete` 接口的实体查询中，确保查询结果不包含已软删除的记录。

### 7.3 CreatorId 数据过滤器

[`SqlSugarConfig`](src/DFApp.Web/Data/SqlSugarConfig.cs) 中的 `ConfigureCreatorIdFilter` 方法实现了 CreatorId 数据过滤器：

```csharp
private void ConfigureCreatorIdFilter(ISqlSugarClient db)
{
    var currentUser = _serviceProvider.GetService<ICurrentUser>();
    if (currentUser != null && currentUser.Id.HasValue)
    {
        db.QueryFilter.Add(new TableFilterItem<ICreatorId>(it => it.CreatorId == currentUser.Id.Value));
    }
}
```

这个过滤器会自动应用到所有实现了 `ICreatorId` 接口的实体查询中，确保查询结果只包含当前用户创建的记录。

### 7.4 权限检查的实现方式

权限检查通过以下方式实现：

1. **权限存储在 JWT Token 的 Claims 中**
   - 在用户登录时，将用户的权限列表添加到 JWT Token 的 Claims 中
   - Claim 类型为 "Permission"

2. **权限检查器**
   - [`PermissionChecker`](src/DFApp.Web/Permissions/PermissionChecker.cs) 从 HTTP 上下文中获取当前用户
   - 从用户的 Claims 中读取权限列表
   - 检查用户是否拥有指定权限

3. **权限授权处理器**
   - [`PermissionAuthorizationHandler`](src/DFApp.Web/Permissions/PermissionAuthorizationHandler.cs) 在授权时检查用户权限
   - 如果用户拥有所需权限，则授权成功

4. **权限特性**
   - [`PermissionAttribute`](src/DFApp.Web/Permissions/PermissionAttribute.cs) 用于标记控制器或操作需要特定权限
   - 可以应用于类或方法

5. **权限策略提供者**
   - [`PermissionPolicyProvider`](src/DFApp.Web/Permissions/PermissionPolicyProvider.cs) 根据权限名称动态创建授权策略

### 7.5 异常处理机制

异常处理通过以下方式实现：

1. **全局异常过滤器**
   - [`GlobalExceptionFilter`](src/DFApp.Web/Infrastructure/GlobalExceptionFilter.cs) 捕获所有未处理的异常
   - 根据异常类型确定 HTTP 状态码
   - 构建统一的错误响应

2. **自定义异常类**
   - [`BusinessException`](src/DFApp.Web/Infrastructure/BusinessException.cs) - 业务异常
   - [`NotFoundException`](src/DFApp.Web/Infrastructure/NotFoundException.cs) - 资源未找到异常
   - [`ValidationException`](src/DFApp.Web/Infrastructure/ValidationException.cs) - 验证异常

3. **错误响应模型**
   - [`ErrorResponse`](src/DFApp.Web/Infrastructure/GlobalExceptionFilter.cs) 包含错误代码、错误消息、详细信息、时间戳和堆栈跟踪

4. **HTTP 状态码映射**
   - `NotFoundException` → 404 Not Found
   - `ValidationException` → 400 Bad Request
   - `BusinessException` → 400 Bad Request
   - `UnauthorizedAccessException` → 401 Unauthorized
   - `ArgumentException` → 400 Bad Request
   - `InvalidOperationException` → 400 Bad Request
   - 其他异常 → 500 Internal Server Error

## 8. 后续工作

### 8.1 Phase 2 的主要任务

Phase 2 将继续推进 ABP Framework 的移除工作，主要任务包括：

1. **迁移应用服务**
   - 将 `DFApp.Application` 项目中的应用服务迁移到 `DFApp.Web` 项目
   - 使用新的服务基类（`AppServiceBase` 和 `CrudServiceBase`）
   - 使用新的仓储接口（`ISqlSugarRepository` 和 `ISqlSugarReadOnlyRepository`）

2. **迁移控制器**
   - 将 `DFApp.HttpApi` 项目中的控制器迁移到 `DFApp.Web` 项目
   - 使用新的控制器基类（`DFAppControllerBase`）
   - 使用新的权限特性（`PermissionAttribute`）

3. **迁移实体**
   - 将 `DFApp.Domain` 项目中的实体迁移到 `DFApp.Web.Domain`
   - 使用新的实体基类（`AuditedEntity`、`FullAuditedEntity` 等）

4. **移除 EF Core**
   - 移除 `DFApp.EntityFrameworkCore` 项目
   - 移除 EF Core 相关包
   - 使用 SqlSugar 进行所有数据库操作

5. **移除 ABP 相关项目**
   - 移除 `DFApp.Application` 项目
   - 移除 `DFApp.HttpApi` 项目
   - 移除 `DFApp.Domain` 项目（迁移到 `DFApp.Web.Domain`）
   - 移除 `DFApp.Domain.Shared` 项目（迁移到 `DFApp.Web`）

6. **更新前端**
   - 更新 API 调用以适配新的后端
   - 更新权限检查逻辑
   - 更新错误处理逻辑

### 8.2 需要注意的事项

1. **数据库迁移**
   - 需要为 SqlSugar 创建数据库初始化脚本
   - 需要确保数据库结构与实体定义一致

2. **权限迁移**
   - 需要将现有的权限定义迁移到新的权限系统
   - 需要确保 JWT Token 包含所有必要的权限 Claims

3. **测试**
   - 需要对所有迁移的功能进行充分测试
   - 需要确保性能没有明显下降

4. **向后兼容**
   - 需要确保 API 接口保持向后兼容
   - 需要确保数据库结构保持向后兼容

### 8.3 可能的风险点

1. **数据丢失风险**
   - 在迁移过程中可能会丢失数据
   - 需要做好数据备份

2. **功能缺失风险**
   - 可能会遗漏某些功能
   - 需要进行充分的功能测试

3. **性能风险**
   - SqlSugar 的性能可能与 EF Core 有所不同
   - 需要进行性能测试和优化

4. **安全风险**
   - 新的权限系统可能存在安全漏洞
   - 需要进行安全测试

5. **兼容性风险**
   - 前端可能需要大量修改
   - 需要做好前后端协调

## 9. 附录

### 9.1 创建的文件列表

#### 数据访问层
- [`src/DFApp.Web/Data/ISqlSugarRepository.cs`](src/DFApp.Web/Data/ISqlSugarRepository.cs)
- [`src/DFApp.Web/Data/ISqlSugarReadOnlyRepository.cs`](src/DFApp.Web/Data/ISqlSugarReadOnlyRepository.cs)
- [`src/DFApp.Web/Data/SqlSugarConfig.cs`](src/DFApp.Web/Data/SqlSugarConfig.cs)
- [`src/DFApp.Web/Data/SqlSugarRepository.cs`](src/DFApp.Web/Data/SqlSugarRepository.cs)
- [`src/DFApp.Web/Data/SqlSugarReadOnlyRepository.cs`](src/DFApp.Web/Data/SqlSugarReadOnlyRepository.cs)

#### 领域层（自定义实体基类）
- [`src/DFApp.Web/Domain/IEntity.cs`](src/DFApp.Web/Domain/IEntity.cs)
- [`src/DFApp.Web/Domain/EntityBase.cs`](src/DFApp.Web/Domain/EntityBase.cs)
- [`src/DFApp.Web/Domain/Entity.cs`](src/DFApp.Web/Domain/Entity.cs)
- [`src/DFApp.Web/Domain/AuditedEntity.cs`](src/DFApp.Web/Domain/AuditedEntity.cs)
- [`src/DFApp.Web/Domain/FullAuditedEntity.cs`](src/DFApp.Web/Domain/FullAuditedEntity.cs)
- [`src/DFApp.Web/Domain/CreationAuditedEntity.cs`](src/DFApp.Web/Domain/CreationAuditedEntity.cs)
- [`src/DFApp.Web/Domain/AuditedEntity.Guid.cs`](src/DFApp.Web/Domain/AuditedEntity.Guid.cs)
- [`src/DFApp.Web/Domain/FullAuditedEntity.Guid.cs`](src/DFApp.Web/Domain/FullAuditedEntity.Guid.cs)
- [`src/DFApp.Web/Domain/CreationAuditedEntity.Guid.cs`](src/DFApp.Web/Domain/CreationAuditedEntity.Guid.cs)
- [`src/DFApp.Web/Domain/IAuditedObject.cs`](src/DFApp.Web/Domain/IAuditedObject.cs)
- [`src/DFApp.Web/Domain/IFullAuditedObject.cs`](src/DFApp.Web/Domain/IFullAuditedObject.cs)
- [`src/DFApp.Web/Domain/ICreationAuditedObject.cs`](src/DFApp.Web/Domain/ICreationAuditedObject.cs)
- [`src/DFApp.Web/Domain/IHasCreationTime.cs`](src/DFApp.Web/Domain/IHasCreationTime.cs)
- [`src/DFApp.Web/Domain/IHasModificationTime.cs`](src/DFApp.Web/Domain/IHasModificationTime.cs)
- [`src/DFApp.Web/Domain/IHasDeletionTime.cs`](src/DFApp.Web/Domain/IHasDeletionTime.cs)
- [`src/DFApp.Web/Domain/ICreatorId.cs`](src/DFApp.Web/Domain/ICreatorId.cs)
- [`src/DFApp.Web/Domain/IModifierId.cs`](src/DFApp.Web/Domain/IModifierId.cs)
- [`src/DFApp.Web/Domain/IDeleterId.cs`](src/DFApp.Web/Domain/IDeleterId.cs)
- [`src/DFApp.Web/Domain/ISoftDelete.cs`](src/DFApp.Web/Domain/ISoftDelete.cs)

#### 基础设施层
- [`src/DFApp.Web/Infrastructure/BusinessException.cs`](src/DFApp.Web/Infrastructure/BusinessException.cs)
- [`src/DFApp.Web/Infrastructure/GlobalExceptionFilter.cs`](src/DFApp.Web/Infrastructure/GlobalExceptionFilter.cs)
- [`src/DFApp.Web/Infrastructure/NotFoundException.cs`](src/DFApp.Web/Infrastructure/NotFoundException.cs)
- [`src/DFApp.Web/Infrastructure/ValidationException.cs`](src/DFApp.Web/Infrastructure/ValidationException.cs)

#### 权限系统
- [`src/DFApp.Web/Permissions/IPermissionChecker.cs`](src/DFApp.Web/Permissions/IPermissionChecker.cs)
- [`src/DFApp.Web/Permissions/PermissionAttribute.cs`](src/DFApp.Web/Permissions/PermissionAttribute.cs)
- [`src/DFApp.Web/Permissions/PermissionAuthorizationHandler.cs`](src/DFApp.Web/Permissions/PermissionAuthorizationHandler.cs)
- [`src/DFApp.Web/Permissions/PermissionChecker.cs`](src/DFApp.Web/Permissions/PermissionChecker.cs)
- [`src/DFApp.Web/Permissions/PermissionPolicyProvider.cs`](src/DFApp.Web/Permissions/PermissionPolicyProvider.cs)
- [`src/DFApp.Web/Permissions/PermissionRequirement.cs`](src/DFApp.Web/Permissions/PermissionRequirement.cs)

#### 服务层
- [`src/DFApp.Web/Services/AppServiceBase.cs`](src/DFApp.Web/Services/AppServiceBase.cs)
- [`src/DFApp.Web/Services/CrudServiceBase.cs`](src/DFApp.Web/Services/CrudServiceBase.cs)

#### 其他
- [`src/DFApp.Web/Controllers/DFAppControllerBase.cs`](src/DFApp.Web/Controllers/DFAppControllerBase.cs)
- [`src/DFApp.Web/Background/Aria2MonitorWorker.cs`](src/DFApp.Web/Background/Aria2MonitorWorker.cs)

### 9.2 修改的文件列表

- [`src/DFApp.Web/Program.cs`](src/DFApp.Web/Program.cs) - 完全重写，移除 ABP 依赖，配置新系统
- [`src/DFApp.Web/DFApp.Web.csproj`](src/DFApp.Web/DFApp.Web.csproj) - 移除 ABP 包，添加新包

### 9.3 删除的文件列表

- `src/DFApp.Web/DFAppWebModule.cs` - ABP 模块定义（已备份为 `.bak` 文件）
- `src/DFApp.Web/Menus/DFAppMenuContributor.cs` - ABP 菜单贡献者（已备份为 `.bak` 文件）
- `src/DFApp.Web/Pages/DFAppPageModel.cs` - ABP 页面模型（已备份为 `.bak` 文件）

### 9.4 编译状态

Phase 1 完成后，项目可以正常编译和运行。

- 后端项目可以正常启动
- API 接口可以正常访问
- 数据库连接正常
- 权限系统正常工作
- 异常处理正常工作

### 9.5 参考文档

- [SqlSugar 官方文档](https://www.donet5.com/Home/Doc)
- [ASP.NET Core 官方文档](https://docs.microsoft.com/aspnet/core)
- [JWT Bearer 认证](https://docs.microsoft.com/aspnet/core/security/authentication/jwt)
- [ASP.NET Core 授权](https://docs.microsoft.com/aspnet/core/security/authorization)

---

**文档版本**: 1.0  
**最后更新**: 2026 年 3 月 26 日  
**维护者**: DFApp 开发团队
