import { readFileSync } from "node:fs";
import { test, expect, APIRequestContext } from "@playwright/test";

/**
 * 第三轮安全修复（dfapp_4c5c）验证用例。
 * API 断言（提权/改密/IDOR/SaveDrive/aria2）跑在 api 工程；
 * UI 流程（登录、修改密码对话框）在 desktop-4k 与 iphone-17 两个设备档位各跑一遍。
 */

const DATA = JSON.parse(readFileSync("/tmp/pw-test-data.json", "utf-8")) as {
  testUserId: string;
  adminUserId: string;
  adminRoleId: string;
  vehicleId: string;
  saveDriveConfigId: number;
  testUser: string;
  testPw: string;
};

const ADMIN_USER = "admin";
const ADMIN_PW = "qwe123";
/** UI 阶段把测试用户密码改到 v3，admin 自改密码后再改回 */
const TEST_PW_V2 = "PwTest#R3@v2";
const TEST_PW_V3 = "PwTest#R3@v3";
const ADMIN_TMP_PW = "Admin#Tmp26";

const API = "http://localhost:44369";

interface ApiResp {
  status: number;
  body: any;
}

async function api(
  ctx: APIRequestContext,
  method: string,
  path: string,
  token?: string,
  body?: any
): Promise<ApiResp> {
  const r = await ctx.fetch(API + path, {
    method,
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    data: body
  });
  let parsed: any = null;
  try {
    parsed = await r.json();
  } catch {
    parsed = {};
  }
  return { status: r.status(), body: parsed };
}

async function login(ctx: APIRequestContext, username: string, password: string) {
  const r = await api(ctx, "POST", "/api/app/account/login", undefined, {
    username,
    password
  });
  expect(r.body?.success, `登录失败: ${JSON.stringify(r.body)}`).toBe(true);
  return r.body.data.accessToken as string;
}

/** 依序尝试多个候选密码登录（用例间存在密码流转时增强健壮性） */
async function loginAny(
  ctx: APIRequestContext,
  username: string,
  ...passwords: string[]
) {
  let last: any = null;
  for (const pw of passwords) {
    const r = await api(ctx, "POST", "/api/app/account/login", undefined, {
      username,
      password: pw
    });
    if (r.body?.success) {
      return r.body.data.accessToken as string;
    }
    last = r.body;
  }
  throw new Error(`所有候选密码均登录失败: ${JSON.stringify(last)}`);
}

/** 业务层拒绝：success=false 或 HTTP >= 400 */
function expectDenied(r: ApiResp, keyword?: string) {
  const denied = r.status >= 400 || r.body?.success === false;
  expect(
    denied,
    `预期被拒绝，实际 status=${r.status} body=${JSON.stringify(r.body)}`
  ).toBe(true);
  if (keyword) {
    expect(String(r.body?.message ?? "")).toContain(keyword);
  }
}

// ---------------------------------------------------------------------------
// API 断言（仅 api 工程）
// ---------------------------------------------------------------------------
test.describe("提权封堵（vuln-0026）", () => {
  test.beforeEach(({}, ti) => ti.skip(ti.project.name !== "api", "仅 api 工程"));

  test("自授 UserManagement 权限被拒绝", async ({ request }) => {
    const tok = await login(request, DATA.testUser, DATA.testPw);
    const r = await api(request, "POST", "/api/app/permission-grant-management", tok, {
      providerType: "User",
      providerKey: DATA.testUserId,
      permissionNames: ["DFApp.UserManagement"]
    });
    expectDenied(r, "不能修改自己的权限");
  });

  test("自指派 admin 角色被拒绝", async ({ request }) => {
    const tok = await login(request, DATA.testUser, DATA.testPw);
    const r = await api(request, "POST", "/api/app/user-role-management", tok, {
      userId: DATA.testUserId,
      roleIds: [DATA.adminRoleId]
    });
    expectDenied(r, "不能修改自己的角色");
  });

  test("非管理员向他人授予管理员级权限被拒绝", async ({ request }) => {
    const tok = await login(request, DATA.testUser, DATA.testPw);
    const r = await api(request, "POST", "/api/app/permission-grant-management", tok, {
      providerType: "User",
      providerKey: DATA.adminUserId,
      permissionNames: ["DFApp.UserManagement"]
    });
    expectDenied(r, "管理员级权限");
  });

  test("非管理员给他人指派静态角色被拒绝", async ({ request }) => {
    const tok = await login(request, DATA.testUser, DATA.testPw);
    const r = await api(request, "POST", "/api/app/user-role-management", tok, {
      userId: DATA.adminUserId,
      roleIds: [DATA.adminRoleId]
    });
    expectDenied(r, "静态角色");
  });
});

