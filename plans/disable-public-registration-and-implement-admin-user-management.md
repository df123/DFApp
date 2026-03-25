# 禁用公开注册并实现管理员用户管理功能

## 概述
本文档描述了如何禁用公开的用户注册功能，并实现管理员专用的用户管理功能。

## 背景
- 当前系统允许任何人通过公开API注册用户（`AccountAppService.RegisterAsync`）
- 系统是内部系统，不允许外部用户自行注册
- 只有管理员可以在权限管理模块中注册新用户
- 前端权限管理页面已存在，但后端缺少对应的Identity用户管理服务

## 目标
1. 完全移除公开的注册接口（`RegisterAsync`）
2. 创建Identity用户管理服务，供管理员使用
3. 确保只有具有适当权限的管理员才能创建和管理用户
4. 更新前端，移除注册页面相关代码

## 实施步骤

### 1. 创建Identity用户管理相关的DTO定义

#### 1.1 创建 IdentityUserDto.cs
**路径**: `src/DFApp.Application.Contracts/IdentityUser/IdentityUserDto.cs`

```csharp
using System;
using System.Collections.Generic;

namespace DFApp.IdentityUser;

/// <summary>
/// 用户数据传输对象
/// </summary>
public class IdentityUserDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 姓氏
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 邮箱是否已确认
    /// </summary>
    public bool? EmailConfirmed { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 手机号是否已确认
    /// </summary>
    public bool? PhoneNumberConfirmed { get; set; }

    /// <summary>
    /// 是否启用双因素认证
    /// </summary>
    public bool? TwoFactorEnabled { get; set; }

    /// <summary>
    /// 是否启用锁定
    /// </summary>
    public bool? LockoutEnabled { get; set; }

    /// <summary>
    /// 锁定结束时间
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>
    /// 并发标记
    /// </summary>
    public string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreationTime { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 角色名称列表
    /// </summary>
    public List<string>? RoleNames { get; set; }
}
```

#### 1.2 创建 CreateIdentityUserDto.cs
**路径**: `src/DFApp.Application.Contracts/IdentityUser/CreateIdentityUserDto.cs`

```csharp
using System.Collections.Generic;

namespace DFApp.IdentityUser;

/// <summary>
/// 创建用户请求DTO
/// </summary>
public class CreateIdentityUserDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 姓氏
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 角色名称列表
    /// </summary>
    public List<string>? RoleNames { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
}
```

#### 1.3 创建 UpdateIdentityUserDto.cs
**路径**: `src/DFApp.Application.Contracts/IdentityUser/UpdateIdentityUserDto.cs`

```csharp
using System.Collections.Generic;

namespace DFApp.IdentityUser;

/// <summary>
/// 更新用户请求DTO
/// </summary>
public class UpdateIdentityUserDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 姓氏
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 并发标记
    /// </summary>
    public string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// 角色名称列表
    /// </summary>
    public List<string>? RoleNames { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
}
```

#### 1.4 创建 GetIdentityUsersInput.cs
**路径**: `src/DFApp.Application.Contracts/IdentityUser/GetIdentityUsersInput.cs`

```csharp
namespace DFApp.IdentityUser;

/// <summary>
/// 获取用户列表请求DTO
/// </summary>
public class GetIdentityUsersInput
{
    /// <summary>
    /// 跳过的记录数
    /// </summary>
    public int SkipCount { get; set; }

    /// <summary>
    /// 最大返回记录数
    /// </summary>
    public int MaxResultCount { get; set; } = 10;

    /// <summary>
    /// 过滤条件
    /// </summary>
    public string? Filter { get; set; }
}
```

### 2. 创建Identity用户管理服务接口

#### 2.1 创建 IIdentityUserAppService.cs
**路径**: `src/DFApp.Application.Contracts/IdentityUser/IIdentityUserAppService.cs`

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.IdentityUser;

/// <summary>
/// Identity用户应用服务接口
/// </summary>
public interface IIdentityUserAppService : IApplicationService
{
    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>用户列表和总数</returns>
    Task<(List<IdentityUserDto> Items, int TotalCount)> GetListAsync(GetIdentityUsersInput input);

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息</returns>
    Task<IdentityUserDto> GetAsync(Guid id);

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="input">创建用户请求</param>
    Task CreateAsync(CreateIdentityUserDto input);

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="input">更新用户请求</param>
    Task UpdateAsync(Guid id, UpdateIdentityUserDto input);

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    Task DeleteAsync(Guid id);
}
```

### 3. 实现Identity用户管理服务

#### 3.1 创建 IdentityUserAppService.cs
**路径**: `src/DFApp.Application/IdentityUser/IdentityUserAppService.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using DFApp.IdentityUser;
using DFApp.Account;
using DFApp.Permissions;

