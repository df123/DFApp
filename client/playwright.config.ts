import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [["html"], ["list"]],
  use: {
    baseURL: "http://localhost:8848",
    trace: "on-first-retry",
    ignoreHTTPSErrors: true,
    screenshot: "on",
    video: "on",
    viewport: { width: 1280, height: 720 },
    locale: "zh-CN",
    timezoneId: "Asia/Shanghai"
  },
  projects: [
    {
      name: "setup",
      testMatch: /.*\.setup\.ts/,
      use: {
        baseURL: "http://localhost:8848"
      }
    },
    {
      name: "chromium",
      use: {
        ...devices["Desktop Chrome"],
        storageState: "playwright/.auth/user.json"
      },
      dependencies: ["setup"]
    },
    {
      name: "firefox",
      use: {
        ...devices["Desktop Firefox"],
        storageState: "playwright/.auth/user.json"
      },
      dependencies: ["setup"]
    },
    {
      name: "Mobile Chrome",
      use: {
        ...devices["Pixel 5"],
        storageState: "playwright/.auth/user.json"
      },
      dependencies: ["setup"]
    }
  ],
  webServer: {
    command: "NODE_ENV=test pnpm dev",
    url: "http://localhost:8848",
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
    env: {
      NODE_ENV: "test"
    }
  }
});