test.describe("change-password 加固（vuln-0027）", () => {
  test.beforeEach(({}, ti) => ti.skip(ti.project.name !== "api", "仅 api 工程"));

  test("本人改密缺少原密码被拒绝", async ({ request }) => {
    const tok = await login(request, DATA.testUser, DATA.testPw);
    const r = await api(request, "POST", "/api/app/user-management/change-password", tok, {
      userId: DATA.testUserId,
      newPassword: "Whatever#123"
    });
    expectDenied(r, "原密码不正确");
  });

  test("本人改密原密码错误被拒绝", async ({ request }) => {
    const tok = await login(request, DATA.testUser, DATA.testPw);
    const r = await api(request, "POST", "/api/app/user-management/change-password", tok, {
      userId: DATA.testUserId,
      currentPassword: "WrongOriginal#0",
      newPassword: "Whatever#123"
    });
    expectDenied(r, "原密码不正确");
  });

  test("非管理员重置管理员密码被拒绝", async ({ request }) => {
    const tok = await login(request, DATA.testUser, DATA.testPw);
    const r = await api(request, "POST", "/api/app/user-management/change-password", tok, {
      userId: DATA.adminUserId,
      newPassword: "Pwned#Admin26"
    });
    expectDenied(r, "管理员账户");
  });

  test("管理员重置普通用户密码无需原密码", async ({ request }) => {
    const admin = await login(request, ADMIN_USER, ADMIN_PW);
    const r = await api(request, "POST", "/api/app/user-management/change-password", admin, {
      userId: DATA.testUserId,
      newPassword: TEST_PW_V2
    });
    expect(r.body?.success, JSON.stringify(r.body)).toBe(true);
    // 新密码可登录、旧密码失效
    await login(request, DATA.testUser, TEST_PW_V2);
    const old = await api(request, "POST", "/api/app/account/login", undefined, {
      username: DATA.testUser,
      password: DATA.testPw
    });
    expectDenied(old);
  });
});

test.describe("对象级归属校验（IDOR）", () => {
  test.beforeEach(({}, ti) => ti.skip(ti.project.name !== "api", "仅 api 工程"));

  let adminTok: string;
  let userTok: string;
  let categoryId: number;

  test.beforeAll(async ({ request }, testInfo) => {
    if (testInfo.project.name !== "api") {
      return;
    }
    adminTok = await login(request, ADMIN_USER, ADMIN_PW);
    userTok = await loginAny(request, DATA.testUser, TEST_PW_V2, DATA.testPw);
    const list = await api(request, "GET", "/api/app/bookkeeping-category", adminTok);
    const items = Array.isArray(list.body?.data) ? list.body.data : [];
    categoryId = items.find((c: any) => c.category === "PW测试分类-勿动")?.id;
    expect(categoryId, "未找到 admin 创建的测试分类").toBeTruthy();
  });

  test("他人分类不可读/改/删", async ({ request }) => {
    const get = await api(request, "GET", `/api/app/bookkeeping-category/${categoryId}`, userTok);
    expectDenied(get, "无权");

    const put = await api(
      request,
      "PUT",
      `/api/app/bookkeeping-category/${categoryId}`,
      userTok,
      { category: "PW劫持分类", icon: "x", rank: 0 }
    );
    expectDenied(put, "无权");

    const del = await api(request, "DELETE", `/api/app/bookkeeping-category/${categoryId}`, userTok);
    expectDenied(del, "无权");
  });

  test("分类列表不泄露他人数据", async ({ request }) => {
    const r = await api(request, "GET", "/api/app/bookkeeping-category", userTok);
    const items = Array.isArray(r.body?.data) ? r.body.data : [];
    expect(
      items.some((c: any) => c.category === "PW测试分类-勿动"),
      `测试用户不应看到 admin 的分类，实际: ${JSON.stringify(items)}`
    ).toBe(false);
  });

  test("他人车辆不可读/删", async ({ request }) => {
    const get = await api(request, "GET", `/api/app/electric-vehicle/${DATA.vehicleId}`, userTok);
    expectDenied(get, "无权");

    const del = await api(request, "DELETE", `/api/app/electric-vehicle/${DATA.vehicleId}`, userTok);
    expectDenied(del, "无权");
  });

  test("车辆列表不泄露他人数据", async ({ request }) => {
    const r = await api(request, "GET", "/api/app/electric-vehicle", userTok);
    const items = Array.isArray(r.body?.data) ? r.body.data : [];
    expect(items.some((v: any) => v.name === "PW测试车辆")).toBe(false);
  });
});

