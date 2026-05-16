import { test, expect } from "@playwright/test";

test.describe("认证测试", () => {
  test("应该访问受保护的页面", async ({ page }) => {
    await page.goto("/");

    await expect(page).toHaveTitle(/DFApp|首页/);
  });

  test("应该显示用户信息", async ({ page }) => {
    await page.goto("/");

    const userInfo = await page.evaluate(() => {
      const userKey = "user-info";
      const item = localStorage.getItem(userKey);
      return item ? JSON.parse(item) : null;
    });

    expect(userInfo).not.toBeNull();
    expect(userInfo.username).toBe("test");
  });

  test("应该导航到彩票页面", async ({ page }) => {
    await page.goto("/");
    await page.waitForLoadState("networkidle");

    const lotteryLink = page.getByRole("link", { name: /彩票/ });
    if (await lotteryLink.isVisible()) {
      await lotteryLink.click();
      await expect(page).toHaveURL(/.*lottery.*/);
    }
  });
});
