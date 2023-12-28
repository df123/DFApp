using DF.Telegram.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace DF.Telegram.Permissions;

public class TelegramPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var meidaGroup = context.AddGroup(TelegramPermissions.Medias.Default, L("Permission:MediaTelegaram"));

        var mediaPermisson = meidaGroup.AddPermission(TelegramPermissions.Medias.Default, L("Permission:Medias"));
        mediaPermisson.AddChild(TelegramPermissions.Medias.Create, L("Permission:Medias.Create"));
        mediaPermisson.AddChild(TelegramPermissions.Medias.Edit, L("Permission:Medias.Edit"));
        mediaPermisson.AddChild(TelegramPermissions.Medias.Delete, L("Permission:Medias.Delete"));
        mediaPermisson.AddChild(TelegramPermissions.Medias.Download, L("Permission:Medias.Download"));

        var dynamicIPGroup = context.AddGroup(TelegramPermissions.DynamicIP.Default, L("Permission:DynamicIPTelegaram"));
        var dynamicIPPermisson = dynamicIPGroup.AddPermission(TelegramPermissions.DynamicIP.Default, L("Permission:DynamicIPIP"));
        dynamicIPPermisson.AddChild(TelegramPermissions.DynamicIP.Delete, L("Permission:DynamicIPIP.Delete"));

        var lotteryGroup = context.AddGroup(TelegramPermissions.Lottery.Default,L("Permission:LotteryTelegaram"));
        var lotteryPermission = lotteryGroup.AddPermission(TelegramPermissions.Lottery.Default,L("Permission:Lottery"));
        lotteryPermission.AddChild(TelegramPermissions.Lottery.Create, L("Permission:Lottery.Create"));
        lotteryPermission.AddChild(TelegramPermissions.Lottery.Edit, L("Permission:Lottery.Edit"));
        lotteryPermission.AddChild(TelegramPermissions.Lottery.Delete, L("Permission:Lottery.Delete"));

        var queueGroup = context.AddGroup(TelegramPermissions.QueueLog.Default, L("Permission:QueueLogTelegaram"));
        var queuePermission = queueGroup.AddPermission(TelegramPermissions.QueueLog.Default, L("Permission:QueueLog"));

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TelegramResource>(name);
    }
}
