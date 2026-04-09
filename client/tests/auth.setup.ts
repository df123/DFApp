import { test as setup, expect } from "@playwright/test";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const authFile = path.join(__dirname, "../playwright/.auth/user.json");

setup("authenticate", async ({ page, request, context }) => {
  const response = await request.post("https://localhost:44369/connect/token", {
    form: {
      grant_type: "password",
      client_id: "DFApp_Web",
      client_secret: "X!*l}4Ab[K~um%I*#2",
      username: "test",
      password: "1q2w3E*"
    },
    headers: {
      "Content-Type": "application/x-www-form-urlencoded"
    }
  });

  expect(response.ok()).toBeTruthy();

  const data = await response.json();
  expect(data.access_token).toBeDefined();
  expect(data.expires_in).toBeDefined();

  const expires = Math.floor(Date.now() / 1000) + data.expires_in;

  const userInfo = {
    accessToken: data.access_token,
    refreshToken: data.refresh_token || "",
    expires: expires * 1000,
    roles: ["admin"],
    permissions: ["*:*:*"],
    username: "test",
    nickname: "Test User",
    avatar: ""
  };

  const oidcUser = {
    id_token: data.id_token || "",
    session_state: null,
    access_token: data.access_token,
    refresh_token: data.refresh_token || "",
    token_type: "Bearer",
    scope: "openid profile email roles DFApp offline_access",
    profile: {
      sub: "3637440c-4304-a5f8-6c1b-3a1f347b3949",
      preferred_username: "test",
      email: "test@test.com",
      name: "Test User",
      email_verified: false
    },
    expires_at: expires,
    state: null
  };

  await context.addCookies([
    {
      name: "XSRF-TOKEN",
      value: "test-csrf-token-for-playwright",
      domain: "localhost",
      path: "/",
      sameSite: "Lax"
    }
  ]);

  await context.addInitScript(
    ({ userInfo, oidcUser }) => {
      localStorage.setItem("user-info", JSON.stringify(userInfo));
      localStorage.setItem("multiple-tabs", "true");
      localStorage.setItem(
        `oidc.user:https://localhost:44369/:DFApp_Web`,
        JSON.stringify(oidcUser)
      );
    },
    { userInfo, oidcUser }
  );

  await page.goto("http://localhost:8848/");
  await page.waitForLoadState("networkidle");

  await expect(async () => {
    const isLoggedIn = await page.evaluate(() => {
      const user = window.localStorage.getItem("user-info");
      return user !== null;
    });
    expect(isLoggedIn).toBe(true);
  }).toPass({ timeout: 10000 });

  await page.context().storageState({ path: authFile });
});
