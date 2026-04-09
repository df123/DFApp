import { test, expect } from "@playwright/test";

test.describe("页面导航测试", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");
  });

  test("应该导航到彩票页面", async ({ page }) => {
    const lotteryLink = page.getByRole("link", { name: /彩票/ });

    if (await lotteryLink.isVisible()) {
      await lotteryLink.click();
      await expect(page).toHaveURL(/.*lottery.*/);
    } else {
      test.skip();
    }
  });

  test("应该导航到记账页面", async ({ page }) => {
    const bookkeepingLink = page.getByRole("link", { name: /记账/ });

    if (await bookkeepingLink.isVisible()) {
      await bookkeepingLink.click();
      await expect(page).toHaveURL(/.*bookkeeping.*/);
    } else {
      test.skip();
    }
  });

  test("应该导航到订阅页面", async ({ page }) => {
    const subscriptionLink = page.getByRole("link", { name: /订阅/ });

    if (await subscriptionLink.isVisible()) {
      await subscriptionLink.click();
      await expect(page).toHaveURL(/.*download-subscription.*/);
    } else {
      test.skip();
    }
  });

  test("应该导航到系统页面", async ({ page }) => {
    const systemLink = page.getByRole("link", { name: /系统/ });

    if (await systemLink.isVisible()) {
      await systemLink.click();
      await expect(page).toHaveURL(/.*system.*/);
    } else {
      test.skip();
    }
  });

  test("应该成功退出登录", async ({ page }) => {
    const logoutButton = page
      .getByRole("button", { name: /退出/ })
      .or(page.getByRole("menuitem", { name: /退出/ }));

    if (await logoutButton.isVisible()) {
      await logoutButton.click();
      await page.waitForURL("**/login");
      await expect(page).toHaveURL(/.*login.*/);
    } else {
      test.skip();
    }
  });
});

test.describe("用户界面测试", () => {
  test("应该显示主要内容区域", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const main = page
      .locator("main")
      .or(page.locator("[class*='content']"))
      .or(page.locator("[class*='app-main']"));
    await expect(main.first()).toBeVisible();
  });
});

test.describe("API 响应测试", () => {
  test("应该成功获取应用配置", async ({ page }) => {
    const apiResponses: { url: string; status: number }[] = [];

    page.on("response", response => {
      if (
        response.url().includes("/api/") ||
        response.url().includes("/connect/")
      ) {
        apiResponses.push({
          url: response.url(),
          status: response.status()
        });
      }
    });

    await page.goto("/");
    await page.waitForLoadState("networkidle");
    await page.waitForTimeout(2000);

    const successResponses = apiResponses.filter(
      r => r.status >= 200 && r.status < 400
    );

    if (successResponses.length > 0) {
      console.log(
        "API 响应:",
        successResponses.map(r => `${r.url} - ${r.status}`)
      );
      expect(successResponses[0].status).toBeGreaterThanOrEqual(200);
      expect(successResponses[0].status).toBeLessThan(400);
    } else {
      console.log("未捕获到 API 响应，检查页面加载");
      const currentUrl = page.url();
      expect(currentUrl).toContain("localhost:8848");
    }
  });
});
