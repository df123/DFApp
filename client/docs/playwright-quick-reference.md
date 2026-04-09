# Playwright 测试快速参考

## 常用测试模式

### 1. 基本页面访问

```typescript
import { test, expect } from "@playwright/test";

test("访问首页", async ({ page }) => {
  await page.goto("/");
  await expect(page).toHaveTitle(/DFApp/);
});
```

### 2. 表单填写和提交

```typescript
test("填写表单", async ({ page }) => {
  await page.goto("/form-page");

  // 填写文本框
  await page.fill("input[name='username']", "testuser");
  await page.fill("input[name='password']", "password123");

  // 选择下拉框
  await page.selectOption("select[name='role']", "admin");

  // 点击按钮
  await page.click("button[type='submit']");

  // 验证结果
  await expect(page).toHaveURL(/.*success.*/);
});
```

### 3. 元素交互

```typescript
test("元素交互", async ({ page }) => {
  await page.goto("/");

  // 点击链接
  await page.click("a[href='/lottery']");

  // 悬停
  await page.hover(".menu-item");

  // 等待元素出现
  await page.waitForSelector(".loading", { state: "hidden" });

  // 验证元素可见性
  await expect(page.locator(".content")).toBeVisible();
});
```

### 4. 验证 localStorage

```typescript
test("验证 localStorage", async ({ page }) => {
  await page.goto("/");

  const userInfo = await page.evaluate(() => {
    return JSON.parse(localStorage.getItem("user-info") || "{}");
  });

  expect(userInfo.username).toBe("test");
  expect(userInfo.accessToken).toBeDefined();
});
```

### 5. API 响应测试

```typescript
test("API 响应测试", async ({ page }) => {
  await page.goto("/");

  const response = await page.waitForResponse(
    response =>
      response.url().includes("/api/data") && response.status() === 200
  );

  const data = await response.json();
  expect(data.items).toBeDefined();
  expect(data.items.length).toBeGreaterThan(0);
});
```

### 6. 拖放操作

```typescript
test("拖放操作", async ({ page }) => {
  await page.goto("/drag-drop");

  await page.dragAndDrop("#draggable-element", "#drop-zone");

  await expect(page.locator("#drop-zone")).toContainText("已放置");
});
```

### 7. 文件上传

```typescript
test("文件上传", async ({ page }) => {
  await page.goto("/upload");

  const fileInput = page.locator("input[type='file']");
  await fileInput.setInputFiles("/path/to/file.txt");

  await page.click("button[type='submit']");
  await expect(page.locator(".success")).toBeVisible();
});
```

## 选择器技巧

### 文本选择器

```typescript
// 按文本查找
page.getByText("登录");
page.getByRole("button", { name: "提交" });

// 正则表达式
page.getByRole("link", { name: /彩票|订阅/ });
```

### 属性选择器

```typescript
// ID 选择器
page.locator("#submit-button");

// 类选择器
page.locator(".menu-item");

// 属性选择器
page.locator("[data-testid='login-form']");
```

### 层级选择器

```typescript
// 子元素
page.locator(".menu > .item");

// 后代元素
page.locator(".menu .item");

// 组合选择器
page.locator(".menu").getByText("登录");
```

## 断言方法

### 基本断言

```typescript
// 可见性
await expect(element).toBeVisible();
await expect(element).toBeHidden();

// 文本内容
await expect(element).toHaveText("期望文本");
await expect(element).toContainText("部分文本");

// 属性值
await expect(element).toHaveAttribute("href", "/page");
await expect(element).toHaveClass(/active/);

// 数量
await expect(page.locator(".item")).toHaveCount(3);
```

### URL 断言

```typescript
await expect(page).toHaveURL("https://localhost:8848/page");
await expect(page).toHaveURL(/.*page.*/);
await expect(page).toHaveURL(/page\?id=\d+/);
```

### 数量断言

```typescript
await expect(page.locator(".item")).toHaveCount(3);
await expect(page.locator(".item")).toHaveCount(lessThan(10));
await expect(page.locator(".item")).toHaveCount(greaterThanOrEqual(1));
```

## 等待策略

```typescript
// 等待元素可见
await page.waitForSelector(".element", { state: "visible" });

// 等待元素隐藏
await page.waitForSelector(".element", { state: "hidden" });

// 等待导航完成
await page.waitForURL("**/success");

// 等待特定时间（不推荐）
await page.waitForTimeout(1000);

// 等待网络空闲
await page.waitForLoadState("networkidle");
```

## 测试配置

### 单个测试配置

```typescript
test.use({
  headless: false,
  viewport: { width: 1280, height: 720 }
});

test("配置的测试", async ({ page }) => {
  // ...
});
```

### 测试分组配置

```typescript
test.describe("分组测试", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
  });

  test("测试1", async ({ page }) => {
    // ...
  });

  test("测试2", async ({ page }) => {
    // ...
  });
});
```

## 调试技巧

### 截图

```typescript
// 失败时自动截图
test.use({ screenshot: "only-on-failure" });

// 手动截图
await page.screenshot({ path: "screenshot.png" });
```

### 查看页面状态

```typescript
// 打印页面 URL
console.log(await page.url());

// 打印页面标题
console.log(await page.title());

// 打印元素文本
const text = await page.locator(".element").textContent();
console.log(text);
```

### 慢速执行

```typescript
test.use({
  launchOptions: {
    slowMo: 100 // 每个操作后等待 100ms
  }
});
```

## 最佳实践

1. **使用描述性测试名称**

   ```typescript
   test("should navigate to lottery page after clicking menu", async ({
     page
   }) => {
     // ...
   });
   ```

2. **使用 page 对象模型**

   ```typescript
   class LoginPage {
     constructor(private page: Page) {}

     async login(username: string, password: string) {
       await this.page.fill("input[name='username']", username);
       await this.page.fill("input[name='password']", password);
       await this.page.click("button[type='submit']");
     }
   }
   ```

3. **避免硬编码等待时间**

   ```typescript
   // ❌ 不好
   await page.waitForTimeout(2000);

   // ✅ 好
   await page.waitForSelector(".element");
   ```

4. **使用语义化选择器**

   ```typescript
   // ❌ 不好
   page.locator("div div div:nth-child(3)");

   // ✅ 好
   page.getByRole("button", { name: "提交" });
   ```

5. **独立测试**

   ```typescript
   // 每个测试应该可以独立运行
   // 不依赖其他测试的执行顺序
   test("独立测试1", async ({ page }) => {
     await page.goto("/");
   });

   test("独立测试2", async ({ page }) => {
     await page.goto("/");
   });
   ```

## 参考资源

- [Playwright 官方文档](https://playwright.dev/)
- [Playwright API 参考](https://playwright.dev/docs/api/class-playwright)
- [项目测试文档](./docs/playwright-testing.md)
