using SqlSugar;

namespace DFApp.Web.Data;

/// <summary>
/// Transient 独立库的请求作用域连接持有者。
/// 同一作用域内（如一次 HTTP 请求、一次 Quartz 任务执行）始终返回同一个客户端实例，
/// 保证镜像条目仓储、分词仓储与手工事务共享连接——事务才能同时覆盖两表写入。
/// </summary>
public class TransientDbConnection
{
    private readonly TransientDbContext _context;
    private ISqlSugarClient? _client;

    public TransientDbConnection(TransientDbContext context)
    {
        _context = context;
    }

    public ISqlSugarClient Client => _client ??= _context.CreateClient();
}