namespace DFApp.IdentityUser;

/// <summary>
/// Identity用户应用服务实现
/// </summary>
[Authorize]
public class IdentityUserAppService : ApplicationService, IIdentityUserAppService
{
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public IdentityUserAppService(
        IRepository<IdentityUser, Guid> userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    public async Task<(List<IdentityUserDto> Items, int TotalCount)> GetListAsync(GetIdentityUsersInput input)
    {
        var queryable = await _userRepository.GetQueryableAsync();

        // 应用过滤条件
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(u =>
                u.UserName.Contains(input.Filter) ||
                (u.Email != null && u.Email.Contains(input.Filter)) ||
                (u.Name != null && u.Name.Contains(input.Filter)) ||
                (u.Surname != null && u.Surname.Contains(input.Filter)));
        }

        // 获取总数
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // 分页
        var users = await AsyncExecuter.ToListAsync(
            queryable
                .OrderBy(u => u.UserName)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        // 转换为DTO
        var items = users.Select(MapToDto).ToList();

        return (items, totalCount);
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    public async Task<IdentityUserDto> GetAsync(Guid id)
    {
        var queryable = await _userRepository.GetQueryableAsync();
        var user = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(u => u.Id == id));

        if (user == null)
        {
            throw new UserFriendlyException("用户不存在");
        }

        return MapToDto(user);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [Authorize(DFAppPermissions.IdentityUsers.Create)]
    public async Task CreateAsync(CreateIdentityUserDto input)
    {
        // 检查用户名是否已存在
        var queryable = await _userRepository.GetQueryableAsync();
        var existingUser = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(u => u.UserName == input.UserName));

        if (existingUser != null)
        {
            throw new UserFriendlyException("用户名已存在");
        }

        // 检查邮箱是否已存在
        if (!string.IsNullOrEmpty(input.Email))
        {
            var existingEmailUser = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(u => u.Email == input.Email));
            if (existingEmailUser != null)
            {
                throw new UserFriendlyException("邮箱已被使用");
            }
        }

        // 创建新用户
        var user = new IdentityUser(Guid.NewGuid(), input.UserName, input.Email)
        {
            Name = input.Name,
            Surname = input.Surname,
            PhoneNumber = input.PhoneNumber,
            IsActive = input.IsActive ?? true
        };

        // 设置密码哈希
        var passwordHash = _passwordHasher.HashPassword(input.Password);
        var passwordHashProperty = typeof(IdentityUser).GetProperty("PasswordHash");
        passwordHashProperty?.SetValue(user, passwordHash);

        await _userRepository.InsertAsync(user);

        Logger.LogInformation($"管理员创建用户成功：{input.UserName}");
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [Authorize(DFAppPermissions.IdentityUsers.Update)]
    public async Task UpdateAsync(Guid id, UpdateIdentityUserDto input)
    {
        var queryable = await _userRepository.GetQueryableAsync();
        var user = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(u => u.Id == id));

        if (user == null)
        {
            throw new UserFriendlyException("用户不存在");
        }

        // 检查用户名是否被其他用户使用
        if (user.UserName != input.UserName)
        {
            var existingUser = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(u => u.UserName == input.UserName && u.Id != id));
            if (existingUser != null)
            {
                throw new UserFriendlyException("用户名已被使用");
            }
        }

