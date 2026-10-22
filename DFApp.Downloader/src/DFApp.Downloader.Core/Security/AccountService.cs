using System.Collections.Concurrent;
using System.Security.Cryptography;
using DFApp.Downloader.Core.Data;
using DFApp.Downloader.Core.Entities;
using SqlSugar;

namespace DFApp.Downloader.Core.Security;

/// <summary>
/// 登录结果
/// </summary>
public enum LoginResult
{
    Ok,
    InvalidCredentials,
    Locked,
}

/// <summary>
/// 账号与登录会话管理：
/// - 账号持久化在 downloader.db 的 Accounts 表（PBKDF2 哈希）
/// - 会话保存在内存（重启后需重新登录），滑动过期
/// - 连续失败锁定，防暴力破解
/// </summary>
public class AccountService
{
    /// <summary>会话有效期（滑动续期）</summary>
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(7);

    /// <summary>连续失败锁定阈值</summary>
    private const int MaxFailedAttempts = 5;

    /// <summary>锁定时长</summary>
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(1);

    /// <summary>默认管理员用户名</summary>
    public const string AdminUsername = "admin";

    private readonly DownloaderDbContext _dbContext;

    private readonly ConcurrentDictionary<string, DateTime> _sessions = new();
    private readonly ConcurrentDictionary<string, (int Failures, DateTime LockedUntil)> _failedAttempts = new();

    public AccountService(DownloaderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 初始化账号表并种子默认 admin：
    /// 旧库升级时 Accounts 表由 CodeFirst 自动创建，无账号则种子 admin/admin（可用环境变量 DFAPP_DOWNLOADER_ADMIN_PASSWORD 指定初始密码）
    /// </summary>
    public void EnsureAdminInitialized()
    {
        using var db = _dbContext.CreateClient();
        if (db.Queryable<Account>().Any())
        {
            return;
        }

        var password = Environment.GetEnvironmentVariable("DFAPP_DOWNLOADER_ADMIN_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
        {
            password = AdminUsername;
        }

        CreateAccount(db, AdminUsername, password);
        Serilog.Log.Warning("已创建默认管理员账号 {Username}/{Username}，请登录后尽快修改密码（或使用环境变量 DFAPP_DOWNLOADER_ADMIN_PASSWORD 指定初始密码）",
            AdminUsername);
    }

    private static void CreateAccount(ISqlSugarClient db, string username, string password)
    {
        var (hash, salt, iterations) = PasswordHasher.Hash(password);
        db.Insertable(new Account
        {
            Username = username,
            PasswordHash = hash,
            PasswordSalt = salt,
            Iterations = iterations,
        }).ExecuteCommand();
    }

    /// <summary>
    /// 校验用户名口令；成功时创建会话并返回会话令牌，失败返回 null
    /// </summary>
    public string? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        if (_failedAttempts.TryGetValue(username, out var attempt) && DateTime.UtcNow < attempt.LockedUntil)
        {
            return null;
        }

        using var db = _dbContext.CreateClient();
        var account = db.Queryable<Account>().First(x => x.Username == username);
        var ok = account is not null
            && PasswordHasher.Verify(password, account.PasswordHash, account.PasswordSalt, account.Iterations);

        if (!ok)
        {
            RecordFailure(username);
            return null;
        }

        _failedAttempts.TryRemove(username, out _);
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        _sessions[token] = DateTime.UtcNow + SessionLifetime;
        CleanupSessions();
        Serilog.Log.Information("管理员 {Username} 登录成功", username);
        return token;
    }

    /// <summary>校验会话令牌，有效则滑动续期</summary>
    public bool ValidateSession(string? token)
    {
        if (string.IsNullOrEmpty(token) || !_sessions.TryGetValue(token, out var expires))
        {
            return false;
        }

        if (DateTime.UtcNow >= expires)
        {
            _sessions.TryRemove(token, out _);
            return false;
        }

        _sessions[token] = DateTime.UtcNow + SessionLifetime;
        return true;
    }

    /// <summary>注销会话</summary>
    public void Logout(string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _sessions.TryRemove(token, out _);
        }
    }

    /// <summary>修改口令（需校验旧口令）</summary>
    public bool ChangePassword(string username, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
        {
            return false;
        }

        using var db = _dbContext.CreateClient();
        var account = db.Queryable<Account>().First(x => x.Username == username);
        if (account is null
            || !PasswordHasher.Verify(oldPassword, account.PasswordHash, account.PasswordSalt, account.Iterations))
        {
            return false;
        }

        var (hash, salt, iterations) = PasswordHasher.Hash(newPassword);
        db.Updateable<Account>()
            .SetColumns(x => x.PasswordHash == hash)
            .SetColumns(x => x.PasswordSalt == salt)
            .SetColumns(x => x.Iterations == iterations)
            .SetColumns(x => x.UpdatedAt == DateTime.UtcNow)
            .Where(x => x.Id == account.Id)
            .ExecuteCommand();

        // 改密后作废所有会话，强制重新登录
        _sessions.Clear();
        Serilog.Log.Information("管理员 {Username} 已修改密码", username);
        return true;
    }

    private void RecordFailure(string username)
    {
        Serilog.Log.Warning("登录失败：{Username}", username);
        var (failures, _) = _failedAttempts.GetOrAdd(username, _ => (0, DateTime.MinValue));
        failures++;
        if (failures >= MaxFailedAttempts)
        {
            _failedAttempts[username] = (0, DateTime.UtcNow + LockoutDuration);
            Serilog.Log.Warning("登录连续失败 {Count} 次，账号 {Username} 被临时锁定 1 分钟", MaxFailedAttempts, username);
        }
        else
        {
            _failedAttempts[username] = (failures, DateTime.MinValue);
        }
    }

    private void CleanupSessions()
    {
        foreach (var (token, expires) in _sessions)
        {
            if (DateTime.UtcNow >= expires)
            {
                _sessions.TryRemove(token, out _);
            }
        }
    }
}
