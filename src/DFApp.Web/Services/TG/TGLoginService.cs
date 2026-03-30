using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DFApp.Web.Services.TG;

/// <summary>
/// Telegram 登录管理服务
/// </summary>
public class TGLoginService : AppServiceBase
{
    private readonly IServiceProvider _services;

    // TODO: ListenTelegramService 未迁移，暂时使用伪代码替代
    private readonly ListenTelegramService _listenTelegramService;

    public TGLoginService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        IServiceProvider services)
        : base(currentUser, permissionChecker)
    {
        _services = services;

        // 从已注册的 IHostedService 中获取 ListenTelegramService 实例
        _listenTelegramService = services.GetRequiredService<IEnumerable<IHostedService>>()
            .OfType<ListenTelegramService>()
            .FirstOrDefault() ?? throw new InvalidOperationException("ListenTelegramService is not registered.");
    }

    /// <summary>
    /// 获取登录状态
    /// </summary>
    public string Status()
    {
        switch (_listenTelegramService.ConfigNeeded)
        {
            case "connecting":
                return "WTelegram is connecting...";
            case "start":
                return "Please start WTelegram background service";
            case null:
                return $"Connected as {_listenTelegramService.User} Get all chats";
            default:
                return $@"Enter {_listenTelegramService.ConfigNeeded}: ";
        }
    }

    /// <summary>
    /// 配置登录
    /// </summary>
    public async Task<string> Config(string value)
    {
        return await _listenTelegramService.DoLogin(value);
    }

    /// <summary>
    /// 获取聊天列表
    /// </summary>
    public async Task<object> Chats()
    {
        if (_listenTelegramService.TGClinet == null)
            throw new InvalidOperationException("WTelegram client is not initialized. Please start the background service.");

        if (_listenTelegramService.User == null)
            throw new InvalidOperationException("Complete the login first");

        var chats = await _listenTelegramService.TGClinet.Messages_GetAllChats();
        return chats.chats;
    }
}
