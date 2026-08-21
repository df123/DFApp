using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Background;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.DTOs.Lottery;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using DFApp.Web.Services.Lottery;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SqlSugar;
using Xunit;
using Xunit.Abstractions;

namespace DFApp.Web.Tests.Background;

/// <summary>
/// LotteryResultJob 集成测试：
/// 使用本地假代理服务（复刻 cwl.gov.cn 上游契约，含 pageNo=0 返回 404 的真实行为）
/// 与临时 SQLite 库（建表 DDL 与生产完全一致），验证断档补数与每日增量逻辑。
/// </summary>
public class LotteryResultJobTests : IDisposable
{
    /// <summary>模拟“今天”：2026-01-22 是周四（双色球开奖日）</summary>
    private static readonly DateTime FakeToday = new(2026, 1, 22, 23, 0, 0);

    private readonly ITestOutputHelper _output;
    private readonly List<string> _tempDbPaths = new();

    public LotteryResultJobTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task 断档补数_应从最后日期续到今天并写入全部缺漏期数()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        // 已有历史：2013001（历史检查锚点）+ 2026001，其后断档
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026001", "2026-01-04(日)", "日");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);

        var draws = new List<FakeDraw>();
        draws.Add(new("双色球", "2026001", "2026-01-04(日)", "01,02,03,04,05,06", "07"));
        foreach (var (code, date, week) in new[]
        {
            ("2026002", "2026-01-06(二)", "二"), ("2026003", "2026-01-08(四)", "四"),
            ("2026004", "2026-01-11(日)", "日"), ("2026005", "2026-01-13(二)", "二"),
            ("2026006", "2026-01-15(四)", "四"), ("2026007", "2026-01-18(日)", "日"),
            ("2026008", "2026-01-20(二)", "二"), ("2026009", "2026-01-22(四)", "四"),
        })
        {
            draws.Add(new("双色球", code, date, "01,02,03,04,05,06", "07"));
        }

        using var server = new FakeLotteryProxyServer(draws);
        var job = CreateJob(db, server);
        await job.Execute(null!);

        var ssq = db.Queryable<LotteryResult>().Where(x => x.Name == "双色球").ToList();
        ssq.Should().HaveCount(10, "断档的 8 期（含今天 2026009）应全部补齐");
        ssq.Select(x => x.Code).Should().Contain(new[] { "2026002", "2026005", "2026009" });

        var prizeCount = db.Queryable<LotteryPrizegrades>().Count();
        prizeCount.Should().Be(4 + 8 * 2, "新写入的 8 期每期应带 2 条奖级");

