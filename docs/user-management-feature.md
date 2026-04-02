# 用户管理功能

## 概述

用户管理功能允许管理员管理系统的用户账户。由于系统是个人使用，不允许公开注册，只能由管理员添加新用户。

## 功能特性

### 1. 移除公开注册接口
- 已移除 [`IAccountAppService.RegisterAsync()`](src/DFApp.Application.Contracts/Account/IAccountAppService.cs) 方法
- 已删除 [`RegisterDto`](src/DFApp.Application.Contracts/Account/RegisterDto.cs) 和 [`RegisterResultDto`](src/DFApp.Application.Contracts/Account/RegisterResultDto.cs)
- 已移除 [`AccountAppService.RegisterAsync()`](src/DFApp.Application/Account/AccountAppService.cs) 实现

### 2. 用户管理功能
管理员可以执行以下操作：
- 查看用户列表（分页）
- 创建新用户
- 编辑用户信息
- 删除用户
- 修改用户密码
- 激用/停用用户

### 3. 权限控制
用户管理功能使用以下权限：
- `DFApp.UserManagement.Default` - 查看用户列表
- `DFApp.UserManagement.Create` - 创建用户
- `DFApp.UserManagement.Update` - 更新用户
- `DFApp.UserManagement.Delete` - 删除用户
- `DFApp.UserManagement.ChangePassword` - 修改密码

## 后端实现

### DTO
- [`UserDto`](src/DFApp.Web/DTOs/Account/UserDto.cs) - 用户信息DTO
- [`CreateUserDto`](src/DFApp.Web/DTOs/Account/CreateUserDto.cs) - 创建用户DTO
- [`UpdateUserDto`](src/DFApp.Web/DTOs/Account/UpdateUserDto.cs) - 更新用户DTO
- [`ChangePasswordDto`](src/DFApp.Web/DTOs/Account/ChangePasswordDto.cs) - 修改密码DTO

### 应用服务
- [`IUserManagementAppService`](src/DFApp.Application.Contracts/Account/IUserManagementAppService.cs) - 用户管理服务接口（旧，待迁移）
- [`UserManagementAppService`](src/DFApp.Web/Services/Account/UserManagementAppService.cs) - 用户管理服务实现

### 权限定义
- [`DFAppPermissions.UserManagement`](src/DFApp.Application.Contracts/Permissions/DFAppPermissions.cs) - 用户管理权限常量
- [`DFAppPermissionDefinitionProvider`](src/DFApp.Application.Contracts/Permissions/DFAppPermissionDefinitionProvider.cs) - 权限定义提供者

### 本地化资源
- [`zh-Hans.json`](src/DFApp.Domain.Shared/Localization/DFApp/zh-Hans.json) - 中文本地化资源

## 前端实现

### 页面
- [`/views/user-management/index.vue`](DFApp.Vue/src/views/user-management/index.vue) - 用户管理页面

### 路由
- [`/router/modules/system.ts`](DFApp.Vue/src/router/modules/system.ts) - 系统管理路由配置

## API 接口

### 获取用户列表
```
GET /api/app/user-management
```

参数：
- `skipCount` - 跳过的记录数
- `maxResultCount` - 最大返回记录数
- `sorting` - 排序字段

### 获取用户详情
```
GET /api/app/user-management/{id}
```

### 创建用户
```
POST /api/app/user-management
```

请求体：
```json
{
  "userName": "string",
  "email": "string",
  "password": "string",
  "isActive": true
}
```

### 更新用户
```
PUT /api/app/user-management/{id}
```

请求体：
```json
{
  "userName": "string",
  "email": "string",
  "isActive": true
}
```

### 删除用户
```
DELETE /api/app/user-management/{id}
```

### 修改密码
```
POST /api/app/user-management/change-password
```

请求体：
```json
{
  "userId": "guid",
  "newPassword": "string"
}
```

## 使用说明

1. 管理员登录系统
2. 导航到"系统管理" -> "用户管理"
3. 使用"新增用户"按钮创建新用户
4. 可以编辑、删除用户或修改用户密码
5. 可以激活或停用用户账户

## 注意事项

1. 只有拥有相应权限的用户才能访问用户管理功能
2. 不能删除当前登录的用户
3. 用户名和邮箱在系统中必须唯一
4. 密码长度必须在6-100个字符之间
5. 系统使用现有的 IdentityUser 表，无需数据库迁移
