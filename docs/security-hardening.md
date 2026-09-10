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

## 2026-08-31 复审修复（第二轮渗透测试）

针对 Strix 复审报告（7/12 已修复确认、5 项部分修复、6 项新发现）的代码修复。

### SSRF 收尾

- `SsrfGuard` 拦截 IPv4 回环**整段** `127.0.0.0/8`（此前仅拦精确的 127.0.0.1，127.0.0.2 等可绕过）。
- RSS 抓取的**代理功能整体下线**：请求/源 DTO、抓取服务、镜像任务与前端表单的 ProxyUrl/ProxyUsername/ProxyPassword 全部移除。
  走代理时目标解析发生在代理侧、服务端校验失效（DNS 重绑定绕过），且实际部署 0 个源使用代理。
  将来确需代理时改为服务端配置（管理员权限）并经同一校验。
- 代理模式特殊分支已从 Guard 移除，所有出站连接一律按内网黑名单严格判定。

### 媒体任意文件读（新发现）

- `MediaInfoService` 客户端提交的 `savePath` 强制净化为纯文件名，落库路径由服务端按 `SaveDrive` 配置拼接。
- 下载端点纵深防御：存储记录中的路径必须解析在媒体根目录内，被污染历史记录也无法越界读取。

### 账号枚举与锁定

- 登录失败计数在**所有失败分支**（用户不存在/停用/密码错误）递增，消除"尝试次数过多"响应构成的账号存在性 oracle。
- `reset-password` 对不存在账号与无效令牌返回同一错误，不再区分响应。
- `GetClientIpAddress` 只信传输层 `RemoteIpAddress`，不再解析可伪造的原始 `X-Forwarded-For`；
  反代场景由 ForwardedHeaders 中间件按 `KnownProxies` 归一化，新增 `ForwardedHeaders:KnownProxies` 配置（支持单 IP 与 CIDR，逗号分隔；默认仅信任回环）。

### 对象级授权（IDOR）逐模块启用

- 记账支出、电动车费用/充电记录、动态 IP、彩票模拟（KL8/SSQ）、媒体、媒体外链全部启用 `RequireOwnerCheck`：
  非管理员只能访问自己创建的记录；管理员（持有用户管理权限）不受限。
- 各服务绕过基类的自定义查询（过滤列表、统计、图表、跨仓储读取）均已补属主过滤。
- 维持共享（未启用）：彩票主数据、记账分类、车辆登记、油价、RSS 源、关键词过滤、系统配置——家庭共享/字典型数据。

### Downloader 管理接口加固（复审 Critical）

- **认证**：非回环来源的请求必须携带匹配的 `X-Management-Token`（固定时间比较）；令牌未配置时非回环一律 401（fail-closed）。
- **默认仅本机**：监听地址默认 `127.0.0.1`，`WebServerBind` 可显式改为 `0.0.0.0`（须配令牌）。
- **凭据脱敏**：`GET /api/settings` 密码/令牌回显 `********`；更新提交掩码值即保留原值，不再明文回传。
- **CORS**：移除 `AllowAnyOrigin`（管理界面同源部署，无需跨域）。
- Web 管理界面适配：请求自动携带令牌，401 时提示输入并保存。

### 依赖升级

- axios：client 1.12.2 → 1.20.0；Downloader web 1.16 → 1.20.0（修复两个 HIGH）。
- js-cookie 3.0.8、qs 6.16.0、echarts 5.6.0 → **6.1.0**（Lines 系列 tooltip XSS）。
- pnpm override 强制 lodash ≥4.18.0（element-plus 传递依赖现解析为 4.18.1）。
- 删除 ABP 时代遗留的 `src/DFApp.Web/package.json` + `yarn.lock`（fast-xml-parser CRITICAL 仅被该锁文件钉住，无任何构建引用）。
- 删除 `sql/12`、`sql/14`（已知弱口令 123456/qwe123# 的 PBKDF2 种子哈希；密码已轮换，脚本废弃）。

### 待运维

- git 历史仍含旧代理令牌（commit d5e4ff9）与弱口令哈希：建议择机 `git filter-repo`/BFG 重写并通知克隆者；在此之前令牌继续视为已泄露（已轮换即可控）。
- 正式服 Apache 侧建议补 HSTS/CSP/版本 banner 隐藏。