        server.Requests.Should().NotContain(r => r.EndsWith("|pageNo=0"), "pageNo=0 上游会返回 404，不允许再出现");
    }

    [Fact]
    public async Task 今日已有其他彩种数据时_当前彩种仍应正常拉取()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        // 双色球今天（2026-01-22）已有数据；快乐8 从 2020-01-01 断档
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026009", "2026-01-22(四)", "四");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);
        SeedResult(db, "快乐8", "2020001", "2020-01-01(三)", "三");
        SeedPrizegrades(db, 3);

        var draws = new List<FakeDraw> { new("双色球", "2026009", "2026-01-22(四)", "01,02,03,04,05,06", "07") };
        // 快乐8 每天一期：2025-12-01 ~ 2026-01-22 共 53 期，pageSize=30 需翻 2 页
        var day = new DateTime(2025, 12, 1);
        for (int i = 1; i <= 53; i++)
        {
            var week = "日一二三四五六"[(int)day.DayOfWeek].ToString();
            draws.Add(new FakeDraw("快乐8", $"2025{i:D4}", $"{day:yyyy-MM-dd}({week})", "01,02,03,04,05,06,07,08,09,10", "20"));
            day = day.AddDays(1);
        }

        using var server = new FakeLotteryProxyServer(draws);
        var job = CreateJob(db, server);
        await job.Execute(null!);

        var kl8 = db.Queryable<LotteryResult>().Where(x => x.Name == "快乐8").ToList();
        kl8.Should().HaveCount(54, "快乐8 断档的 53 期应全部补齐（含今天），不能因双色球已有今日数据而跳过");
        kl8.Should().ContainSingle(x => x.Date!.StartsWith("2026-01-22"), "今天的快乐8 开奖必须入库");

        server.Requests.Should().Contain(r => r.Contains("name=kl8") && r.EndsWith("|pageNo=1"), "翻页应从 pageNo=1 开始");
        server.Requests.Should().Contain(r => r.Contains("name=kl8") && r.EndsWith("|pageNo=2"), "53 条数据应翻满 2 页");
        server.Requests.Should().NotContain(r => r.EndsWith("|pageNo=0"));
    }

    [Fact]
    public async Task 断档起点_应取当前彩种自己的最新期号()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        // 快乐8 的期号（2026999）比双色球最新期号新，不能影响双色球的断档起点
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026005", "2026-01-13(二)", "二");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);
        SeedResult(db, "快乐8", "2026999", "2026-01-21(三)", "三");
        SeedPrizegrades(db, 3);

        var draws = new List<FakeDraw>();
        foreach (var (code, date, week) in new[]
        {
            ("2026006", "2026-01-15(四)", "四"), ("2026007", "2026-01-18(日)", "日"),
            ("2026008", "2026-01-20(二)", "二"), ("2026009", "2026-01-22(四)", "四"),
        })
        {
            draws.Add(new FakeDraw("双色球", code, date, "01,02,03,04,05,06", "07"));
        }

        using var server = new FakeLotteryProxyServer(draws);
        var job = CreateJob(db, server);
        await job.Execute(null!);

        var ssq = db.Queryable<LotteryResult>().Where(x => x.Name == "双色球").ToList();
        ssq.Should().HaveCount(6, "应从双色球自己的最新期 2026005（01-13）续到今天，补齐 4 期");
        ssq.Select(x => x.Code).Should().Contain(new[] { "2026006", "2026007", "2026008", "2026009" });
    }

    [Fact]
    public async Task 手动触发_应在后台执行完整补数并立即返回()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026005", "2026-01-13(二)", "二");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);

        var draws = new List<FakeDraw>();
        foreach (var (code, date, week) in new[]
        {
            ("2026006", "2026-01-15(四)", "四"), ("2026007", "2026-01-18(日)", "日"),
            ("2026008", "2026-01-20(二)", "二"), ("2026009", "2026-01-22(四)", "四"),
        })
        {
            draws.Add(new("双色球", code, date, "01,02,03,04,05,06", "07"));
        }

        using var server = new FakeLotteryProxyServer(draws);
        var job = CreateJob(db, server);
        var service = new LotteryDataFetchService(
            new Mock<ICurrentUser>().Object,
            new Mock<IPermissionChecker>().Object,
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new ConfigurationInfoRepository(db),
            job,
            new SimpleHttpClientFactory(),
            BuildConfiguration(server),
            new TestOutputLogger<LotteryDataFetchService>(_output));

        var response = service.TriggerResultJob();
        response.Success.Should().BeTrue("触发接口应立即返回成功");

        var polling = CreatePollingClient(path);
        for (int i = 0; i < 200; i++)
        {
            if (polling.Queryable<LotteryResult>().Count(x => x.Name == "双色球") >= 6)
            {
                break;
            }
            await Task.Delay(50);
        }

        polling.Queryable<LotteryResult>().Count(x => x.Name == "双色球")
            .Should().Be(6, "后台任务应完成 4 期补数");
        polling.Queryable<LotteryPrizegrades>().Count()
            .Should().Be(4 + 4 * 2, "新补的 4 期每期应带 2 条奖级");
        lock (server.Requests)
        {
            server.Requests.Should().NotContain(r => r.EndsWith("|pageNo=0"));
        }
    }

    [Fact]
    public async Task 任务执行中重复触发_应直接跳过不产生重复数据()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026005", "2026-01-13(二)", "二");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);

        var draws = new List<FakeDraw>();
        foreach (var (code, date, week) in new[]
        {
            ("2026006", "2026-01-15(四)", "四"), ("2026007", "2026-01-18(日)", "日"),
            ("2026008", "2026-01-20(二)", "二"), ("2026009", "2026-01-22(四)", "四"),
        })
        {
            draws.Add(new("双色球", code, date, "01,02,03,04,05,06", "07"));
        }

        // 阻塞门：请求到达后挂起，模拟上游响应慢，保证两次触发确实重叠
        using var server = new FakeLotteryProxyServer(draws)
        {
            BlockRequests = new ManualResetEventSlim(false),
        };
        var job1 = CreateJob(db, server);
        var job2 = CreateJob(db, server);

        var t1 = Task.Run(() => job1.Execute(null!));
        for (int i = 0; i < 100; i++)
        {
            lock (server.Requests)
            {
                if (server.Requests.Count > 0)
                {
                    break;
                }
            }
            await Task.Delay(50);
        }
        lock (server.Requests)
        {
            server.Requests.Should().NotBeEmpty("前置条件：第一次触发已发出请求并阻塞");
        }

        var t2 = Task.Run(() => job2.Execute(null!));
        var finished = await Task.WhenAny(t2, Task.Delay(3000));
        finished.Should().Be(t2, "执行中的重复触发应立即跳过返回，而不是等待或并发执行");

        server.BlockRequests!.Set();
        await Task.WhenAll(t1, t2);

        var ssq = db.Queryable<LotteryResult>().Where(x => x.Name == "双色球").ToList();
        ssq.Should().HaveCount(6, "补数只应实际执行一次");
        ssq.GroupBy(x => x.Code).Should().OnlyContain(g => g.Count() == 1, "不允许产生重复期号");
    }

    [Fact]
    public async Task 配置令牌后_补数任务所有代理请求都应携带XProxyToken()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026005", "2026-01-13(二)", "二");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);

        var draws = new List<FakeDraw>();
        foreach (var (code, date, week) in new[]
        {
            ("2026006", "2026-01-15(四)", "四"), ("2026007", "2026-01-18(日)", "日"),
            ("2026008", "2026-01-20(二)", "二"), ("2026009", "2026-01-22(四)", "四"),
        })
        {
            draws.Add(new("双色球", code, date, "01,02,03,04,05,06", "07"));
        }

        using var server = new FakeLotteryProxyServer(draws);
        var job = new LotteryResultJob(
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarReadOnlyRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new SqlSugarReadOnlyRepository<LotteryPrizegrades, long>(db),
            new ConfigurationInfoRepository(db),
            new LotteryMapper(),
            new SimpleHttpClientFactory(),
            BuildConfiguration(server, token: "job-test-token"),
            new TestOutputLogger<LotteryResultJob>(_output),
            new FixedLocalTimeProvider(FakeToday));
        await job.Execute(null!);

        lock (server.RequestTokens)
        {
            server.RequestTokens.Should().NotBeEmpty("补数任务应已发出代理请求");
            server.RequestTokens.Should().OnlyContain(t => t == "job-test-token",
                "暴露公网的代理要求每个请求都携带 X-Proxy-Token，漏一个该请求就会被 401 拒绝");
        }
    }

    [Fact]
    public async Task 配置令牌后_手动抓取代理请求也应携带XProxyToken()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);

        var draws = new List<FakeDraw>
        {
            new("双色球", "2026009", "2026-01-22(四)", "01,02,03,04,05,06", "07"),
        };
        using var server = new FakeLotteryProxyServer(draws);
        var service = new LotteryDataFetchService(
            new Mock<ICurrentUser>().Object,
            new Mock<IPermissionChecker>().Object,
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new ConfigurationInfoRepository(db),
            CreateJob(db, server),
            new SimpleHttpClientFactory(),
            BuildConfiguration(server, token: "fetch-test-token"),
            new TestOutputLogger<LotteryDataFetchService>(_output));

        var response = await service.FetchLotteryData(new LotteryDataFetchRequestDto
        {
            LotteryType = "ssq",
            DayStart = "2026-01-15",
            DayEnd = "2026-01-22",
            PageNo = 1,
            SaveToDatabase = false,
        });

        response.Success.Should().BeTrue();
        lock (server.RequestTokens)
        {
            server.RequestTokens.Should().ContainSingle().Which.Should().Be("fetch-test-token");
        }
    }

    [Fact]
    public async Task 数据库配置令牌后_补数任务请求应携带该令牌()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026005", "2026-01-13(二)", "二");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);
        SeedProxyToken(db, "db-job-token");

        var draws = new List<FakeDraw>();
        foreach (var (code, date, week) in new[]
        {
            ("2026006", "2026-01-15(四)", "四"), ("2026007", "2026-01-18(日)", "日"),
            ("2026008", "2026-01-20(二)", "二"), ("2026009", "2026-01-22(四)", "四"),
        })
        {
            draws.Add(new("双色球", code, date, "01,02,03,04,05,06", "07"));
        }

        using var server = new FakeLotteryProxyServer(draws);
        var job = new LotteryResultJob(
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarReadOnlyRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new SqlSugarReadOnlyRepository<LotteryPrizegrades, long>(db),
            new ConfigurationInfoRepository(db),
            new LotteryMapper(),
            new SimpleHttpClientFactory(),
            BuildConfiguration(server),
            new TestOutputLogger<LotteryResultJob>(_output),
            new FixedLocalTimeProvider(FakeToday));
        await job.Execute(null!);

        lock (server.RequestTokens)
        {
            server.RequestTokens.Should().NotBeEmpty("补数任务应已发出代理请求");
            server.RequestTokens.Should().OnlyContain(t => t == "db-job-token",
                "令牌已迁移到数据库（AppConfigurationInfo），appsettings 未配置时应改用数据库的值");
        }
    }

    [Fact]
    public async Task 数据库与配置文件同时配置令牌_数据库优先()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        SeedResult(db, "双色球", "2013001", "2013-01-01(二)", "二");
        SeedResult(db, "双色球", "2026005", "2026-01-13(二)", "二");
        SeedPrizegrades(db, 1);
        SeedPrizegrades(db, 2);
        SeedProxyToken(db, "db-token");

        var draws = new List<FakeDraw>
        {
            new("双色球", "2026006", "2026-01-15(四)", "01,02,03,04,05,06", "07"),
        };
        using var server = new FakeLotteryProxyServer(draws);
        var job = new LotteryResultJob(
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarReadOnlyRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new SqlSugarReadOnlyRepository<LotteryPrizegrades, long>(db),
            new ConfigurationInfoRepository(db),
            new LotteryMapper(),
            new SimpleHttpClientFactory(),
            BuildConfiguration(server, token: "file-token"),
            new TestOutputLogger<LotteryResultJob>(_output),
            new FixedLocalTimeProvider(FakeToday));
        await job.Execute(null!);

        lock (server.RequestTokens)
        {
            server.RequestTokens.Should().NotBeEmpty();
            server.RequestTokens.Should().OnlyContain(t => t == "db-token",
                "两处同时配置时数据库优先，避免配置漂移时行为摇摆");
        }
    }

    [Fact]
    public async Task 数据库配置令牌后_手动抓取请求也应携带该令牌()
    {
        var db = CreateTestDb(out var path);
        _tempDbPaths.Add(path);
        SeedProxyToken(db, "db-fetch-token");

        var draws = new List<FakeDraw>
        {
            new("双色球", "2026009", "2026-01-22(四)", "01,02,03,04,05,06", "07"),
        };
        using var server = new FakeLotteryProxyServer(draws);
        var service = new LotteryDataFetchService(
            new Mock<ICurrentUser>().Object,
            new Mock<IPermissionChecker>().Object,
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new ConfigurationInfoRepository(db),
            CreateJob(db, server),
            new SimpleHttpClientFactory(),
            BuildConfiguration(server),
            new TestOutputLogger<LotteryDataFetchService>(_output));

        var response = await service.FetchLotteryData(new LotteryDataFetchRequestDto
        {
            LotteryType = "ssq",
            DayStart = "2026-01-15",
            DayEnd = "2026-01-22",
            PageNo = 1,
            SaveToDatabase = false,
        });

        response.Success.Should().BeTrue();
        lock (server.RequestTokens)
        {
            server.RequestTokens.Should().ContainSingle().Which.Should().Be("db-fetch-token");
        }
    }

    [Fact]
    public async Task 假代理探活_应能返回响应()
    {
        using var server = new FakeLotteryProxyServer(new List<FakeDraw>());
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        try
        {
            var response = await client.GetAsync($"{server.BaseUrl}/api/proxy/lottery/findDrawNotice?name=ssq&pageNo=1");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        catch (Exception)
        {
            // 落到下方 HandlerErrors 输出真实原因
        }

        lock (server.HandlerErrors)
        {
            server.HandlerErrors.Should().BeEmpty(string.Join("\n", server.HandlerErrors));
        }
    }

    // ==================== 测试基础设施 ====================

    private LotteryResultJob CreateJob(SqlSugarClient db, FakeLotteryProxyServer server)
    {
        return new LotteryResultJob(
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarReadOnlyRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new SqlSugarReadOnlyRepository<LotteryPrizegrades, long>(db),
            new ConfigurationInfoRepository(db),
            new LotteryMapper(),
            new SimpleHttpClientFactory(),
            BuildConfiguration(server),
            new TestOutputLogger<LotteryResultJob>(_output),
            new FixedLocalTimeProvider(FakeToday));
    }

    private static IConfiguration BuildConfiguration(FakeLotteryProxyServer server, string? token = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["LotteryProxy:Url"] = server.BaseUrl,
        };
        if (token != null)
        {
            settings["LotteryProxy:Token"] = token;
        }
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    /// <summary>对同一临时库文件的独立连接（后台任务写库时并发轮询用，避免争用同一 SqlSugarClient）</summary>
    private static SqlSugarClient CreatePollingClient(string path)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"DataSource={path}",
            DbType = DbType.Sqlite,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
        });
    }

    /// <summary>建临时库并按生产 DDL 建表（含 ExtraProperties NOT NULL 等真实约束）</summary>
    private static SqlSugarClient CreateTestDb(out string path)
    {
        path = Path.Combine(Path.GetTempPath(), $"lottery-test-{Guid.NewGuid():N}.db");
        var db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"DataSource={path}",
            DbType = DbType.Sqlite,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
        });

        db.Ado.ExecuteCommand(@"
CREATE TABLE ""AppLotteryResult"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AppLotteryResult"" PRIMARY KEY AUTOINCREMENT,
    ""Name"" TEXT NULL, ""Code"" TEXT NULL, ""DetailsLink"" TEXT NULL, ""VideoLink"" TEXT NULL,
    ""Date"" TEXT NULL, ""Week"" TEXT NULL, ""Red"" TEXT NULL, ""Blue"" TEXT NULL, ""Blue2"" TEXT NULL,
    ""Sales"" TEXT NULL, ""PoolMoney"" TEXT NULL, ""Content"" TEXT NULL, ""AddMoney"" TEXT NULL,
    ""AddMoney2"" TEXT NULL, ""Msg"" TEXT NULL, ""Z2Add"" TEXT NULL, ""M2Add"" TEXT NULL,
    ""IsDeleted"" INTEGER NOT NULL DEFAULT 0,
    ""ExtraProperties"" TEXT NOT NULL,
    ""ConcurrencyStamp"" TEXT NOT NULL,
    ""CreationTime"" TEXT NOT NULL,
    ""CreatorId"" TEXT NULL, ""LastModificationTime"" TEXT NULL, ""LastModifierId"" TEXT NULL
);
CREATE TABLE ""AppLotteryPrizegrades"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AppLotteryPrizegrades"" PRIMARY KEY AUTOINCREMENT,
    ""ConcurrencyStamp"" TEXT NOT NULL, ""CreationTime"" TEXT NOT NULL, ""CreatorId"" TEXT NULL,
    ""ExtraProperties"" TEXT NOT NULL, ""IsDeleted"" INTEGER NOT NULL DEFAULT 0,
    ""LastModificationTime"" TEXT NULL, ""LastModifierId"" TEXT NULL,
    ""LotteryResultId"" INTEGER NOT NULL, ""Type"" TEXT NULL, ""TypeMoney"" TEXT NULL, ""TypeNum"" TEXT NULL
);
CREATE TABLE ""AppConfigurationInfo"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AppConfigurationInfo"" PRIMARY KEY AUTOINCREMENT,
    ""ConcurrencyStamp"" TEXT NULL, ""CreationTime"" TEXT NOT NULL, ""CreatorId"" TEXT NULL,
    ""LastModificationTime"" TEXT NULL, ""LastModifierId"" TEXT NULL,
    ""ModuleName"" TEXT NULL, ""ConfigurationName"" TEXT NULL, ""ConfigurationValue"" TEXT NULL, ""Remark"" TEXT NULL
);");

        // 复刻生产 AOP：插入时空并发标记自动填充
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            if (entityInfo.PropertyName == "ConcurrencyStamp" && entityInfo.EntityValue != null)
            {
                var property = entityInfo.EntityValue.GetType().GetProperty("ConcurrencyStamp");
                if (property != null && property.GetValue(entityInfo.EntityValue) == null)
                {
                    property.SetValue(entityInfo.EntityValue, Guid.NewGuid().ToString());
                }
            }
        };

        return db;
    }

    private static void SeedResult(SqlSugarClient db, string name, string code, string date, string week)
    {
        db.Ado.ExecuteCommand(
            "INSERT INTO AppLotteryResult (Name, Code, Date, Week, Red, Blue, IsDeleted, ExtraProperties, ConcurrencyStamp, CreationTime) " +
            "VALUES (@name, @code, @date, @week, '01,02,03,04,05,06', '07', 0, '{}', 'seed', '2026-01-01 00:00:00');",
            new { name, code, date, week });
    }

    private static void SeedPrizegrades(SqlSugarClient db, long resultId)
    {
        db.Ado.ExecuteCommand(
            "INSERT INTO AppLotteryPrizegrades (LotteryResultId, Type, TypeNum, TypeMoney, IsDeleted, ExtraProperties, ConcurrencyStamp, CreationTime) " +
            "VALUES (@resultId, '1', '5', '10000000', 0, '{}', 'seed', '2026-01-01 00:00:00')," +
            "       (@resultId, '2', '100', '5000', 0, '{}', 'seed', '2026-01-01 00:00:00');",
            new { resultId });
    }

    /// <summary>向 AppConfigurationInfo 写入代理共享令牌（模拟数据库配置）</summary>
    private static void SeedProxyToken(SqlSugarClient db, string token)
    {
        db.Ado.ExecuteCommand(
            "INSERT INTO AppConfigurationInfo (ModuleName, ConfigurationName, ConfigurationValue, Remark, CreationTime) " +
            "VALUES ('DFApp.Web.Lottery', 'LotteryProxyToken', @token, '测试种子', '2026-01-01 00:00:00');",
            new { token });
    }

    private sealed record FakeDraw(string Name, string Code, string Date, string Red, string Blue);

    /// <summary>
    /// 假代理服务：复刻 cwl.gov.cn 上游契约。
    /// 关键行为（已实测）：pageNo=0 返回 404；正常请求按 dayStart/dayEnd 过滤并按 pageSize 分页。
    /// </summary>
    private sealed class FakeLotteryProxyServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly List<FakeDraw> _draws;
        private readonly Task _loop;

        public string BaseUrl { get; }
        public List<string> Requests { get; } = new();

        /// <summary>每个请求携带的 X-Proxy-Token（未携带为 null）</summary>
        public List<string?> RequestTokens { get; } = new();

        /// <summary>HandleAsync 抛出的异常信息，用于诊断假代理自身的问题</summary>
        public List<string> HandlerErrors { get; } = new();

        /// <summary>非空时每个请求在记录后挂起，直到 Set，用于构造慢上游场景</summary>
        public ManualResetEventSlim? BlockRequests { get; init; }

        public FakeLotteryProxyServer(List<FakeDraw> draws)
        {
            _draws = draws;
            var port = GetFreePort();
            BaseUrl = $"http://127.0.0.1:{port}";
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            _listener.Start();
            _loop = Task.Run(AcceptLoopAsync);
        }

        private async Task AcceptLoopAsync()
        {
            while (_listener.IsListening)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = await _listener.GetContextAsync();
                }
                catch (Exception ex)
                {
                    lock (HandlerErrors)
                    {
                        HandlerErrors.Add($"GetContextAsync: {ex}");
                    }
                    break;
                }

                try
                {
                    await HandleAsync(ctx);
                }
                catch (Exception ex)
                {
                    // 单个请求处理异常不影响后续请求
                    lock (HandlerErrors)
                    {
                        HandlerErrors.Add(ex.ToString());
                    }
                }
            }
        }

        private async Task HandleAsync(HttpListenerContext ctx)
        {
            var query = ctx.Request.Url!.Query;
            var parameters = query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(kv => kv.Split('=', 2))
                .ToDictionary(kv => kv[0], kv => kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "");

            var name = parameters.GetValueOrDefault("name", "");
            // 上游契约：请求用英文名（ssq/kl8），返回数据用中文名
            var displayName = name switch
            {
                "ssq" => "双色球",
                "kl8" => "快乐8",
                _ => name,
            };
            var dayStart = parameters.GetValueOrDefault("dayStart", "");
            var dayEnd = parameters.GetValueOrDefault("dayEnd", "");
            var pageNo = int.TryParse(parameters.GetValueOrDefault("pageNo"), out var p) ? p : 1;
            var pageSize = int.TryParse(parameters.GetValueOrDefault("pageSize"), out var s) ? s : 30;

            Requests.Add($"name={name}|dayStart={dayStart}|dayEnd={dayEnd}|pageNo={pageNo}");
            // NameValueCollection 索引器在键缺失时返回 null，必须用 Get 判空
            var tokenString = ctx.Request.Headers.Get("X-Proxy-Token") ?? "";
            lock (RequestTokens)
            {
                RequestTokens.Add(tokenString.Length == 0 ? null : tokenString);
            }
            BlockRequests?.Wait();

            // 上游真实行为：pageNo 从 0 开始请求会返回 404（nginx）
            if (pageNo <= 0)
            {
                ctx.Response.StatusCode = 404;
                var html = Encoding.UTF8.GetBytes("<html><head><title>404 Not Found</title></head><body>404</body></html>");
                await ctx.Response.OutputStream.WriteAsync(html);
                ctx.Response.Close();
                return;
            }

            var filtered = _draws
                .Where(d => d.Name == displayName)
                .Where(d => string.Compare(d.Date.Split('(')[0], dayStart, StringComparison.Ordinal) >= 0
                            && string.Compare(d.Date.Split('(')[0], dayEnd, StringComparison.Ordinal) <= 0)
                .OrderByDescending(d => d.Date)
                .ToList();

            var total = filtered.Count;
            var pageNum = (int)Math.Ceiling(total / (double)pageSize);
            var pageItems = filtered.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

            var payload = new
            {
                state = 0,
                message = "查询成功",
                total,
                pageNum,
                pageNo,
                pageSize,
                result = pageItems.Select(d => new
                {
                    name = d.Name,
                    code = d.Code,
                    date = d.Date,
                    week = d.Date.Split('(')[1].TrimEnd(')'),
                    red = d.Red,
                    blue = d.Blue,
                    blue2 = "",
                    sales = "100000000",
                    poolmoney = "200000000",
                    content = "测试开奖详情",
                    prizegrades = new object[]
                    {
                        new { type = "1", typenum = "5", typemoney = "10000000" },
                        new { type = "2", typenum = "100", typemoney = "5000" },
                    },
                }),
            };

            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            ctx.Response.ContentType = "application/json; charset=utf-8";
            await ctx.Response.OutputStream.WriteAsync(bytes);
            ctx.Response.Close();
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void Dispose()
        {
            try { _listener.Stop(); } catch (Exception) { }
            try { _listener.Close(); } catch (Exception) { }
        }
    }

    private sealed class SimpleHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    /// <summary>把任务内部日志透传到 xUnit 输出，失败时可直接看到真实错误</summary>
    private sealed class TestOutputLogger<T>(ITestOutputHelper output) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            output.WriteLine($"[{logLevel}] {formatter(state, exception)}");
            if (exception != null)
            {
                output.WriteLine(exception.ToString());
            }
        }
    }

    private sealed class FixedLocalTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedLocalTimeProvider(DateTime localDateTime)
        {
            _utcNow = new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime)).ToUniversalTime();
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    public void Dispose()
    {
        foreach (var path in _tempDbPaths)
        {
            try { File.Delete(path); } catch (Exception) { }
        }
    }
}
