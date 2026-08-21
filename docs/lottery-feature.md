# 彩票开奖数据功能文档

## 目录
- [功能概述](#功能概述)
- [技术架构](#技术架构)
- [数据库设计](#数据库设计)
- [文件结构](#文件结构)
- [核心流程](#核心流程)
- [2026-08 断档修复](#2026-08-断档修复)
- [单元测试（TDD）](#单元测试tdd)
- [配置说明](#配置说明)
- [故障排查](#故障排查)

---

## 功能概述

自动从中福彩官网（www.cwl.gov.cn）抓取双色球、快乐8 开奖数据，入库并提供查询、统计、走势等服务。

### 核心功能
1. **历史数据初始化** - 首次运行从起始期号全量拉取（双色球 2013 起、快乐8 2020 起）
2. **每日增量补数** - Quartz 定时任务（每天 23:00）从库内最新期号续抓到当天
3. **手动触发补数** - 前端"彩票数据获取"页面提供"立即补数"按钮，`POST /api/app/lottery-data-fetch/trigger-job` 后台执行与定时任务完全相同的补数逻辑，立即返回
4. **奖级信息** - 每期开奖同步保存奖级（一等/二等奖注数与金额）
5. **手动抓取** - `LotteryDataFetchService` 提供按日期/页号的手动拉取入口

## 技术架构

- **ORM**: SqlSugar（`ISqlSugarRepository` / `ISqlSugarReadOnlyRepository`）
- **数据库**: SQLite（主库 DFApp.db）
- **后台任务**: Quartz.NET，cron `0 0 23 * * ?`
- **时钟**: `TimeProvider` 注入（可测试），生产为 `TimeProvider.System`
- **中转代理**: `DFApp.LotteryProxy`（独立进程，默认端口 5000），转发官网 findDrawNotice 接口

## 数据库设计

- `AppLotteryResult` - 开奖主表：Name（中文名）、Code（期号）、Date（"yyyy-MM-dd(周X)"）、Red/Blue、Sales/PoolMoney 等；`ExtraProperties TEXT NOT NULL`（存量表结构保留列，实体固定写 `"{}"`）
- `AppLotteryPrizegrades` - 奖级表：LotteryResultId 关联主表，Type/TypeNum/TypeMoney；同样带 `ExtraProperties TEXT NOT NULL`

## 文件结构

| 文件 | 职责 |
| --- | --- |
| `src/DFApp.Web/Background/LotteryResultJob.cs` | 定时任务：初始化 + 增量补数 |
| `src/DFApp.Web/Services/Lottery/LotteryDataFetchService.cs` | 手动抓取 |
| `src/DFApp.Web/DTOs/Lottery/LotteryConst.cs` | 彩种常量（中英文名、起始期号） |
| `src/DFApp.Web/DTOs/Lottery/LotteryInputDto.cs` | 上游响应 DTO（total/pageNo/pageSize/result） |
| `src/DFApp.Web/Mapping/LotteryMapper.cs` | Mapperly 实体映射 |
| `DFApp.LotteryProxy/` | 官网接口中转代理 |

## 核心流程

`LotteryResultJob.Execute` 对每个彩种（先双色球后快乐8）：

1. 库内无任何数据 → 全量初始化（2013-01-01 至今，pageNo 从 1 开始翻页）
2. 判断是否需要增量：周二/四/日（双色球开奖日）或快乐8（每日开奖）
3. 按彩种检查当天是否已有数据，没有则：
   - 更新奖级信息（失败仅记日志，不中断）
   - **按彩种**取最新期号的日期作为 dayStart
   - 从 dayStart 起翻页续抓（每页 30 条），写入主表 + 奖级表

上游契约（实测）：请求用英文名（name=ssq），响应用中文名；`pageNo=0` 返回 404，翻页必须从 1 开始。

## 2026-08 断档修复

现象：库内数据停在 2026-01（双色球 2026007 / 快乐8 2026017），之后每晚任务都无法续上。根因是 4 个叠加缺陷，全部已修：

1. **增量起点页号错误** - `GetCurrentLotteryResult(dayStart, 0, ...)` 传 pageNo=0，上游 nginx 直接 404，每次必失败。已改为从 1 开始。
2. **"今天已有数据"检查未按彩种过滤** - 双色球写入后，同晚快乐8 误判"今天有数据"而跳过。已加 `item.Name == lotteryType` 过滤。
3. **最新期号选取未按彩种过滤** - 快乐8 期号比双色球新，双色球把快乐8 的日期当起点导致漏抓/错乱。已在 `OrderByDescending(Code)` 前加 `Where(x => x.Name == lotteryType)`。
4. **实体缺少 ExtraProperties 列** - 存量表该列 NOT NULL 无默认值，实体不携带导致所有插入失败。实体已补 `public string ExtraProperties { get; set; } = "{}";`（LotteryResult、LotteryPrizegrades 两处）。

另将 `DateTime.Now` 全部替换为注入的 `TimeProvider`，使时钟可控可测。

任务加了静态 `SemaphoreSlim` 并发防护：手动触发与定时任务（或重复点击）重叠时，后一次直接跳过并记日志，避免并发写入产生重复期号。Quartz 每次执行都新建 Job 实例，因此锁必须为 static。

无需 SQL 脚本；部署后当晚 23:00 任务（或页面"立即补数"按钮）会自动从断档处补齐到当天。

## 单元测试（TDD）

`test/DFApp.Web.Tests/Background/LotteryResultJobTests.cs`，采用先红后绿开发：临时 SQLite 建生产同构表，`HttpListener` 起本地假中转代理（模拟上游分页、中英文彩种名、pageNo=0 返回 404），`FixedLocalTimeProvider` 固定时钟。

| 用例 | 覆盖缺陷 |
| --- | --- |
| 断档补数_应从最后日期续到今天并写入全部缺漏期数 | pageNo=0、ExtraProperties 缺失 |
| 今日已有其他彩种数据时_当前彩种仍应正常拉取 | 今天检查未按彩种过滤 |
| 断档起点_应取当前彩种自己的最新期号 | 最新期号选取未按彩种过滤 |
| 手动触发_应在后台执行完整补数并立即返回 | 手动触发接口（后台执行 + 立即返回） |
| 任务执行中重复触发_应直接跳过不产生重复数据 | 并发防护（假代理阻塞门构造慢上游场景） |

运行：`dotnet test test/DFApp.Web.Tests --filter "FullyQualifiedName~LotteryResultJobTests"`

## 配置说明

- `LotteryProxy:Url` - 中转代理地址，默认 `http://localhost:5000`
- `LotteryProxy:Token` - 中转代理共享密钥（`X-Proxy-Token` 请求头）；代理暴露公网时必配，与代理端 `ProxySettings:ProxyToken` 配成对，留空表示代理未启用令牌
- 中转代理需独立部署启动（见 `DFApp.LotteryProxy`）

## 故障排查

- **持续无法补数**：先 `curl` 中转代理确认存活；再看日志是否出现 404（pageNo 问题已修复，出现则说明上游契约再变）
- **写入报 NOT NULL 约束失败**：确认实体 `ExtraProperties` 默认值仍在
- **某彩种停更**：检查当天判断与最新期号选取是否带 `Name == lotteryType` 过滤
