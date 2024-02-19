﻿using DFApp.Localization;
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


        var managementBackgroundGroup = context.AddGroup(DFAppPermissions.ManagementBackground.Default, L("Permisssion:ManagementBackground"));
        var managementBackgroundPermission = managementBackgroundGroup.AddPermission(DFAppPermissions.ManagementBackground.Default, L("Permisssion:ManagementBackground"));
        managementBackgroundPermission.AddChild(DFAppPermissions.ManagementBackground.Restart, L("Permisssion:ManagementBackground:Restart"));
        managementBackgroundPermission.AddChild(DFAppPermissions.ManagementBackground.Stop, L("Permisssion:ManagementBackground:Stop"));


    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<DFAppResource>(name);
    }
}
