# Playwright 测试截图配置

## 配置说明

Playwright 配置已启用每个测试保存截图功能：

```typescript
// playwright.config.ts
use: {
  screenshot: "on",              // 每个测试都保存截图
  video: "retain-on-failure",     // 失败时保存视频
  trace: "retain-on-failure",     // 失败时保存 trace
}
```

## 截图选项

| 选项                | 说明                                                  |
| ------------------- | ----------------------------------------------------- |
| `"off"`             | 不保存截图                                            |
| `"on"`              | 每个测试都保存截图（默认名称：`test-finished-1.png`） |
| `"only-on-failure"` | 仅在测试失败时保存截图                                |

## 截图命名规则

### 默认命名

```
test-results/{test-name}-{browser}/test-finished-1.png
```

示例：

```
test-results/app-Authentication-Tests-should-display-user-information-chromium/test-finished-1.png
test-results/app-Authentication-Tests-should-display-user-information-firefox/test-finished-1.png
test-results/app-Authentication-Tests-should-display-user-information-Mobile-Chrome/test-finished-1.png
```

### 自定义命名

可以在测试中手动截图并指定名称：

```typescript
import { test, expect } from "@playwright/test";

test("my test", async ({ page }) => {
  await page.goto("/");

  // 手动截图
  await page.screenshot({
    path: "screenshots/home-page.png",
    fullPage: true
  });

  // 或使用相对路径
  await page.screenshot({
    path: "my-screenshot.png",
    fullPage: false
  });
});
```

## 截图类型

### 1. 测试完成时截图

```typescript
// playwright.config.ts
use: {
  screenshot: "on";
}
```

### 2. 测试失败时截图

```typescript
// playwright.config.ts
use: {
  screenshot: "only-on-failure";
}
```

### 3. 手动截图

```typescript
// 截取视口区域
await page.screenshot({ path: "screenshot.png" });

// 截取完整页面
await page.screenshot({
  path: "full-page.png",
  fullPage: true
});

// 截取元素
const element = page.locator(".my-element");
await element.screenshot({ path: "element.png" });
```

## 查看截图

### 使用文件浏览器

截图保存在 `test-results/` 目录下：

```bash
# 查看所有截图
find test-results -name "*.png" | sort

# 查看截图数量
find test-results -name "*.png" | wc -l

# 查看最近的截图
ls -lt test-results/*/*.png | head -10
```

### 使用 Playwright HTML 报告

```bash
pnpm test:report
```

在 HTML 报告中，每个测试都有截图缩略图。

## 按浏览器查看截图

```bash
# Chromium 截图
find test-results -name "*chromium" -name "*.png"

# Firefox 截图
find test-results -name "*firefox" -name "*.png"

# Mobile Chrome 截图
find test-results -name "*Mobile*" -name "*.png"
```

## 按测试名称查看截图

```bash
# 查看特定测试的截图
find test-results -name "*test-name*" -name "*.png"

# 例如：查看认证测试的截图
find test-results -name "*Authentication*" -name "*.png"
```

## 截图质量设置

可以在配置中设置截图质量（仅适用于 JPEG）：

```typescript
use: {
  screenshot: "on",
  screenshot: {
    path: "test-results/",
    type: "jpeg",        // 'png' 或 'jpeg'
    quality: 80          // 0-100（仅 JPEG）
  }
}
```

## 自动化处理截图

### 批量查看截图

```bash
# 使用图片查看器打开所有截图
eog test-results/*/*.png

# 或使用 feh (Linux)
feh test-results/*/*.png

# 或使用 open (macOS)
open test-results/*/*.png
```

### 生成截图索引

```bash
# 创建截图索引文件
find test-results -name "*.png" | sort > screenshots-index.txt

# 查看索引
cat screenshots-index.txt
```

## 清理截图

```bash
# 删除所有截图
find test-results -name "*.png" -delete

# 删除特定测试的截图
find test-results -name "*test-name*" -name "*.png" -delete

# 删除旧截图（7天前）
find test-results -name "*.png" -mtime +7 -delete
```

## 当前项目统计

运行以下命令查看截图统计：

```bash
# 查看截图总数
find test-results -name "*.png" | wc -l

# 查看截图总大小
du -sh test-results/

# 按浏览器统计
echo "Chromium: $(find test-results -name "*chromium*" -name "*.png" | wc -l)"
echo "Firefox: $(find test-results -name "*firefox*" -name "*.png" | wc -l)"
echo "Mobile: $(find test-results -name "*Mobile*" -name "*.png" | wc -l)"
```

## 视频录制

视频录制配置：

```typescript
use: {
  video: "retain-on-failure"; // 仅失败时保留
  // 或
  video: "on"; // 每个测试都录制
  // 或
  video: "off"; // 不录制
}
```

视频保存在 `test-results/{test-name}-{browser}/video.webm`

## Trace 文件

Trace 文件配置：

```typescript
use: {
  trace: "retain-on-failure"; // 仅失败时保留
  // 或
  trace: "on"; // 每个测试都保存
  // 或
  trace: "off"; // 不保存
}
```

Trace 文件保存在 `test-results/{test-name}-{browser}/trace.zip`

在 Playwright Inspector 中打开：

```bash
npx playwright show-trace test-results/{test-name}-{browser}/trace.zip
```

## 最佳实践

1. **默认配置**：使用 `"only-on-failure"` 仅在失败时保存
2. **调试时**：使用 `"on"` 保存所有截图
3. **CI/CD**：使用 `"on"` 并在失败后查看截图
4. **存储**：定期清理旧截图以节省空间
5. **命名**：使用有意义的测试名称便于查找

## 故障排查

### 截图未保存

1. 检查配置：确保 `screenshot: "on"` 已设置
2. 检查路径：确保 `test-results/` 目录有写权限
3. 查看日志：检查测试运行日志中的错误

### 截图路径错误

使用绝对路径或相对于项目根目录的路径：

```typescript
// 推荐（相对于项目根目录）
await page.screenshot({ path: "screenshots/test.png" });

// 或使用绝对路径
await page.screenshot({ path: "/full/path/to/screenshots/test.png" });
```

## 相关文档

- [Playwright 截图文档](https://playwright.dev/docs/screenshots)
- [Playwright 视频文档](https://playwright.dev/docs/videos)
- [Playwright Trace 文档](https://playwright.dev/docs/trace-viewer)