test.describe("SaveDrive 敏感配置防护（vuln-0023）", () => {
  test.beforeEach(({}, ti) => ti.skip(ti.project.name !== "api", "仅 api 工程"));

  test("非特权用户改写 SaveDrive 被拒绝", async ({ request }) => {
    const tok = await loginAny(request, DATA.testUser, TEST_PW_V2, DATA.testPw);
    const r = await api(
      request,
      "PUT",
      `/api/app/configuration-info/${DATA.saveDriveConfigId}`,
      tok,
      {
        moduleName: "DFApp.Web.Media",
        configurationName: "SaveDrive",
        configurationValue: "/etc",
        remark: "劫持测试"
      }
    );
    expectDenied(r, "安全敏感配置");
  });
});

test.describe("aria2 SSRF 与路径写防护（vuln-0015）", () => {
  test.beforeEach(({}, ti) => ti.skip(ti.project.name !== "api", "仅 api 工程"));

  test("回环 URL 被 SSRF 防护拦截", async ({ request }) => {
    const tok = await login(request, ADMIN_USER, ADMIN_PW);
    const r = await api(request, "POST", "/api/app/aria2/add-download", tok, {
      urls: ["http://127.0.0.1:6800/jsonrpc"]
    });
    expectDenied(r, "内网");
  });

  test("绝对路径 savePath 被拒绝", async ({ request }) => {
    const tok = await login(request, ADMIN_USER, ADMIN_PW);
    const r = await api(request, "POST", "/api/app/aria2/add-download", tok, {
      urls: ["http://example.com/file.bin"],
      savePath: "/tmp/evil-path"
    });
    expectDenied(r, "相对子目录");
  });

  test("aria2-manage 端点同样受防护", async ({ request }) => {
    const tok = await login(request, ADMIN_USER, ADMIN_PW);
    const r = await api(request, "POST", "/api/app/aria2-manage/add-uri", tok, {
      urls: ["http://169.254.169.254/latest/meta-data/"]
    });
    expectDenied(r, "内网");
  });
});

