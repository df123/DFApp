# Playwright 测试快速开始

## 📋 前提条件

在运行测试之前，请确保：

1. ✅ Node.js 已安装 (>= 20.19.0)
2. ✅ pnpm 已安装 (>= 9)
3. ✅ 后端服务正在运行 (`https://localhost:44369`)
4. ✅ 测试用户已创建 (`test` / `1q2w3E*`)

## 🚀 快速开始

### 1. 安装依赖

```bash
cd /home/df/dfapp/DFApp.Vue
pnpm install
```

### 2. 安装 Playwright 浏览器

```bash
pnpm test:install
```

### 3. 运行测试

```bash
pnpm test
```

## 🎯 运行特定测试

### 运行单个测试文件

```bash
pnpm test tests/app.spec.ts
```

### 运行特定测试用例

```bash
pnpm test -g "should load home page"
```

### 使用 UI 模式（推荐用于调试）

```bash
pnpm test:ui
```

### 查看测试报告

```bash
pnpm test:report
```

## 🛠️ 使用测试脚本

### 基础测试脚本

```bash
./scripts/run-playwright-tests.sh
```

### 带截图的测试脚本

```bash
./scripts/run-with-screenshots.sh
```

这个脚本会：

- 🔍 检查后端服务状态
- 📦 确认 Playwright 浏览器已安装
- 🎨 提供多种运行模式选择
- 📸 选择截图选项（仅失败时 vs 所有测试）
- 🎬 选择视频录制选项

## 📸 截图功能

### 默认配置

默认情况下，仅在测试失败时保存截图：

```typescript
use: {
  screenshot: "only-on-failure";
}
```

### 启用所有截图

#### 方法 1: 使用脚本（推荐）

```bash
./scripts/run-with-screenshots.sh
```

#### 方法 2: 手动修改配置

编辑 `playwright.config.ts`：

```typescript
use: {
  screenshot: "on",  // 每个测试都保存
  video: "on",       // 每个测试都录制
}
```

详细说明：[启用测试截图文档](./docs/enable-screenshots.md)

完整文档：[Playwright 截图配置](./docs/playwright-screenshots.md)

### 查看截图

```bash
# 查找所有截图
find test-results -name "*.png" | sort

# 查看截图数量
find test-results -name "*.png" | wc -l

# 按浏览器分类
find test-results -name "*chromium*" -name "*.png"
find test-results -name "*firefox*" -name "*.png"
find test-results -name "*Mobile*" -name "*.png"
```

## 📊 测试说明

### 测试文件

| 文件                       | 说明                   |
| -------------------------- | ---------------------- |
| `tests/auth.setup.ts`      | 认证设置（获取 token） |
| `tests/app.spec.ts`        | 基本应用测试           |
| `tests/e2e.spec.ts`        | 端到端测试             |
| `tests/navigation.spec.ts` | 导航测试               |
| `tests/features.spec.ts`   | 功能特性测试           |

### 测试浏览器

- 🌐 Chromium (Chrome)
- 🦊 Firefox
- 📱 Mobile Chrome

## 🔐 认证说明

测试使用 OpenIddict Password grant 进行认证：

```
POST https://localhost:44369/connect/token

参数:
- grant_type: password
- client_id: DFApp_Web
- client_secret: X!*l}4Ab[K~um%I*#2
- username: test
- password: 1q2w3E*
```

认证状态保存在 `playwright/.auth/user.json`，所有测试自动使用。

## 🐛 调试技巧

### 1. 运行调试模式

```bash
pnpm test --debug
```

### 2. 使用 headful 模式

在测试文件中添加：

```typescript
test.use({ headless: false });

test("debug test", async ({ page }) => {
  await page.goto("/");
});
```

### 3. 慢速执行

```typescript
test.use({
  launchOptions: {
    slowMo: 500 // 每个操作后等待 500ms
  }
});
```

### 4. 查看截图和视频

失败时自动保存在 `test-results/` 目录。

### 5. 使用浏览器开发工具

在调试模式或 UI 模式下，可以使用浏览器开发工具。

## 📝 编写新测试

### 基本模板

```typescript
import { test, expect } from "@playwright/test";

test("测试描述（中文）", async ({ page }) => {
  // 1. 导航到页面
  await page.goto("/");

  // 2. 执行操作
  await page.click("button");

  // 3. 验证结果（断言消息使用中文）
  await expect(page)
    .toHaveTitle(/预期标题/)
    .toMatchSnapshot("快照名称");
});
```

### 编写规范

1. **测试描述**：
   - 使用中文描述测试名称和预期行为
   - 示例：`test("应该显示用户信息")` 而不是 `test("should display user information")`

2. **测试分组**：
   - `test.describe()` 使用中文描述
   - 示例：`test.describe("认证测试")` 而不是 `test.describe("Authentication Tests")`

3. **断言消息**：
   - 所有断言消息应使用中文
   - 示例：`expect(userInfo.username).toBe("test")` - 期望用户名为 test

4. **日志输出**：
   - 所有 `console.log()` 应使用中文
   - 示例：`console.log("找到 5 个菜单链接")` 而不是 `console.log("Found 5 menu links")`

5. **错误处理**：
   - 错误信息使用中文
   - 示例：`console.log("未找到用户元素，跳过测试")` 而不是 `console.log("User element not found, skipping")`

6. **代码注释**：
   - 注释使用中文描述
   - 示例：`// 等待页面加载完成` 而不是 `// Wait for page to load`

### 示例对比

#### ✅ 推荐写法（中文）

