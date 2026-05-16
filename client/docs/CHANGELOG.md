# Playwright 测试更新日志

## 2026-02-03

### 新增功能

#### 1. Playwright 配置

- 创建 `playwright.config.ts` 配置文件
- 配置支持 Chromium、Firefox、WebKit 浏览器
- 配置支持移动端测试
- 设置 `ignoreHTTPSErrors: true` 支持自签名证书
- 配置认证状态保存和恢复
- 配置截图和视频录制
- 配置 Web 服务器自动启动

#### 2. 认证设置

- 创建 `tests/auth.setup.ts` 认证设置文件
- 实现 OpenIddict Password grant 认证
- 自动获取和保存 token 到 `playwright/.auth/user.json`
- 配置 localStorage 中的 `user-info`

#### 3. 测试套件

- **tests/app.spec.ts** - 基本应用功能测试
  - 首页加载测试
  - 用户信息验证
  - 导航测试

- **tests/e2e.spec.ts** - 端到端测试
  - 页面加载测试
  - UI 组件测试
  - API 响应测试
  - 用户界面测试

- **tests/navigation.spec.ts** - 导航测试
  - 彩票页面导航
  - 记账页面导航
  - 订阅页面导航
  - 系统页面导航
  - 退出登录测试

- **tests/features.spec.ts** - 功能特性测试
  - 彩票管理功能
  - 记账功能
  - 下载订阅功能
  - 系统设置功能
  - 响应式设计测试
  - 性能测试

#### 4. 文档

- **docs/playwright-testing.md** - 完整测试文档
  - 安装和运行说明
  - 认证流程说明
  - 配置说明
  - 故障排查指南

- **docs/playwright-quick-reference.md** - 快速参考
  - 常用测试模式
  - 选择器技巧
  - 断言方法
  - 最佳实践

- **docs/playwright-summary.md** - 测试总结
  - 已完成工作清单
  - 文件结构
  - 使用流程
  - 关键特性

- **docs/backend-testing-config.md** - 后端测试配置（DFApp 目录）
  - 测试用户创建
  - OpenIddict 配置
  - 自签名证书说明

- **TESTING.md** - 快速开始指南
  - 前提条件
  - 快速开始步骤
  - 运行特定测试
  - 调试技巧
  - 常见问题

#### 5. 脚本

- **scripts/run-playwright-tests.sh** - 测试启动脚本
  - 自动检查后端服务
  - 交互式菜单
  - 支持多种运行模式

- **scripts/verify-playwright.sh** - 配置验证脚本
  - 检查项目结构
  - 验证配置文件
  - 检查后端服务
  - 提供快速开始指南

#### 6. Package.json 更新

- 添加 `@playwright/test` 依赖
- 添加测试脚本：
  - `pnpm test` - 运行所有测试
  - `pnpm test:ui` - UI 模式
  - `pnpm test:report` - 查看报告
  - `pnpm test:install` - 安装浏览器

#### 7. 配置文件

- **.env.test** - 测试环境配置
- **.gitignore** - 忽略测试产物
  - playwright-report/
  - test-results/
  - playwright/.auth/\*.json

#### 8. 目录结构

- 创建 `playwright/.auth/` 目录
- 添加 `.gitkeep` 文件确保目录被提交

### 主要特性

#### 1. 自签名证书支持

- `ignoreHTTPSErrors: true` 配置
- 自动信任自签名证书

#### 2. 认证状态管理

- Setup 测试获取 token
- 自动保存和恢复状态
- localStorage 中的 user-info

#### 3. 多浏览器支持

- Chromium (Chrome)
- Firefox
- WebKit (Safari)
- Mobile Chrome

#### 4. 测试报告

- HTML 报告
- 视频录制
- 截图（失败时）
- Trace 文件

#### 5. 调试支持

- UI 模式
- 调试模式
- 慢速执行
- 截图和视频

### 使用说明

#### 首次设置

```bash
# 安装依赖
pnpm install

# 安装 Playwright 浏览器
pnpm test:install

# 在后端创建测试用户
# 参考: docs/backend-testing-config.md
```

#### 运行测试

```bash
# 运行所有测试
pnpm test

# UI 模式
pnpm test:ui

# 查看报告
pnpm test:report

# 使用脚本
./scripts/run-playwright-tests.sh
```

### 认证流程

1. 运行 `tests/auth.setup.ts`
2. POST 请求到 `/connect/token`
3. 获取 `access_token` 和 `refresh_token`
4. 保存到 `playwright/.auth/user.json`
5. 其他测试自动加载认证状态
6. `user-info` 存储在 localStorage

### 注意事项

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

### 下一步

可以添加的测试：

- 更多功能测试
- 表单验证
- 数据导出
- 文件上传
- 性能测试
- 可访问性测试
- 安全测试
- 视觉回归测试

可以优化的地方：

- Page Object Model
- 测试数据管理
- 并行测试
- CI/CD 集成

### 参考资源

- [Playwright 官方文档](https://playwright.dev/)
- [ABP Framework 文档](https://docs.abp.io/)
- [Vue 3 文档](https://vuejs.org/)
- [Element Plus 文档](https://element-plus.org/)
