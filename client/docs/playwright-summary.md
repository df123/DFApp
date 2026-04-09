# Playwright 测试设置总结

## 已完成的工作

### 1. 配置文件

#### playwright.config.ts

- ✅ 配置了测试目录和浏览器
- ✅ 设置了 `ignoreHTTPSErrors: true` 以支持自签名证书
- ✅ 配置了 Web 服务器自动启动
- ✅ 设置了认证状态保存和恢复
- ✅ 配置了截图和视频录制

### 2. 认证设置

#### tests/auth.setup.ts

- ✅ 使用 OpenIddict Password grant 获取 token
- ✅ POST 请求到 `/connect/token`
- ✅ 将认证状态保存到 `playwright/.auth/user.json`
- ✅ 包含 `user-info` 在 localStorage 中

### 3. 测试文件

#### tests/app.spec.ts

- ✅ 基本应用功能测试
- ✅ 用户信息验证
- ✅ 页面导航测试

#### tests/e2e.spec.ts

- ✅ 端到端测试
- ✅ 页面加载测试
- ✅ UI 组件测试
- ✅ API 响应测试

#### tests/navigation.spec.ts

- ✅ 页面导航测试
- ✅ 各模块页面访问测试
- ✅ 退出登录测试

#### tests/features.spec.ts

- ✅ 彩票管理测试
- ✅ 记账功能测试
- ✅ 下载订阅测试
- ✅ 系统设置测试
- ✅ 响应式设计测试
- ✅ 性能测试

### 4. 文档

#### docs/playwright-testing.md

- ✅ 完整的测试文档
- ✅ 安装和运行说明
- ✅ 认证流程说明
- ✅ 故障排查指南

#### docs/playwright-quick-reference.md

- ✅ 快速参考指南
- ✅ 常用测试模式
- ✅ 选择器技巧
- ✅ 断言方法
- ✅ 最佳实践

#### docs/backend-testing-config.md（后端）

- ✅ 后端测试配置文档
- ✅ 测试用户创建指南
- ✅ OpenIddict 配置说明
- ✅ 自签名证书说明

### 5. 脚本

#### scripts/run-playwright-tests.sh

- ✅ 测试启动脚本
- ✅ 自动检查后端服务
- ✅ 支持多种运行模式
- ✅ 交互式菜单

### 6. 其他配置

#### package.json

- ✅ 添加了 `playwright` 依赖
- ✅ 添加了测试脚本：
  - `pnpm test` - 运行所有测试
  - `pnpm test:ui` - UI 模式
  - `pnpm test:report` - 查看报告
  - `pnpm test:install` - 安装浏览器

#### .gitignore

- ✅ 忽略测试报告和认证状态文件
- ✅ 保留 `.gitkeep` 文件

## 文件结构

```
DFApp.Vue/
├── playwright.config.ts              # Playwright 配置
├── package.json                       # 包含测试脚本
├── .gitignore                         # 忽略测试产物
├── docs/
│   ├── playwright-testing.md         # 测试文档
│   └── playwright-quick-reference.md  # 快速参考
├── scripts/
│   └── run-playwright-tests.sh       # 测试启动脚本
├── tests/
│   ├── auth.setup.ts                  # 认证设置
│   ├── app.spec.ts                    # 基本测试
│   ├── e2e.spec.ts                    # E2E 测试
│   ├── navigation.spec.ts             # 导航测试
│   └── features.spec.ts               # 功能测试
└── playwright/
    └── .auth/
        ├── .gitkeep                   # 确保目录被提交
        └── user.json                  # 认证状态（自动生成，不提交）
```

DFApp/
└── docs/
└── backend-testing-config.md # 后端测试配置

## 使用流程

### 1. 首次设置

```bash
# 安装依赖
cd /home/df/dfapp/DFApp.Vue
pnpm install

# 安装 Playwright 浏览器
pnpm test:install

# 在后端创建测试用户（参考 docs/backend-testing-config.md）
cd /home/df/dfapp/DFApp
dotnet run
```

### 2. 运行测试

```bash
# 使用脚本（推荐）
cd /home/df/dfapp/DFApp.Vue
./scripts/run-playwright-tests.sh

# 或直接运行
pnpm test

# 或使用 UI 模式
pnpm test:ui

# 或查看报告
pnpm test:report
```

### 3. 查看文档

- [测试文档](./docs/playwright-testing.md)
- [快速参考](./docs/playwright-quick-reference.md)
- [后端配置](../DFApp/docs/backend-testing-config.md)

## 认证流程

```
1. 运行 auth.setup.ts
   ↓
2. POST /connect/token
   参数:
   - grant_type=password
   - client_id=DFApp_Web
   - client_secret=X!*l}4Ab[K~um%I*#2
   - username=test
   - password=1q2w3E*
   ↓
3. 获取 access_token 和 refresh_token
   ↓
4. 保存到 playwright/.auth/user.json
   ↓
5. 其他测试自动加载认证状态
   ↓
6. user-info 存储在 localStorage
```

## 关键特性

### 1. 自签名证书支持

- `ignoreHTTPSErrors: true` 配置
- 自动信任自签名证书

### 2. 认证状态管理

- Setup 测试获取 token
- 自动保存和恢复状态
- localStorage 中的 user-info

### 3. 多浏览器支持

- Chromium
- Firefox
- 可扩展到 WebKit

### 4. 测试报告

- HTML 报告
- 视频录制
- 截图（失败时）
- Trace 文件

### 5. 调试支持

- UI 模式
- 调试模式
- 慢速执行
- 截图和视频

## 下一步

### 可以添加的测试

1. **更多功能测试**
   - 表单验证
   - 数据导出
   - 文件上传
   - 打印功能

2. **性能测试**
   - 页面加载时间
   - API 响应时间
   - 内存使用

3. **可访问性测试**
   - ARIA 属性
   - 键盘导航
   - 屏幕阅读器支持

4. **安全测试**
   - XSS 防护
   - CSRF 防护
   - 权限控制

5. **视觉回归测试**
   - 截图对比
   - UI 一致性

### 可以优化的地方

1. **Page Object Model**
   - 创建页面类
   - 封装页面操作
   - 提高可维护性

2. **测试数据管理**
   - 使用测试数据工厂
   - 数据库状态管理
   - 测试数据清理

3. **并行测试**
   - 提高测试速度
   - 分组测试
   - 依赖管理

4. **CI/CD 集成**
   - GitHub Actions
   - 自动化测试
   - 测试报告通知

## 注意事项

1. **测试用户必须存在**
   - 用户名: `test`
   - 密码: `1q2w3E*`
   - 角色: `Admin`

2. **后端服务必须运行**
   - 端口: `44369`
   - HTTPS 配置
   - 自签名证书

3. **认证状态文件不提交**
   - `.gitignore` 已配置
   - 只提交 `.gitkeep`
   - 每次运行时重新生成

4. **测试隔离**
   - 每个测试独立
   - 不依赖执行顺序
   - 清理测试数据

## 参考资源

- [Playwright 官方文档](https://playwright.dev/)
- [ABP Framework 文档](https://docs.abp.io/)
- [Vue 3 文档](https://vuejs.org/)
- [Element Plus 文档](https://element-plus.org/)
