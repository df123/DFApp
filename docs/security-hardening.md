# 安全加固说明

## 变更范围

本文记录 2026-08-25 安全评审后的后端修复。`DFApp.Downloader.App` 由项目负责人确认仅运行于可信内网，本次不调整其绑定、CORS 与认证策略。

## JWT 密钥配置

- `appsettings.json` 不再保存 `Jwt:SecretKey`，避免 `${JWT_SECRET_KEY}` 占位符被当作真实密钥使用。
- 部署环境必须使用 ASP.NET Core 标准环境变量名：

```bash
export Jwt__SecretKey='至少32字节的随机密钥'
```

- `Program.cs` 和 JWT 生成逻辑均要求密钥 UTF-8 字节长度不少于 32 字节，不满足时启动或签发失败。
- 生成建议：`openssl rand -base64 48`。

## 已泄露凭据处置

仓库历史中曾出现彩票代理令牌、OAuth 客户端密钥、测试账号密码以及 `admin / 123456` 登录快照。当前分支已移除这些文件和明文值，但仅删除当前分支内容不能消除 Git 历史泄露，必须完成外部轮换：

1. 执行 `sql/26-invalidate-lottery-proxy-token.sql`，先使旧彩票代理令牌失效。
2. 生成新的 `LotteryProxyToken`，同步更新 Web 数据库配置与 LotteryProxy 的 `ProxySettings__ProxyToken` 环境变量。
3. 在 OpenIddict 中轮换 OAuth 客户端密钥，并更新部署环境与 Playwright 环境变量。
4. 修改 `admin`、测试用户及其他使用过默认或弱密码的账号。
5. 如必须彻底清除 Git 历史，备份后使用 `git filter-repo` 或 BFG，重写后通知所有协作者重新克隆并处理强制推送。

敏感文件处置：

- 删除被跟踪的 `client/.env.test` 与 `.playwright-cli/*.yml` 快照。
- `client/.env.test.example` 仅保留空值模板。
- `.gitignore` 忽略前端环境文件。
- `DFApp.LotteryProxy/.dockerignore` 排除 `.env` 与 `.env.*`，避免构建上下文带入密钥。
- Playwright 认证使用 `PLAYWRIGHT_TEST_USERNAME`、`PLAYWRIGHT_TEST_PASSWORD`、`PLAYWRIGHT_OAUTH_CLIENT_SECRET` 环境变量。

## Telegram 管理权限

`/api/app/tg-login/status`、`/config`、`/chats` 均要求 `DFApp.TelegramManagement` 权限。执行 `sql/25-add-telegram-management-permission.sql` 会将该权限授予 `admin` 角色。

## 用户激活与密码重置

- 已停用用户即使密码正确也不能登录、发送、验证或使用密码重置令牌。
- 登录失败统一返回“用户名或密码错误”，避免账户状态被枚举。
- 密码重置令牌在验证阶段不再删除，只有重置成功后才清除。
- 系统尚未接入邮件或短信发送服务，前端已将密码重置页面改为“联系管理员”占位页，并隐藏登录页入口。

## 路径安全

### 文件上传

