using DFApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace DFApp.Permissions;

public class DFAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var meidaGroup = context.AddGroup(DFAppPermissions.Medias.Default, L("Permission:MediaTelegaram"));

        var mediaPermisson = meidaGroup.AddPermission(DFAppPermissions.Medias.Default, L("Permission:Medias"));
        mediaPermisson.AddChild(DFAppPermissions.Medias.Create, L("Permission:Medias.Create"));
        mediaPermisson.AddChild(DFAppPermissions.Medias.Edit, L("Permission:Medias.Edit"));
        mediaPermisson.AddChild(DFAppPermissions.Medias.Delete, L("Permission:Medias.Delete"));
        mediaPermisson.AddChild(DFAppPermissions.Medias.Download, L("Permission:Medias.Download"));

        var dynamicIPGroup = context.AddGroup(DFAppPermissions.DynamicIP.Default, L("Permission:DynamicIPTelegaram"));
        var dynamicIPPermisson = dynamicIPGroup.AddPermission(DFAppPermissions.DynamicIP.Default, L("Permission:DynamicIPIP"));
        dynamicIPPermisson.AddChild(DFAppPermissions.DynamicIP.Delete, L("Permission:DynamicIPIP.Delete"));

        var lotteryGroup = context.AddGroup(DFAppPermissions.Lottery.Default, L("Permission:LotteryTelegaram"));
        var lotteryPermission = lotteryGroup.AddPermission(DFAppPermissions.Lottery.Default, L("Permission:Lottery"));
        lotteryPermission.AddChild(DFAppPermissions.Lottery.Create, L("Permission:Lottery.Create"));
        lotteryPermission.AddChild(DFAppPermissions.Lottery.Edit, L("Permission:Lottery.Edit"));
        lotteryPermission.AddChild(DFAppPermissions.Lottery.Delete, L("Permission:Lottery.Delete"));

        var queueGroup = context.AddGroup(DFAppPermissions.QueueLog.Default, L("Permission:QueueLogTelegaram"));
        var queuePermission = queueGroup.AddPermission(DFAppPermissions.QueueLog.Default, L("Permission:QueueLog"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<DFAppResource>(name);
    }
}