## 2026-09-10 第三轮复测修复（dfapp_4c5c，27 项发现全量处置）

针对 Strix 第三轮复测（8 项已修复确认、4 项部分、5 项未修复 + 新发现）的代码修复。
全部修复通过 93 项单元测试（含新增归属校验架构测试）。

### 提权与认证（新发现，P0）

- **权限自提权封堵**（vuln-0026）：`PermissionGrantManagement` 的 Grant/Update/Revoke 拒绝
  非管理员操作自己的权限、授予管理员级权限（UserManagement/RoleManagement/PermissionGrantManagement/
  UserRoleManagement 前缀）、修改静态角色权限；`UserRoleManagement` 的 Assign/Remove 拒绝
  非管理员修改自己的角色、分配或移除静态角色（如 `admin`）。
- **改密接管封堵**（vuln-0027）：`change-password` 修改本人密码必须携带并校验 `currentPassword`；
  对 admin 角色账户的改密/更新/删除仅限持有 admin 角色的调用者（`EnsureTargetUserModifiableAsync`）。
- `IPermissionChecker` 新增 `IsInRoleAsync`，`AppServiceBase` 新增 `IsAdminUserAsync`（按静态 `admin` 角色 claim 判定）。
- 前端"修改密码"对话框：目标为当前登录用户时展示并必填原密码字段。

### aria2 SSRF 与任意文件写（新发现，P0）

- 新增 `Services/Aria2/Aria2OptionGuard`：出站 URL 统一经 `SsrfGuard.EnsureAllowed`（磁力链接走 DHT 豁免）；
  `savePath` 仅允许下载目录内相对子目录（拒绝绝对路径/盘符/上跳）；自定义 RPC 选项白名单仅放行 `select-file`。
- 覆盖 `Aria2Service.AddDownloadAsync`（含 torrent 解析失败回退路径）与 `Aria2ManageService` 的
  AddUri/BatchAddUri/AddTorrent/BatchAddTorrent 全部入口。

### 对象级归属校验全面收口（vuln-0006/0010/0014/0016/0017/0018/0019/0020/0021/0022/0024）

- **`CrudServiceBase.RequireOwnerCheck` 默认翻转为 `true`（fail-closed）**；全局共享数据服务
  （Aria2 任务队列、系统配置、关键词过滤、彩票主数据/开奖结果）显式重写为 `false` 并标注
  `[OwnerCheckDisabledForSharedData]` 特性留痕。
- 补齐缺失归属校验的模块：电动车车辆+里程端点、RSS 源（手写 CRUD 逐方法补校验+列表过滤）、
  记账分类（重名检查按归属范围）、记账支出 Get/Update/图表/月度统计/分类查找、充电记录
  Get/Update/Delete（删除改为先校验再删关联成本行）、彩票模拟 delete-by-term/开奖计算/统计/分页明细
  （KL8 与 SSQ）、媒体 completed 列表/外链标记/物理删除/清理/invalid 批量删除、外链删除（先校验再移除文件）、
  文件上传 SHA1 去重（按归属范围收敛）。
- **架构测试**（`test/DFApp.Web.Tests/Services/OwnerCheckArchitectureTests.cs`）：
  ① 默认值必须为 true；② 未豁免服务重写 GetAsync/UpdateAsync/DeleteAsync 时，方法 IL
  （含 async 状态机与 base 调用代理）必须调用 `EnsureOwnerAsync` 或基类实现——该回归已连犯两轮，现在构建即失败。
  方法级豁免需标注 `[OwnerCheckSuppressed("原因")]`。

### 媒体存储根接管（vuln-0023）

- `ConfigurationInfoService`：`SaveDrive`、`SavePhotoPathPrefix`、`SaveVideoPathPrefix`、
  `ReturnDownloadUrlPrefix`、`ReplaceUrlPrefix` 为安全敏感配置，创建/修改（含改名）仅特权用户可操作。
- 新增 `Services/Media/MediaPathGuard` 围栏；媒体物理文件删除全部经围栏解析：
  `MediaInfoService.DeletePhysicalFileAsync` / `CleanupRetrievedFilesAsync` / `ExternalLinkService.RemoveFileAsync`，
  越出媒体根目录的路径一律拒绝。