// ---------------------------------------------------------------------------
// UI 流程（desktop-4k / iphone-17）
// ---------------------------------------------------------------------------
test.describe("UI 登录与修改密码对话框", () => {
  test.beforeEach(({}, ti) =>
    ti.skip(ti.project.name === "api", "UI 仅设备档位工程")
  );

  test("登录页渲染并成功登录 admin", async ({ page }) => {
    await page.goto("/login");
    await expect(page.getByPlaceholder("请输入用户名")).toBeVisible();
    await page.getByPlaceholder("请输入用户名").fill(ADMIN_USER);
    await page.getByPlaceholder("请输入密码").fill(ADMIN_PW);
    await page.locator(".login-button").click();
    await expect(page.getByText("欢迎使用应用程序")).toBeVisible({
      timeout: 20000
    });
    await page.screenshot({
      path: `playwright-security-shots/${test.info().project.name}-welcome.png`,
      fullPage: true
    });
  });

  test("修改他人密码：无原密码字段，直接设置成功", async ({ page }) => {
    await page.goto("/login");
    await page.getByPlaceholder("请输入用户名").fill(ADMIN_USER);
    await page.getByPlaceholder("请输入密码").fill(ADMIN_PW);
    await page.locator(".login-button").click();
    await expect(page.getByText("欢迎使用应用程序")).toBeVisible({ timeout: 20000 });

    await page.goto("/system/user-management");
    await expect(page.getByRole("button", { name: "修改密码" }).first()).toBeVisible({
      timeout: 20000
    });

    const row = page.locator("tr", { hasText: DATA.testUser }).first();
    await row.getByRole("button", { name: "修改密码" }).click();

    const dialog = page.locator(".el-dialog").filter({ hasText: "修改密码" });
    await expect(dialog).toBeVisible();
    // 目标不是当前登录用户 → 不显示原密码字段
    await expect(dialog.getByText("原密码")).toHaveCount(0);

    await dialog.getByPlaceholder("请输入新密码").fill(TEST_PW_V3);
    await dialog.getByPlaceholder("请再次输入新密码").fill(TEST_PW_V3);
    await dialog.getByRole("button", { name: "确定" }).click();
    await expect(page.getByText("密码修改成功")).toBeVisible({ timeout: 15000 });
    await page.screenshot({
      path: `playwright-security-shots/${test.info().project.name}-reset-others.png`
    });
  });

  test("修改本人密码：原密码必填，错误被拒，正确后成功并改回", async ({ page }) => {
    await page.goto("/login");
    await page.getByPlaceholder("请输入用户名").fill(ADMIN_USER);
    await page.getByPlaceholder("请输入密码").fill(ADMIN_PW);
    await page.locator(".login-button").click();
    await expect(page.getByText("欢迎使用应用程序")).toBeVisible({ timeout: 20000 });

    await page.goto("/system/user-management");
    const row = page.locator("tr", { hasText: ADMIN_USER }).first();
    await row.getByRole("button", { name: "修改密码" }).click();

    const dialog = page.locator(".el-dialog").filter({ hasText: "修改密码" });
    await expect(dialog).toBeVisible();
    // 目标是当前登录用户 → 显示原密码字段
    await expect(dialog.getByText("原密码", { exact: true })).toBeVisible();
    await page.screenshot({
      path: `playwright-security-shots/${test.info().project.name}-self-dialog.png`
    });

    // 原密码错误 → 服务端拒绝
    await dialog.getByPlaceholder("请输入当前登录用户的原密码").fill("WrongOriginal#0");
    await dialog.getByPlaceholder("请输入新密码").fill(ADMIN_TMP_PW);
    await dialog.getByPlaceholder("请再次输入新密码").fill(ADMIN_TMP_PW);
    await dialog.getByRole("button", { name: "确定" }).click();
    await expect(page.getByText("修改密码失败")).toBeVisible({ timeout: 15000 });

    // 原密码正确 → 修改成功
    await dialog.getByPlaceholder("请输入当前登录用户的原密码").fill(ADMIN_PW);
    await dialog.getByPlaceholder("请输入新密码").fill(ADMIN_TMP_PW);
    await dialog.getByPlaceholder("请再次输入新密码").fill(ADMIN_TMP_PW);
    await dialog.getByRole("button", { name: "确定" }).click();
    await expect(page.getByText("密码修改成功")).toBeVisible({ timeout: 15000 });

    // 改回原密码，保持环境一致
    await row.getByRole("button", { name: "修改密码" }).click();
    const dialog2 = page.locator(".el-dialog").filter({ hasText: "修改密码" });
    await dialog2.getByPlaceholder("请输入当前登录用户的原密码").fill(ADMIN_TMP_PW);
    await dialog2.getByPlaceholder("请输入新密码").fill(ADMIN_PW);
    await dialog2.getByPlaceholder("请再次输入新密码").fill(ADMIN_PW);
    await dialog2.getByRole("button", { name: "确定" }).click();
    await expect(page.getByText("密码修改成功")).toBeVisible({ timeout: 15000 });
  });

  test("测试用户用新密码可从 UI 登录", async ({ page }) => {
    await page.goto("/login");
    await page.getByPlaceholder("请输入用户名").fill(DATA.testUser);
    await page.getByPlaceholder("请输入密码").fill(TEST_PW_V3);
    await page.locator(".login-button").click();
    await expect(page.getByText("欢迎使用应用程序")).toBeVisible({ timeout: 20000 });
    await page.screenshot({
      path: `playwright-security-shots/${test.info().project.name}-testuser-welcome.png`,
      fullPage: true
    });
  });
});