```typescript
import { test, expect } from "@playwright/test";

test.describe("认证测试", () => {
  test("应该显示用户信息", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const userInfo = await page.evaluate(() => {
      return JSON.parse(localStorage.getItem("user-info") || "{}");
    });

    expect(userInfo).not.toBeNull();
    expect(userInfo.username).toBe("test");
    console.log("用户信息:", userInfo);
  });

  test("应该导航到彩票页面", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const lotteryLink = page.getByRole("link", { name: /彩票/ });

    if (await lotteryLink.isVisible()) {
      await lotteryLink.click();
      await expect(page).toHaveURL(/.*lottery.*/);
      console.log("成功导航到彩票页面");
    } else {
      console.log("未找到彩票链接");
    }
  });
});
```

#### ❌ 不推荐写法（英文）

```typescript
import { test, expect } from "@playwright/test";

test.describe("Authentication Tests", () => {
  test("should display user information", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const userInfo = await page.evaluate(() => {
      return JSON.parse(localStorage.getItem("user-info") || "{}");
    });

    expect(userInfo).not.toBeNull();
    expect(userInfo.username).toBe("test");
    console.log("User info:", userInfo);
  });
});
```

### 更多示例

参见 [快速参考文档](./docs/playwright-quick-reference.md)

## 🔧 配置说明

### playwright.config.ts

```typescript
use: {
  baseURL: "http://localhost:8848",      // 前端地址
  ignoreHTTPSErrors: true,               // 忽略证书错误
  viewport: { width: 1280, height: 720 }, // 视口大小
  screenshot: "only-on-failure",         // 失败时截图（可改为 "on"）
  video: "retain-on-failure"              // 失败时录像（可改为 "on"）
}
```

## 📚 文档

- 📖 [完整测试文档](./docs/playwright-testing.md)
- 📋 [快速参考](./docs/playwright-quick-reference.md)
- 📊 [测试总结](./docs/playwright-summary.md)
- 📸 [截图配置](./docs/playwright-screenshots.md)
- 🔧 [启用截图](./docs/enable-screenshots.md)
- 🔧 [后端配置](../DFApp/docs/backend-testing-config.md)

## 🧹 清理测试数据

```bash
# 清理测试报告
rm -rf playwright-report test-results

# 清理认证状态
rm -rf playwright/.auth/*.json

# 重新运行认证设置
pnpm test tests/auth.setup.ts
```

## ❓ 常见问题

### 1. 后端服务未运行

```
Error: connect ECONNREFUSED
```

**解决方案**: 启动后端服务

```bash
cd /home/df/dfapp/DFApp
dotnet run
```

### 2. 测试用户不存在

```
Error: Invalid username or password
```

**解决方案**: 在后端创建测试用户（参考后端配置文档）

### 3. 证书错误

```
Error: self signed certificate
```

**解决方案**: Playwright 配置中已设置 `ignoreHTTPSErrors: true`，确保配置正确

### 4. 端口被占用

```
Error: Port 8848 is already in use
```

**解决方案**: 关闭占用端口的进程或修改端口配置

## 🎓 学习资源

- [Playwright 官方文档](https://playwright.dev/)
- [Playwright API 参考](https://playwright.dev/docs/api/class-playwright)
- [Playwright 最佳实践](https://playwright.dev/docs/best-practices)

## 🤝 贡献

欢迎贡献新的测试用例！请遵循以下规范：

1. ✅ 使用描述性的测试名称
2. ✅ 保持测试独立性
3. ✅ 添加适当的断言
4. ✅ 遵循现有代码风格

## 📞 支持

如有问题，请：

1. 查看 [常见问题](#常见问题)
2. 查看 [完整文档](./docs/playwright-testing.md)
3. 提交 Issue 到项目仓库

---

**Happy Testing! 🚀**

---

## 📸 当前截图和视频配置（已更新）

### 配置状态

✅ **已启用**：每次测试都保存截图和视频（包括成功的测试）

### 当前配置

```typescript
// playwright.config.ts
use: {
  screenshot: "on",              // 每个测试都保存截图
  video: "on",                   // 每个测试都录制视频
  trace: "on-first-retry",       // 首次重试时保存 trace
}
```

### 生成的文件

每个测试会在 `test-results/{test-name}-{browser}/` 目录下生成：

- `test-finished-1.png` - 测试完成时的截图
- `video.webm` - 测试过程录制的视频

### 查看截图和视频

```bash
# 查找所有截图
find test-results -name "*.png"

# 查找所有视频
find test-results -name "*.webm"

# 统计数量
echo "截图: $(find test-results -name "*.png" | wc -l)"
echo "视频: $(find test-results -name "*.webm" | wc -l)"
```

### 配置选项

| 选项       | 值                    | 说明                       |
| ---------- | --------------------- | -------------------------- |
| screenshot | `"on"`                | 每个测试都保存截图（当前） |
| screenshot | `"only-on-failure"`   | 仅失败时保存               |
| screenshot | `"off"`               | 不保存                     |
| video      | `"on"`                | 每个测试都录制视频（当前） |
| video      | `"retain-on-failure"` | 每个都录制但删除成功的     |
| video      | `"on-first-retry"`    | 仅首次重试时录制           |
| video      | `"off"`               | 不录制                     |

详细文档：[保存所有截图和视频](./docs/save-all-screenshots-and-videos.md)

### 修改配置

如需修改为仅失败时保存：

```bash
# 编辑 playwright.config.ts
# 将 screenshot: "on" 改为 screenshot: "only-on-failure"
# 将 video: "on" 改为 video: "retain-on-failure"
```
