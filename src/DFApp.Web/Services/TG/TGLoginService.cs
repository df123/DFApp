using System;
using System.Threading.Tasks;
using DFApp.Web.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DFApp.Web.Infrastructure;

namespace DFApp.Web.Services.TG;

/// <summary>
/// Telegram 登录管理服务
/// </summary>
/// <remarks>ListenTelegramService 已移除，该服务暂不可用</remarks>
public class TGLoginService : AppServiceBase
{
    private readonly IServiceProvider _services;

    // TODO: ListenTelegramService 未迁移，以下功能暂不可用
    // private readonly ListenTelegramService _listenTelegramService;

    public TGLoginService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        IServiceProvider services)
        : base(currentUser, permissionChecker)
    {
        _services = services;

        // TODO: ListenTelegramService 未迁移，暂时注释掉
        // _listenTelegramService = services.GetRequiredService<IEnumerable<IHostedService>>()
        //     .OfType<ListenTelegramService>()
        //     .FirstOrDefault() ?? throw new InvalidOperationException("ListenTelegramService is not registered.");
    }

    /// <summary>
    /// 获取登录状态
    /// </summary>
    public string Status()
    {
        // TODO: ListenTelegramService 未迁移，暂时返回不可用提示
        return "Telegram 服务暂不可用（ListenTelegramService 未迁移）";

        // switch (_listenTelegramService.ConfigNeeded)
        // {
        //     case "connecting":
        //         return "WTelegram is connecting...";
        //     case "start":
        //         return "Please start WTelegram background service";
        //     case null:
        //         return $"Connected as {_listenTelegramService.User} Get all chats";
        //     default:
        //         return $@"Enter {_listenTelegramService.ConfigNeeded}: ";
        // }
    }

    /// <summary>
    /// 配置登录
    /// </summary>
    public Task<string> Config(string value)
    {
        // TODO: ListenTelegramService 未迁移，暂时不可用
        throw new BusinessException("Telegram 服务暂不可用（ListenTelegramService 未迁移）");

        // return await _listenTelegramService.DoLogin(value);
    }

    /// <summary>
    /// 获取聊天列表
    /// </summary>
    public Task<object> Chats()
    {
        // TODO: ListenTelegramService 未迁移，暂时不可用
        throw new BusinessException("Telegram 服务暂不可用（ListenTelegramService 未迁移）");

        // if (_listenTelegramService.TGClinet == null)
        //     throw new InvalidOperationException("WTelegram client is not initialized. Please start the background service.");
        //
        // if (_listenTelegramService.User == null)
        //     throw new InvalidOperationException("Complete the login first");
        //
        // var chats = await _listenTelegramService.TGClinet.Messages_GetAllChats();
        // return chats.chats;
    }
}