        // 检查邮箱是否被其他用户使用
        if (!string.IsNullOrEmpty(input.Email) && user.Email != input.Email)
        {
            var existingEmailUser = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(u => u.Email == input.Email && u.Id != id));
            if (existingEmailUser != null)
            {
                throw new UserFriendlyException("邮箱已被使用");
            }
        }

        // 更新用户信息
        user.UserName = input.UserName;
        user.Name = input.Name;
        user.Surname = input.Surname;
        user.Email = input.Email;
        user.PhoneNumber = input.PhoneNumber;
        user.IsActive = input.IsActive ?? user.IsActive;
        user.ConcurrencyStamp = input.ConcurrencyStamp;

        await _userRepository.UpdateAsync(user);

        Logger.LogInformation($"管理员更新用户成功：{input.UserName}");
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [Authorize(DFAppPermissions.IdentityUsers.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var queryable = await _userRepository.GetQueryableAsync();
        var user = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(u => u.Id == id));

        if (user == null)
        {
            throw new UserFriendlyException("用户不存在");
        }

        await _userRepository.DeleteAsync(user);

        Logger.LogInformation($"管理员删除用户成功：{user.UserName}");
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    private IdentityUserDto MapToDto(IdentityUser user)
    {
        return new IdentityUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            ConcurrencyStamp = user.ConcurrencyStamp,
            CreationTime = user.CreationTime,
            IsActive = user.IsActive,
            RoleNames = null // 角色信息需要单独查询
        };
    }
}
```

### 4. 添加权限定义

#### 4.1 更新 DFAppPermissions.cs
**路径**: `src/DFApp.Application.Contracts/Permissions/DFAppPermissions.cs`

在文件末尾添加以下内容：

```csharp
    public static class IdentityUsers
    {
        public const string Default = GroupName + ".IdentityUsers";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
```

#### 4.2 更新 DFAppPermissionDefinitionProvider.cs
**路径**: `src/DFApp.Application.Contracts/Permissions/DFAppPermissionDefinitionProvider.cs`

在权限定义提供者中添加Identity用户管理权限：

```csharp
var identityUsersGroup = context.GetGroupOrNull(DFAppPermissions.GroupName)
    ?? context.AddGroup(DFAppPermissions.GroupName, "DFApp");

var identityUsersPermission = identityUsersGroup.AddPermission(
    DFAppPermissions.IdentityUsers.Default,
    "Identity用户管理");
identityUsersPermission.AddChild(DFAppPermissions.IdentityUsers.Create, "创建用户");
identityUsersPermission.AddChild(DFAppPermissions.IdentityUsers.Update, "更新用户");
identityUsersPermission.AddChild(DFAppPermissions.IdentityUsers.Delete, "删除用户");
```

### 5. 移除公开注册接口

#### 5.1 更新 IAccountAppService.cs
**路径**: `src/DFApp.Application.Contracts/Account/IAccountAppService.cs`

移除 `RegisterAsync` 方法的声明。

#### 5.2 更新 AccountAppService.cs
**路径**: `src/DFApp.Application/Account/AccountAppService.cs`

移除 `RegisterAsync` 方法的实现（第142-204行）。

#### 5.3 删除DTO文件
删除以下文件：
- `src/DFApp.Application.Contracts/Account/RegisterDto.cs`
- `src/DFApp.Application.Contracts/Account/RegisterResultDto.cs`

### 6. 更新前端

#### 6.1 移除注册页面
删除或禁用前端注册页面：
- `DFApp.Vue/src/views/login/register.vue`（如果存在）

#### 6.2 更新路由配置
从路由配置中移除注册页面的路由。

#### 6.3 更新登录页面
如果登录页面有"注册"链接，移除该链接。

### 7. 更新文档

#### 7.1 更新后端文档
在 `docs/backend-testing-config.md` 或相关文档中更新用户管理相关的说明。

#### 7.2 创建或更新用户管理文档
创建新文档说明管理员如何使用权限管理模块来创建和管理用户。

## 注意事项

1. **权限控制**：Identity用户管理服务的Create、Update、Delete方法都需要相应的权限，确保只有授权的管理员才能执行这些操作。

2. **密码哈希**：使用项目现有的 `IPasswordHasher` 接口进行密码哈希，保持一致性。

3. **日志记录**：所有用户管理操作都应记录日志，便于审计。

4. **并发控制**：更新用户时使用 `ConcurrencyStamp` 进行并发控制。

5. **角色管理**：当前实现中角色信息未完全集成，如需要角色管理功能，需要进一步实现。

6. **数据验证**：在DTO中添加适当的数据验证特性（如 `[Required]`、`[EmailAddress]` 等）。

7. **前端API路径**：前端调用的API路径是 `/api/identity/users`，需要确保后端服务正确映射到这个路径。

## 测试计划

1. 测试管理员创建用户功能
2. 测试管理员更新用户功能
3. 测试管理员删除用户功能
4. 测试用户列表查询功能
5. 验证公开注册接口已被移除
6. 验证权限控制是否正常工作
7. 验证前端权限管理页面是否正常工作

## 回滚计划

如果需要回滚，可以：
1. 恢复 `RegisterAsync` 方法
2. 恢复 `RegisterDto` 和 `RegisterResultDto` 文件
3. 删除Identity用户管理相关的代码
4. 恢复前端注册页面
