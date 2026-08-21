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
using DFApp.Web.Mapping;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

    // ==================== 测试基础设施 ====================

    private LotteryResultJob CreateJob(SqlSugarClient db, FakeLotteryProxyServer server)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LotteryProxy:Url"] = server.BaseUrl,
            })
            .Build();

        return new LotteryResultJob(
            new SqlSugarRepository<LotteryResult, long>(db),
            new SqlSugarReadOnlyRepository<LotteryResult, long>(db),
            new SqlSugarRepository<LotteryPrizegrades, long>(db),
            new SqlSugarReadOnlyRepository<LotteryPrizegrades, long>(db),
            new LotteryMapper(),
            new SimpleHttpClientFactory(),
            configuration,
            new TestOutputLogger(_output),
            new FixedLocalTimeProvider(FakeToday));
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
                catch (Exception)
                {
                    break;
                }

                try
                {
                    await HandleAsync(ctx);
                }
                catch (Exception)
                {
                    // 单个请求处理异常不影响后续请求
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
    private sealed class TestOutputLogger(ITestOutputHelper output) : ILogger<LotteryResultJob>
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
