# Phase 4.3 Batch 1 迁移总结 - AccountAppService 和 UserManagementAppService

## 迁移日期
2026-03-30

## 迁移范围
本次迁移了账户管理相关的两个核心服务：
1. `AccountAppService` - 账户服务（登录、密码重置）
2. `UserManagementAppService` - 用户管理服务（CRUD、密码修改）

## 迁移文件

### 1. AccountAppService
- **原文件**：`src/DFApp.Application/Account/AccountAppService.cs`
- **新文件**：`src/DFApp.Web/Services/Account/AccountAppService.cs`

### 2. UserManagementAppService
- **原文件**：`src/DFApp.Application/Account/UserManagementAppService.cs`
- **新文件**：`src/DFApp.Web/Services/Account/UserManagementAppService.cs`

## 主要变更

### AccountAppService

| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `ApplicationService` | `AppServiceBase` |
| 用户仓储 | `IRepository<IdentityUser, Guid>` | `ISqlSugarRepository<User, Guid>` |
| 权限仓储 | `IRepository<PermissionGrant, Guid>` | `ISqlSugarRepository<PermissionGrant, Guid>` |
| 角色查询 | 导航属性 `u.Roles` | 独立 `ISqlSugarRepository<UserRole, Guid>` |
| 查询方式 | `GetQueryableAsync()` + `AsyncExecuter` | `GetQueryable()` + `.ToListAsync()` |
| 异常类型 | `UserFriendlyException` | `BusinessException` |
| 日志 | `Logger.LogWarning()` | `_logger.LogWarning()` |
| 密码更新 | 反射设置 `PasswordHash` | 直接设置 `user.PasswordHash` 属性 |
| 构造函数 | 无 `ICurrentUser`/`IPermissionChecker` | 注入 `ICurrentUser`/`IPermissionChecker` |

#### 方法迁移状态
- ✅ `LoginAsync` - 完整迁移，包含登录尝试限制、密码验证、JWT 生成
- ✅ `GenerateJwtTokenAsync` - 完整迁移，改用独立角色表查询
- ✅ `SendPasswordResetCodeAsync` - 完整迁移，包含请求频率限制
- ✅ `VerifyPasswordResetTokenAsync` - 完整迁移
- ✅ `ResetPasswordAsync` - 完整迁移，密码更新改为直接属性赋值

### UserManagementAppService

| 变更项 | 原实现 | 新实现 |
|--------|--------|--------|
| 基类 | `ApplicationService` | `AppServiceBase` |
| 用户仓储 | `IRepository<IdentityUser, Guid>` | `ISqlSugarRepository<User, Guid>` |
| 权限控制 | `[Authorize]` 特性 | `CheckPermissionAsync()` 方法调用 |
| 分页请求 | `PagedAndSortedResultRequestDto` | 自定义 `GetUserListDto` |
| 分页结果 | ABP `PagedResultDto<T>` | 自定义 `PagedResultDto<T>` |
| 异常类型 | `UserFriendlyException` | `BusinessException` |
| 用户创建 | 反射设置属性 | 直接属性赋值 |
| 用户更新 | 反射设置属性 | 直接属性赋值 |
| 密码修改 | 反射设置 `PasswordHash` | 直接属性赋值 |

#### 方法迁移状态
- ✅ `GetListAsync` - 完整迁移，使用自定义分页 DTO
- ✅ `GetAsync` - 完整迁移，添加实体存在性检查
- ✅ `CreateAsync` - 完整迁移，直接属性赋值替代反射
- ✅ `UpdateAsync` - 完整迁移，直接属性赋值替代反射
- ✅ `DeleteAsync` - 完整迁移
- ✅ `ChangePasswordAsync` - 完整迁移，直接属性赋值替代反射

## 优化改进

1. **消除反射调用**：原代码通过反射设置 `PasswordHash`、`UserName`、`Email`、`IsActive` 等属性，新代码直接通过属性赋值
2. **角色查询优化**：原代码使用 ABP 的导航属性 `u.Roles` 查询用户角色，新代码使用独立的 `UserRole` 表查询，避免导航查询
3. **权限检查方式**：从 `[Authorize]` 特性改为方法内 `CheckPermissionAsync()` 调用，更灵活且与新的权限系统一致
4. **实体存在性检查**：`GetAsync`、`UpdateAsync`、`ChangePasswordAsync` 方法中添加了 `EnsureEntityExists()` 检查
5. **异常处理改进**：`SendPasswordResetCodeAsync` 和 `ResetPasswordAsync` 中添加了 `BusinessException` 的重新抛出，避免业务异常被通用异常处理吞掉

## 未迁移的依赖

1. **DTO 类**：`LoginDto`、`LoginResultDto`、`SendPasswordResetCodeDto`、`VerifyPasswordResetTokenDto`、`ResetPasswordDto`、`UserDto`、`CreateUserDto`、`UpdateUserDto`、`ChangePasswordDto` 仍定义在 `DFApp.Application.Contracts` 项目中，通过项目引用链可用
2. **IPasswordHasher**：仍定义在 `DFApp.Domain` 项目中，通过项目引用链可用
3. **User 实体**：已迁移到 `src/DFApp.Web/Domain/Account/User.cs`，命名空间为 `DFApp.Account`
4. **Claim 构造函数**：可能存在 `System.IdentityModel.Tokens.Jwt` 版本兼容问题，需后续验证

## 新增文件

- `src/DFApp.Web/Services/Account/AccountAppService.cs`
- `src/DFApp.Web/Services/Account/UserManagementAppService.cs`

## 后续工作

1. 创建对应的 Controller（路由 `/api/app/account` 和 `/api/app/user-management`）
2. 将 DTO 类迁移到 `src/DFApp.Web/DTOs/Account/` 目录
3. 将 `IPasswordHasher` 接口迁移到 `src/DFApp.Web/Domain/Account/`
4. 验证 JWT 相关包的版本兼容性
5. 使用 Mapperly 替换手动映射
6. 在 DI 容器中注册新服务
