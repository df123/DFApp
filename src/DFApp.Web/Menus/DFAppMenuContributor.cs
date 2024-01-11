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
            var mediaFirst = new ApplicationMenuItem(DFAppMenus.Media, l["Menu:Media"], "~/Media", icon: "fas fa-play", order: 1);

            mediaFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Media, l["Menu:Media"], "~/Media", icon: "fas fa-play", order: 1));
            mediaFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Media, l["Menu:Media:ExternalLink"], "~/Media/ExternalLink", icon: "fas fa-link", order: 2));
            mediaFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Media, l["Menu:Media:Chart"], "~/Media/Chart", icon: "fas fa-chart-bar", order: 3));

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

            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:Lottery"], "~/Lottery", icon: "fas fa-baseball-ball", order: 1));
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotteryStatistics"], "~/Lottery/Statistics", icon: "fas fa-baseball-ball", order: 2));
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotteryStatisticsItem"], "~/Lottery/StatisticsItem", icon: "fas fa-baseball-ball", order: 3));
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotterySpecifyPeriod"], "~/Lottery/SpecifyPeriod", icon: "fas fa-baseball-ball", order: 3));
            lotteryFirst.AddItem(new ApplicationMenuItem(DFAppMenus.Lottery, l["Menu:LotteryResult"], "~/Lottery/Result", icon: "fas fa-baseball-ball", order: 4));

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

            context.Menu.AddItem(bookkeepingFirst);

        }



        administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);


        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        return;
    }
}
