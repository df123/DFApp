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

        var logSinkGroup = context.AddGroup(DFAppPermissions.LogSink.Default, L("Permission:LogSinkTelegaram"));
        var logSinkPermission = logSinkGroup.AddPermission(DFAppPermissions.LogSink.Default, L("Permission:LogSinkTelegaram"));
        logSinkPermission.AddChild(DFAppPermissions.LogSink.SignalRSink, L("Permission:SignalRSink"));
        logSinkPermission.AddChild(DFAppPermissions.LogSink.QueueSink, L("Permission:QueueSink"));


        var bookkeepingGroup = context.AddGroup(DFAppPermissions.Bookkeeping.Default, L("Permisssion:Bookkeeping"));
        var bookkeepingPermission = bookkeepingGroup.AddPermission(DFAppPermissions.Bookkeeping.Default, L("Permisssion:Bookkeeping"));

        var bookkeepingCategoryPermission = bookkeepingPermission.AddChild(DFAppPermissions.BookkeepingCategory.Default, L("Permisssion:BookkeepingCategory"));
        bookkeepingCategoryPermission.AddChild(DFAppPermissions.BookkeepingCategory.Create, L("Permisssion:BookkeepingCategory:Create"));
        bookkeepingCategoryPermission.AddChild(DFAppPermissions.BookkeepingCategory.Delete, L("Permisssion:BookkeepingCategory:Delete"));
        bookkeepingCategoryPermission.AddChild(DFAppPermissions.BookkeepingCategory.Edit, L("Permisssion:BookkeepingCategory:Modify"));

        var bookkeepingExpenditurePermission = bookkeepingPermission.AddChild(DFAppPermissions.BookkeepingExpenditure.Default, L("Permisssion:BookkeepingExpenditure"));
        bookkeepingExpenditurePermission.AddChild(DFAppPermissions.BookkeepingExpenditure.Create, L("Permisssion:BookkeepingExpenditure:Create"));
        bookkeepingExpenditurePermission.AddChild(DFAppPermissions.BookkeepingExpenditure.Delete, L("Permisssion:BookkeepingExpenditure:Delete"));
        bookkeepingExpenditurePermission.AddChild(DFAppPermissions.BookkeepingExpenditure.Edit, L("Permisssion:BookkeepingExpenditure:Modify"));
        bookkeepingExpenditurePermission.AddChild(DFAppPermissions.BookkeepingExpenditure.Analysis, L("Permisssion:BookkeepingExpenditure:Analysis"));
        var fileUploadDownloadGroup = context.AddGroup(DFAppPermissions.FileUploadDownload.Default, L("Permission:FileUploadDownload"));
        var fileUploaddownloadPermission = fileUploadDownloadGroup.AddPermission(DFAppPermissions.FileUploadDownload.Default, L("Permission:FileUploadDownload"));
        fileUploaddownloadPermission.AddChild(DFAppPermissions.FileUploadDownload.Upload, L("Permission:FileUploadDownload:Upload"));
        fileUploaddownloadPermission.AddChild(DFAppPermissions.FileUploadDownload.Download, L("Permission:FileUploadDownload:Download"));
        fileUploaddownloadPermission.AddChild(DFAppPermissions.FileUploadDownload.Delete, L("Permission:FileUploadDownload:Delete"));

        var configurationInfoGroup = context.AddGroup(DFAppPermissions.ConfigurationInfo.Default, L("Permisssion:ConfigurationInfo"));
        var configurationInfoPermission = configurationInfoGroup.AddPermission(DFAppPermissions.ConfigurationInfo.Default, L("Permisssion:ConfigurationInfo"));
        configurationInfoPermission.AddChild(DFAppPermissions.ConfigurationInfo.Create, L("Permisssion:ConfigurationInfo:Create"));
        configurationInfoPermission.AddChild(DFAppPermissions.ConfigurationInfo.Delete, L("Permisssion:ConfigurationInfo:Delete"));
        configurationInfoPermission.AddChild(DFAppPermissions.ConfigurationInfo.Edit, L("Permisssion:ConfigurationInfo:Modify"));

        var aria2Group = context.AddGroup(DFAppPermissions.Aria2.Default, L("Permisssion:Aria2"));
        var aria2Permission = aria2Group.AddPermission(DFAppPermissions.Aria2.Default, L("Permisssion:Aria2"));
        aria2Permission.AddChild(DFAppPermissions.Aria2.Link, L("Permisssion:Aria2:Link"));
        aria2Permission.AddChild(DFAppPermissions.Aria2.Delete, L("Permisssion:Aria2:Delete"));

        // 添加LogViewer权限组
        var logViewerGroup = context.AddGroup(DFAppPermissions.LogViewer.Default, L("LogViewer"));
        logViewerGroup.AddPermission(DFAppPermissions.LogViewer.Default, L("LogViewer"));

        // 添加RSS权限组
        var rssGroup = context.AddGroup(DFAppPermissions.Rss.Default, L("Permission:Rss"));
        var rssPermission = rssGroup.AddPermission(DFAppPermissions.Rss.Default, L("Permission:Rss"));
        rssPermission.AddChild(DFAppPermissions.Rss.Create, L("Permission:Rss.Create"));
        rssPermission.AddChild(DFAppPermissions.Rss.Update, L("Permission:Rss.Update"));
        rssPermission.AddChild(DFAppPermissions.Rss.Delete, L("Permission:Rss.Delete"));
        rssPermission.AddChild(DFAppPermissions.Rss.Download, L("Permission:Rss.Download"));

        // 添加FileFilter权限组
        var fileFilterGroup = context.AddGroup(DFAppPermissions.FileFilter.Default, L("Permission:FileFilter"));
        var fileFilterPermission = fileFilterGroup.AddPermission(DFAppPermissions.FileFilter.Default, L("Permission:FileFilter"));
        fileFilterPermission.AddChild(DFAppPermissions.FileFilter.Create, L("Permission:FileFilter.Create"));
        fileFilterPermission.AddChild(DFAppPermissions.FileFilter.Edit, L("Permission:FileFilter.Edit"));
        fileFilterPermission.AddChild(DFAppPermissions.FileFilter.Delete, L("Permission:FileFilter.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<DFAppResource>(name);
    }
}
