# 下载器账号密码登录

## 背景

原认证方案：Web 管理界面仅允许本机（回环同源）免认证访问；非本机访问必须配置 `X-Management-Token` 请求头（浏览器场景基本不可用）。即实际只有 localhost 能正常使用 Web 界面。

2026-10 起改为**账号密码登录**：所有来源（含本机）访问受保护资源均需先登录，适合将 `WebServerBind` 配置为 `0.0.0.0` 后在局域网使用。

## 认证模型

- 单管理员账号：默认 `admin / admin`（PBKDF2-SHA256 哈希，10 万次迭代，存储于 `downloader.db` 的 `Accounts` 表）
- 登录成功后服务端颁发随机会话令牌，通过 `HttpOnly + SameSite=Lax` Cookie（`downloader_session`）携带，有效期 7 天滑动续期
- 会话保存在内存中，进程重启后需重新登录
- 连续 5 次登录失败锁定账号 1 分钟（防暴力破解）
- 修改密码后所有会话立即作废，强制重新登录

### 受保护与放行范围

| 路径 | 认证要求 |
|------|----------|
| `POST /api/auth/login` | 匿名放行 |
| 静态资源（index.html、JS/CSS、SPA fallback） | 匿名放行（登录页需要加载） |
| `/api/*`（其余全部接口） | 会话 Cookie **或** `X-Management-Token` |
| `/media/*`（下载目录映射） | 会话 Cookie **或** `X-Management-Token` |

注意：本机（localhost/127.0.0.1）访问同样需要登录，不再免认证。

### X-Management-Token 兼容

为兼容脚本/程序化调用，`settings.json` 的 `ManagementToken`（或环境变量 `DFAPP_DOWNLOADER_MANAGEMENT_TOKEN`）仍然有效：携带匹配的 `X-Management-Token` 请求头即可访问 API（无需 Cookie）。Web 界面登录无需配置此令牌。

## API

| 接口 | 方法 | 说明 |
|------|------|------|
| `/api/auth/login` | POST | `{username, password}`，成功设置会话 Cookie |
| `/api/auth/logout` | POST | 注销当前会话 |
| `/api/auth/session` | GET | 会话探测，有效 200 / 无效 401（前端路由守卫使用） |
| `/api/auth/change-password` | POST | `{oldPassword, newPassword}`，需已登录；成功后会话全部作废 |

## 前端行为

- 路由守卫：未登录访问任何页面重定向到 `/login?redirect=...`，登录成功后跳回目标页
- API 收到 401 时自动跳转登录页（登录接口本身除外）
- 侧边栏底部提供"退出登录"按钮
- 设置页提供"修改登录密码"表单（旧密码/新密码/确认新密码）

## 迁移（旧版本升级）

- **旧 `downloader.db`**：启动时 SqlSugar CodeFirst 自动创建 `Accounts` 表（无数据丢失），随后种子默认管理员
- **旧 `settings.json`**：字段兼容，缺失的新字段（`WebServerBind`、`ManagementToken`、`SyncMissedOnStartup`）自动使用默认值
- **初始密码**：默认种子 `admin/admin`；可在启动前设置环境变量 `DFAPP_DOWNLOADER_ADMIN_PASSWORD` 指定初始密码（仅首次种子时生效）
- 升级后原 localhost 用户首次访问需登录一次

## 相关代码

| 位置 | 职责 |
|------|------|
| `Core/Entities/Account.cs` | 账号实体 |
| `Core/Security/PasswordHasher.cs` | PBKDF2 口令哈希 |
| `Core/Security/AccountService.cs` | 账号、会话、防暴力破解 |
| `App/Controllers/AuthController.cs` | 登录/登出/会话/改密接口 |
| `App/Program.cs` | 认证中间件（Cookie 会话 + 令牌兼容） |
| `web/src/views/Login.vue` | 登录页 |
| `web/src/router/index.ts` | 路由守卫 |

数据库变更记录见 `sql/31-add-downloader-account-table.sql`（实际建表由 CodeFirst 完成）。
