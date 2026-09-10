import { defineConfig, devices } from "@playwright/test";

/**
 * 第三轮安全修复验证专用配置：
 * - api         ：纯接口断言（提权封堵/改密加固/IDOR/SaveDrive/aria2）
 * - desktop-4k  ：3840×2160 物理 @200% 缩放（CSS 1920×1080，DPR 2）
 * - iphone-17   ：iPhone 17 标准版（2622×1206 物理 → CSS 874×402，DPR 3）
 */
export default defineConfig({
  testDir: "./tests",
  testMatch: /security-round3\.spec\.ts/,
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [["list"]],
  use: {
    baseURL: "http://localhost:9949",
    ignoreHTTPSErrors: true,
    screenshot: "off",
    video: "off",
    locale: "zh-CN",
    timezoneId: "Asia/Shanghai",
    // 复用本机已缓存的 Chrome for Testing（@playwright/test 1.58 对应的 1208 号构建
    // 在官方源与镜像均无法下载，直接指定可用的本地二进制）
    launchOptions: {
      executablePath:
        "/home/df/.cache/ms-playwright/chromium-1243/chrome-linux64/chrome"
    }
  },
  projects: [
    {
      name: "api",
      use: { ...devices["Desktop Chrome"] }
    },
    {
      name: "desktop-4k",
      use: {
        ...devices["Desktop Chrome"],
        viewport: { width: 1920, height: 1080 },
        deviceScaleFactor: 2
      }
    },
    {
      name: "iphone-17",
      use: {
        ...devices["Desktop Chrome"],
        userAgent:
          "Mozilla/5.0 (iPhone; CPU iPhone OS 19_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/19.0 Mobile/15E148 Safari/604.1",
        viewport: { width: 874, height: 402 },
        deviceScaleFactor: 3,
        isMobile: true,
        hasTouch: true
      }
    }
  ]
});