- multipart 文件名必须是纯 basename，不能包含 `/`、`\`、绝对路径或目录穿越段。
- 保存路径由服务端解析，写入前校验最终路径必须位于配置的上传根目录内。
- 只创建上传根目录，不再根据用户输入创建任意目录。

### 日志查看

- `fileName` 必须是位于 `Logs` 目录内的 `.txt` basename。
- 后端解析绝对路径后校验相对路径不能越出日志根目录。
- 内容读取与下载共用同一套校验逻辑。

## 依赖与质量门禁

- Web 与 LotteryProxy 更新 `AngleSharp`、`Swashbuckle.AspNetCore`、`Microsoft.OpenApi`、`SQLitePCLRaw.lib.e_sqlite3`，当前 `dotnet list package --vulnerable --include-transitive` 无漏洞结果。
- CI 在 push 与 pull_request 上执行后端 restore/build/test、漏洞依赖检查、前端 typecheck/lint/build。
- 只有后端与前端质量检查全部通过后，`master` push 才会发布预览 Release。

## 2026-08-30 渗透测试修复

针对外部渗透测试报告（Strix，2026-08-30）的代码层修复。全部修复已有单元测试覆盖（`test/DFApp.Web.Tests`，85 项全通过）。

### Downloader 路径穿越（高危）

- 下载通知中的文件名不再被信任：`DownloadManager.OnNotificationReceived` 将其强制扁平化为纯文件名（`Path.GetFileName`），携带路径分隔符/目录穿越段的输入直接拒绝入队。
- 解析出的最终路径必须位于下载根目录内（`GetFullPath` 前缀校验），越界即拒绝。
- 入库的 `FileName`/`LocalPath` 均使用净化后的值，删除、重试等后续链路自动受保护。

### 排序参数 SQL 注入（中危）

- 新增 `Infrastructure/SortingSanitizer`：排序串必须命中实体简单类型属性白名单，方向仅允许 `asc`/`desc`，非法输入回退默认排序。
- 接入点：`RssSourceAppService`、`RssMirrorItemAppService`、`RssWordSegmentAppService`（列表与统计两处）、`LotteryService.GetListGrouped`（内存排序同样净化，消除任意动态表达式求值面）。

### SSRF 出站防护（高危）

- 新增 `Infrastructure/SsrfGuard`：仅允许 http/https、拒绝 userinfo、拒绝内网/回环/链路本地/唯一本地/CGNAT 地址（含云元数据 169.254.169.254）；校验发生在建立连接时（对实际连接 IP 判定，防 DNS 重绑定）；重定向关闭自动跟随，由 `SafeGetAsync` 逐跳校验。
- 接入点：`RssFetchService.FetchRssFeed`、`RssMirrorFetchJob.FetchRssSource`、`Aria2Service` 两处 torrent 下载。
- 已知取舍：RSS 抓取的代理功能保留（属运营者配置，受权限保护）。走代理时目标解析发生在代理侧，本端对目标的校验为尽力而为（直连场景为强制）。
  如需彻底消除该面，需下线代理功能——待业务确认。

### 认证与会话（中危）

- `/hubs/aria2` 增加 `[Authorize]`，前端 `aria2/manage.vue` 连接时通过 `accessTokenFactory` 携带 JWT。
- 登录失败锁定改为「用户名+来源IP」键（阈值 5 次/15 分钟），另按用户名单设跨来源上限（50 次）防多源撞库；
  计数器在窗口创建时固定过期时间，后续失败只递增不续期，攻击者无法通过持续失败延长受害者锁定窗口。
- 密码重置发送接口响应不再区分账号是否存在（防枚举），限速从"按提交串"改为"按来源 IP"（5 次/小时）。

### 对象级授权（中危，定向修复）

- `CrudServiceBase` 新增可选所有权强制（`RequireOwnerCheck`，默认关闭）：开启后单条读取/更新/删除与列表/分页查询均按 `CreatorId` 过滤；
  持有用户管理权限的账号视为管理员可访问全部记录（含历史无创建者数据）。
- 文件上传模块已开启（`FileUploadInfoService.RequireOwnerCheck = true`）：非管理员只能读取/下载/删除自己上传的文件；
  `DeleteAsync` 先校验所有权再删物理文件。
- 决策记录：记账、彩票等业务模块维持"登录即可共享"的现状（单运营者部署、家庭成员共享场景），如后续引入多租户需逐模块开启。

### 上传与响应头（中危/低危）

- 上传接口新增危险扩展名黑名单（可执行文件、脚本、Web Shell 载体）；下载端点本就以 `FileDownloadName` 强制 attachment。
- Web 端新增安全响应头：`X-Content-Type-Options: nosniff`、`X-Frame-Options: SAMEORIGIN`、`Referrer-Policy: no-referrer`，非开发环境启用 HSTS。

### LotteryProxy 令牌缺省必填（中危）

- `ProxyToken` 为空时不再静默跳过校验：未显式配置 `ProxySettings__AllowAnonymous=true` 时，非 health 请求一律返回 503（fail-closed）。
- 部署机必须通过 `.env` 注入令牌；本地调试需显式豁免。

### 待运维操作（上线时执行）

1. 执行 `sql/25`、`sql/26`（Telegram 管理权限授权、旧代理令牌失效）。
2. 按上文"已泄露凭据处置"完成代理令牌轮换与 `LotteryProxy` 环境变量同步。
3. Downloader 需重新 `publish` 后由负责人重启（勿直接改动运行中的 9550 实例）。
