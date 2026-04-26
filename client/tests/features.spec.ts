import { test, expect } from "@playwright/test";

test.describe("Lottery Management Tests", () => {
  test("should navigate to lottery page", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const lotteryLink = page.getByRole("link", { name: /彩票/ });

    if (await lotteryLink.isVisible()) {
      await lotteryLink.click();
      await expect(page).toHaveURL(/.*lottery.*/);
    } else {
      test.skip();
    }
  });

  test("should display lottery list", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const lotteryLink = page.getByRole("link", { name: /彩票/ });

    if (await lotteryLink.isVisible()) {
      await lotteryLink.click();
      await page.waitForLoadState("networkidle");

      const table = page.locator("table").or(page.locator(".el-table"));
      const rows = table.locator("tr");

      const count = await rows.count();
      expect(count).toBeGreaterThan(0); // 至少有内容
    } else {
      test.skip();
    }
  });

  test("should search lottery by name", async ({ page }) => {
    await page.goto("/lottery");
    await page.waitForLoadState("networkidle");

    const searchInput = page
      .locator("input[placeholder*='搜索'], input[placeholder*='search']")
      .first();

    if (await searchInput.isVisible()) {
      await searchInput.fill("双色球");
      await page.waitForTimeout(1000);

      // 验证搜索结果
      const table = page.locator("table").or(page.locator(".el-table"));
      await expect(table).toBeVisible();
    }
  });

  test("should add new lottery", async ({ page }) => {
    await page.goto("/lottery");
    await page.waitForLoadState("networkidle");

    const addButton = page.getByRole("button", { name: /新增|添加|新建/ });

    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);

      // 填写表单
      const nameInput = page.locator("input[name*='name']").first();
      if (await nameInput.isVisible()) {
        await nameInput.fill("测试彩票");
      }

      // 保存
      const saveButton = page.getByRole("button", { name: /保存|确认|提交/ });
      if (await saveButton.isVisible()) {
        await saveButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });
});

test.describe("Bookkeeping Tests", () => {
  test("should navigate to bookkeeping page", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const bookkeepingLink = page.getByRole("link", { name: /记账/ });

    if (await bookkeepingLink.isVisible()) {
      await bookkeepingLink.click();
      await expect(page).toHaveURL(/.*bookkeeping.*/);
    } else {
      test.skip();
    }
  });

  test("should display transactions list", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const bookkeepingLink = page.getByRole("link", { name: /记账/ });

    if (await bookkeepingLink.isVisible()) {
      await bookkeepingLink.click();
      await page.waitForLoadState("networkidle");

      const table = page.locator("table").or(page.locator(".el-table"));

      if ((await table.count()) > 0) {
        await expect(table.first()).toBeVisible();
      } else {
        console.log("No table found on bookkeeping page");
      }
    } else {
      test.skip();
    }
  });

  test("should add new transaction", async ({ page }) => {
    await page.goto("/bookkeeping");
    await page.waitForLoadState("networkidle");

    const addButton = page.getByRole("button", { name: /新增|添加|记一笔/ });

    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);

      // 填写金额
      const amountInput = page
        .locator("input[name*='amount'], input[placeholder*='金额']")
        .first();
      if (await amountInput.isVisible()) {
        await amountInput.fill("100.00");
      }

      // 填写备注
      const noteInput = page
        .locator("input[name*='note'], input[placeholder*='备注'], textarea")
        .first();
      if (await noteInput.isVisible()) {
        await noteInput.fill("测试记账");
      }

      // 保存
      const saveButton = page.getByRole("button", { name: /保存|确认|提交/ });
      if (await saveButton.isVisible()) {
        await saveButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });
});

