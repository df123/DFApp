# Playwright 自动测试

## 概述

本项目的 Playwright 测试用于自动化测试前端应用，使用 OpenIddict Password grant 进行认证。

## 前置条件

确保满足以下条件：

- 后端服务运行在 `https://localhost:44369`
- 测试用户 `test` 已创建（密码：`1q2w3E*`）
- 自签名证书已被信任（测试时自动处理）

## 安装 Playwright

```bash
pnpm install
pnpm test:install
```

## 运行测试

### 运行所有测试

```bash
pnpm test
```

### 运行测试 UI 模式

```bash
pnpm test:ui
```

### 查看测试报告

```bash
pnpm test:report
```

## 认证流程

测试使用 OpenIddict Password grant 获取 token：

1. **Setup 测试** (`tests/auth.setup.ts`)
   - POST 请求到 `/connect/token`
   - 使用以下参数：
     - `grant_type=password`
     - `client_id=DFApp_Web`
     - `client_secret=X!*l}4Ab[K~um%I*#2`
     - `username=test`
     - `password=1q2w3E*`
   - 将 token 保存到 `playwright/.auth/user.json`

2. **其他测试** 自动使用保存的认证状态
   - `user-info` 存储在 localStorage 中
   - 包含访问令牌、刷新令牌、用户角色等信息

## 测试文件结构

```
tests/
├── auth.setup.ts      # 认证设置（每次测试前运行）
└── app.spec.ts        # 应用功能测试
```

## 配置说明

### playwright.config.ts

- `ignoreHTTPSErrors: true` - 忽略自签名证书错误
- `baseURL: "http://localhost:8848"` - 前端服务地址
- `storageState` - 保存和恢复认证状态
- `webServer` - 自动启动前端开发服务器

### 认证状态文件

认证状态保存在 `playwright/.auth/user.json`，包含：

- Cookies
- localStorage
- sessionStorage
- IndexedDB

## 编写新测试

### 创建测试文件

在 `tests/` 目录下创建新的 `.spec.ts` 文件：

```typescript
import { test, expect } from "@playwright/test";

test("my new test", async ({ page }) => {
  await page.goto("/");
  // 测试逻辑
});
```

### 认证状态

所有测试自动使用已保存的认证状态，无需重复登录。

## 调试技巧

### 查看测试运行

```bash
pnpm test --ui
```

### 查看详细日志

```bash
pnpm test --debug
```

### 查看浏览器运行

在测试文件中添加 `--headed` 标志或使用 `test.use({ headless: false })`：

```typescript
test.use({ headless: false });

test("debug test", async ({ page }) => {
  await page.goto("/");
});
```

## 故障排查

### 后端服务未运行

确保后端服务已启动：

```bash
cd /home/df/dfapp/DFApp
dotnet run
```

### 测试用户不存在

确保在数据库中创建测试用户，用户名：`test`，密码：`1q2w3E*`

### 证书错误

如果遇到证书错误，确保 `ignoreHTTPSErrors: true` 已在配置中设置。

## 清理测试数据

```bash
# 清理测试报告
rm -rf playwright-report test-results

# 清理认证状态
rm -rf playwright/.auth/*.json
```
