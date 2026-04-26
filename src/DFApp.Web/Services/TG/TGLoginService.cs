using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Web.Background;
using DFApp.Web.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DFApp.Web.Services.TG;

/// <summary>
/// Telegram 登录管理服务
/// </summary>
public class TGLoginService
{
    private readonly ListenTelegramService _listenTelegramService;

    public TGLoginService(IServiceProvider services)
    {
        _listenTelegramService = services.GetRequiredService<IEnumerable<IHostedService>>()
            .OfType<ListenTelegramService>()
            .FirstOrDefault() ?? throw new InvalidOperationException("ListenTelegramService 未注册");
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
    /// <param name="value">用户输入（手机号、验证码、密码等）</param>
    /// <returns>null 表示登录完成，否则返回需要输入的参数名</returns>
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
            throw new BusinessException("WTelegram 客户端未初始化，请启动后台服务");

        if (_listenTelegramService.User == null)
            throw new BusinessException("请先完成登录");

        var chats = await _listenTelegramService.TGClinet.Messages_GetAllChats();
        return chats.chats;
    }
}