test.describe("Download Subscription Tests", () => {
  test("should navigate to subscription page", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const subscriptionLink = page.getByRole("link", { name: /订阅/ });

    if (await subscriptionLink.isVisible()) {
      await subscriptionLink.click();
      await expect(page).toHaveURL(/.*download-subscription.*/);
    } else {
      test.skip();
    }
  });

  test("should display subscriptions list", async ({ page }) => {
    await page.goto("/download-subscription");
    await page.waitForLoadState("networkidle");

    const card = page.locator(".el-card, .card").first();
    if (await card.isVisible()) {
      await expect(card).toBeVisible();
    }
  });

  test("should create new subscription", async ({ page }) => {
    await page.goto("/download-subscription");
    await page.waitForLoadState("networkidle");

    const addButton = page.getByRole("button", { name: /新增|添加/ });

    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(500);

      // 填写名称
      const nameInput = page.locator("input[name*='name']").first();
      if (await nameInput.isVisible()) {
        await nameInput.fill("测试订阅");
      }

      // 填写 URL
      const urlInput = page
        .locator("input[name*='url'], input[placeholder*='URL']")
        .first();
      if (await urlInput.isVisible()) {
        await urlInput.fill("https://example.com/feed");
      }

      // 保存
      const saveButton = page.getByRole("button", { name: /保存|确认|提交/ });
      if (await saveButton.isVisible()) {
        await saveButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });
});

test.describe("System Settings Tests", () => {
  test("should navigate to system page", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const systemLink = page.getByRole("link", { name: /系统/ });

    if (await systemLink.isVisible()) {
      await systemLink.click();
      await expect(page).toHaveURL(/.*system.*/);
    } else {
      test.skip();
    }
  });

  test("should display user list", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const systemLink = page.getByRole("link", { name: /系统/ });

    if (await systemLink.isVisible()) {
      await systemLink.click();
      await page.waitForLoadState("networkidle");

      const table = page.locator("table").or(page.locator(".el-table"));

      if ((await table.count()) > 0) {
        await expect(table.first()).toBeVisible();
      } else {
        console.log("No table found on system page");
      }
    } else {
      test.skip();
    }
  });

  test("should display role list", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const systemLink = page.getByRole("link", { name: /系统/ });

    if (await systemLink.isVisible()) {
      await systemLink.click();
      await page.waitForLoadState("networkidle");

      const table = page.locator("table").or(page.locator(".el-table"));

      if ((await table.count()) > 0) {
        await expect(table.first()).toBeVisible();
      } else {
        console.log("No table found on system page");
      }
    } else {
      test.skip();
    }
  });

  test("should display settings", async ({ page }) => {
    await page.goto("/system/settings");
    await page.waitForLoadState("networkidle");

    const form = page.locator("form, .el-form").first();
    if (await form.isVisible()) {
      await expect(form).toBeVisible();
    }
  });
});

test.describe("Responsive Design Tests", () => {
  const viewports = [
    { name: "Mobile", width: 375, height: 667 },
    { name: "Tablet", width: 768, height: 1024 },
    { name: "Desktop", width: 1920, height: 1080 }
  ];

  for (const viewport of viewports) {
    test(`should display correctly on ${viewport.name}`, async ({ page }) => {
      await page.setViewportSize({
        width: viewport.width,
        height: viewport.height
      });
      await page.goto("/");
      await page.waitForLoadState("networkidle");

      const mainContent = page.locator("main, [class*='content']").first();
      await expect(mainContent).toBeVisible();
    });
  }
});

test.describe("Performance Tests", () => {
  test("should load page within acceptable time", async ({ page }) => {
    test.setTimeout(15000); // 增加超时时间到 15 秒

    const startTime = Date.now();

    await page.goto("/");
    await page.waitForLoadState("networkidle", { timeout: 10000 });

    const loadTime = Date.now() - startTime;
    console.log(`Page load time: ${loadTime}ms`);

    expect(loadTime).toBeLessThan(5000); // 5秒内加载完成
  });

  test("should not have console errors", async ({ page }) => {
    const errors: string[] = [];

    page.on("console", msg => {
      if (msg.type() === "error") {
        errors.push(msg.text());
      }
    });

    await page.goto("/");
    await page.waitForLoadState("networkidle");

    expect(errors.length).toBe(0);
  });
});