### 特权判定收紧

- `IsPrivilegedUserAsync` 从"持有 UserManagement 权限"改为"持有静态 admin 角色"：
  权限可被委托，被委托者不应绕过所有对象级归属校验（报告中期建议 #9）。

### 登录时序侧信道（vuln-0025）

- 账号不存在/停用的早退分支执行同参数哑 PBKDF2 计算（`PerformDummyPasswordWork`），
  消除"存在账号走哈希、不存在账号直回"的 ~5ms 时序差。

### SignalR 按组权限

- `Aria2Hub.JoinMonitorGroup` 需 `DFApp.Aria2.Default`；`DownloadNotificationHub.JoinDownloadGroup`
  需 `DFApp.Medias.Default`（直接读连接 JWT 的权限声明）。

### Downloader 回环加固与机密不落盘（vuln-0002）

- 新增 `LoopbackRequestGuard`：回环零配置放行仅限同源请求——Host 必须为本地主机名
  （localhost/127.0.0.1/[::1]），携带 Origin/Referer 时其主机也必须本地，封堵 drive-by CSRF 与 DNS rebinding。
- 新增 `SettingsSecrets`：三个机密（DFApp 密码、Apache 密码、管理令牌）支持环境变量注入
  （`DFAPP_DOWNLOADER_DFAPP_PASSWORD` / `DFAPP_DOWNLOADER_APACHE_PASSWORD` / `DFAPP_DOWNLOADER_MANAGEMENT_TOKEN`），
  优先于 settings.json，可实现明文密码不落盘。

### Git 历史清理（vuln-0001）

- 已执行 `git filter-repo --replace-text`：旧彩票代理令牌在全部历史中替换为 `REDACTED_LOTTERY_PROXY_TOKEN`，
  全历史检索 0 残留。**待运维**：`git push --force origin master`（会改写全部提交哈希，通知协作者重新克隆；
  fork/缓存中的旧对象仍存在，令牌已于 sql/26 轮换，无需再处理）。

### 依赖 CVE（vuln-0003/0004/0005/0007/0008/0009/0011/0012/0013）

- pnpm overrides 补齐：`lodash-es ^4.18.0`（此前只覆盖 lodash，element-plus 传递的 lodash-es 4.17.21 漏了）、
  `ws ^7.5.11`、`defu ^6.1.5`、`tar ^7.5.19`、`postcss ^8.5.18`、`rollup ^4.59.0`。
- vite：client 7.1.5 → 7.3.6；Downloader web 8.0.11 → 8.3.0。两端 pnpm build 通过。

### 未实施（需部署决策）

- PBKDF2 迭代次数 10000 低于 OWASP 建议（600k）：直接提高会使全部现存密码哈希失效（哈希格式未内嵌迭代数），
  需要哈希格式迁移方案（如迭代数前缀 + 登录时透明重哈希），建议单独排期。
- Apache 边缘 HSTS/CSP：部署侧配置，不在仓库内。

### 2026-09-10 Playwright 验证（第三轮修复闭环）

- 新增 `client/playwright.security.config.ts` 与 `client/tests/security-round3.spec.ts`：
  24 项用例覆盖提权封堵、改密加固、IDOR、SaveDrive、aria2 防护与 UI 改密对话框流程，
  在 api / desktop-4k（3840×2160@200%，即 CSS 1920×1080+DPR2）/ iphone-17（2622×1206，CSS 874×402+DPR3）三档全部通过。
- 运行方式：`python3 /tmp/pw_prepare.py`（或按 spec 头部注释重建测试用户）后
  `cd client && pnpm exec playwright test -c playwright.security.config.ts`。
  注意套件是有状态的（密码在用例间流转 v2→v3），重复运行前需重建测试用户。
- 环境备注：`@playwright/test@1.58.2` 对应的 1208 号 Chromium 构建在官方源与 npmmirror 均无法下载，
  配置中以 `launchOptions.executablePath` 显式复用本机缓存的 Chrome for Testing
  （`~/.cache/ms-playwright/chromium-1243`）；另需 `ASPNETCORE_ENVIRONMENT=Development`
  与项目 content root 才能读到 `appsettings.secrets.json` 的 JWT 密钥。
