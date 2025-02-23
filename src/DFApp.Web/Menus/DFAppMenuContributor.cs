using System.Drawing;
using System.Threading.Tasks;
using DFApp.Localization;
using DFApp.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace DFApp.Web.Menus;

public class DFAppMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<DFAppResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                DFAppMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fas fa-home",
                order: 0
            )
        );

        if (await context.IsGrantedAsync(DFAppPermissions.Medias.Default))
        {
            var mediaFirst = new ApplicationMenuItem(
                DFAppMenus.Media.Default
                , l["Menu:TG:Media"]
                , "~/TG"
                , icon: "fas fa-play"
                , order: 1);

            mediaFirst.AddItem(
                new ApplicationMenuItem(
                    DFAppMenus.Media.Default
                    , l["Menu:TG:Media"]
                    , "~/TG/Media"
                    , icon: "fas fa-play"
                    , order: 1));

            mediaFirst.AddItem(
                new ApplicationMenuItem(
                    DFAppMenus.Media.ExternalLink
                    , l["Menu:TG:Media:ExternalLink"]
                    , "~/TG/Media/ExternalLink"
                    , icon: "fas fa-link"
                    , order: 2));

            mediaFirst.AddItem(
                new ApplicationMenuItem(
                    DFAppMenus.Media.Chart
                    , l["Menu:TG:Media:Chart"]
                    , "~/TG/Media/Chart"
                    , icon: "fas fa-chart-bar"
                    , order: 3));

            mediaFirst.AddItem(
                new ApplicationMenuItem(
                    DFAppMenus.Media.Login
                    , l["Menu:TG:Media:Login"]
                    , "~/TG/Login"
                    , icon: "fas fa-door-open"
                    , order: 4));

            context.Menu.AddItem(mediaFirst);
        }

        if (await context.IsGrantedAsync(DFAppPermissions.DynamicIP.Default))
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    DFAppMenus.DynamicIP,
                    l["Menu:DynamicIP"],
                    "~/DynamicIP",
                    icon: "fas fa-map",
                    order: 2
                )
            );
        }

        if (await context.IsGrantedAsync(DFAppPermissions.Lottery.Default))
        {

            var lotteryFirst = new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:Lottery"], "~/Lottery", icon: "fas fa-baseball-ball", order: 3);

            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotteryBuy"], "~/Lottery", icon: "fas fa-baseball-ball", order: 1));
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotteryResult"], "~/Lottery/Result", icon: "fas fa-baseball-ball", order: 2));
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotteryStatistics"], "~/Lottery/Statistics", icon: "fas fa-baseball-ball", order: 3));
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotteryStatisticsItem"], "~/Lottery/StatisticsItem", icon: "fas fa-baseball-ball", order: 4));
            // 添加彩票模拟子菜单
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.LotterySimulation, l["Menu:LotterySimulation"], "~/Lottery/Simulation", icon: "fas fa-random", order: 5));
           
            context.Menu.AddItem(lotteryFirst);

        }

        if (await context.IsGrantedAsync(DFAppPermissions.LogSink.Default))
        {
            var logSinkFirst = new ApplicationMenuItem(
                DFAppMenus.LogSink
                , l["Menu:LogSink"]
                , "~/LogSink", icon: "fas fa-book"
                , order: 4);

            logSinkFirst.AddItem(
                new ApplicationMenuItem(
                    DFAppMenus.LogSink
                    , l["Menu:SignalRSink"]
                    , "~/LogSink/SignalRSink"
                    , icon: "fas fa-book"
                    , order: 1));

            logSinkFirst.AddItem(
                new ApplicationMenuItem(
                    DFAppMenus.LogSink
                    , l["Menu:QueueSink"]
                    , "~/LogSink/QueueSink"
                    , icon: "fas fa-book"
                    , order: 2));

            context.Menu.AddItem(logSinkFirst);

        }

        if (await context.IsGrantedAsync(DFAppPermissions.FileUploadDownload.Default))
        {

            var fileUploadDownloadFirst = new ApplicationMenuItem(
                DFAppMenus.FileUploadDownload
                , l["Menu:FileUploadDownload"]
                , "~/FileUploadDownload"
                , icon: "fas fa-file"
                , order: 5);

            if (await context.IsGrantedAsync(DFAppPermissions.FileUploadDownload.Upload))
            {
                fileUploadDownloadFirst.AddItem(new ApplicationMenuItem(
                    DFAppMenus.Bookkeeping
                    , l["Menu:FileUploadDownload:Upload"]
                    , "~/FileUploadDownload/Upload"
                    , icon: "fas fa-file"
                    , order: 1));
            }

            if (await context.IsGrantedAsync(DFAppPermissions.FileUploadDownload.Download))
            {
                fileUploadDownloadFirst.AddItem(new ApplicationMenuItem(
                    DFAppMenus.Bookkeeping
                    , l["Menu:FileUploadDownload:Download"]
                    , "~/FileUploadDownload/FileList"
                    , icon: "fas fa-file"
                    , order: 2));
            }

            context.Menu.AddItem(fileUploadDownloadFirst);
        }

        if (await context.IsGrantedAsync(DFAppPermissions.Bookkeeping.Default))
        {

            var bookkeepingFirst = new ApplicationMenuItem(
                DFAppMenus.Bookkeeping
                , l["Menu:Bookkeeping"]
                , "~/Bookkeeping"
                , icon: "fas fa-receipt"
                , order: 6);

            if (await context.IsGrantedAsync(DFAppPermissions.BookkeepingCategory.Default))
            {
                bookkeepingFirst.AddItem(new ApplicationMenuItem(
                    DFAppMenus.Bookkeeping
                    , l["Menu:BookkeepingCategory"]
                    , "~/Bookkeeping/Category"
                    , icon: "fas fa-receipt"
                    , order: 1));
            }

            if (await context.IsGrantedAsync(DFAppPermissions.BookkeepingExpenditure.Default))
            {
                bookkeepingFirst.AddItem(new ApplicationMenuItem(
                    DFAppMenus.Bookkeeping
                    , l["Menu:BookkeepingExpenditure"]
                    , "~/Bookkeeping/Expenditure"
                    , icon: "fas fa-receipt"
                    , order: 2));
            }

            if (await context.IsGrantedAsync(DFAppPermissions.BookkeepingExpenditure.Analysis))
            {
                bookkeepingFirst.AddItem(new ApplicationMenuItem(
                    DFAppMenus.Bookkeeping
                    , l["Menu:BookkeepingExpenditure:Analysis"]
                    , "~/Bookkeeping/Expenditure/Analysis"
                    , icon: "fas fa-receipt"
                    , order: 3));
            }

            context.Menu.AddItem(bookkeepingFirst);

        }

        if (await context.IsGrantedAsync(DFAppPermissions.ConfigurationInfo.Default))
        {

            var configurationInfoFirst = new ApplicationMenuItem(
                DFAppMenus.ConfigurationInfo.Default
                , l["Menu:ConfigurationInfo"]
                , "~/Configuration"
                , icon: "fas fa-hammer"
                , order: 7);

            context.Menu.AddItem(configurationInfoFirst);

        }

        if (await context.IsGrantedAsync(DFAppPermissions.ManagementBackground.Default))
        {

            var configurationInfoFirst = new ApplicationMenuItem(
                DFAppMenus.ManagementBackground.Default
                , l["Menu:ManagementBackground"]
                , "~/Management/Background"
                , icon: "fas fa-toolbox"
                , order: 8);

            context.Menu.AddItem(configurationInfoFirst);

        }

        if(await context.IsGrantedAsync(DFAppPermissions.Aria2.Default))
        {
            var aria2First = new ApplicationMenuItem(
                DFAppMenus.Aria2.Default
                , l["Menu:Aria2"]
                ,"~/Aria2"
                , icon: "fas fa-download"
                , order: 9);
            context.Menu.AddItem(aria2First);
        }

        administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        return;
    }
}
