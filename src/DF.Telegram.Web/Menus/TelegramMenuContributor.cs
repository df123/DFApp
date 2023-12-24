using System.Threading.Tasks;
using DF.Telegram.Localization;
using DF.Telegram.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace DF.Telegram.Web.Menus;

public class TelegramMenuContributor : IMenuContributor
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
        var l = context.GetLocalizer<TelegramResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                TelegramMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fas fa-home",
                order: 0
            )
        );

        if (await context.IsGrantedAsync(TelegramPermissions.Medias.Default))
        {
            var mediaFirst = new ApplicationMenuItem(TelegramMenus.Home, l["Menu:Media"], "~/Media", icon: "fas fa-play", order: 1);

            mediaFirst.AddItem(new ApplicationMenuItem(TelegramMenus.Home, l["Menu:Media"], "~/Media", icon: "fas fa-play", order: 1));
            mediaFirst.AddItem(new ApplicationMenuItem(TelegramMenus.Home, l["Menu:Media:ExternalLink"], "~/Media/ExternalLink", icon: "fas fa-link", order: 2));
            mediaFirst.AddItem(new ApplicationMenuItem(TelegramMenus.Home, l["Menu:Media:Chart"], "~/Media/Chart", icon: "fas fa-chart-bar", order: 3));

            context.Menu.Items.Insert(1, mediaFirst);
        }

        if (await context.IsGrantedAsync(TelegramPermissions.DynamicIP.Default))
        {
            context.Menu.Items.Insert(
                1,
                new ApplicationMenuItem(
                    TelegramMenus.Home,
                    l["Menu:DynamicIP"],
                    "~/DynamicIP",
                    icon: "fas fa-map",
                    order: 2
                )
            );
        }

        if (await context.IsGrantedAsync(TelegramPermissions.Lottery.Default))
        {

            var lotteryFirst = new ApplicationMenuItem(TelegramMenus.Home, l["Menu:Lottery"], "~/Lottery", icon: "fas fa-baseball-ball", order: 3);

            lotteryFirst.AddItem(new ApplicationMenuItem(TelegramMenus.Home, l["Menu:Lottery"], "~/Lottery", icon: "fas fa-baseball-ball", order: 1));
            lotteryFirst.AddItem(new ApplicationMenuItem(TelegramMenus.Home, l["Menu:LotteryStatistics"], "~/Lottery/Statistics", icon: "fas fa-baseball-ball", order: 2));
            lotteryFirst.AddItem(new ApplicationMenuItem(TelegramMenus.Home, l["Menu:LotteryStatisticsItem"], "~/Lottery/StatisticsItem", icon: "fas fa-baseball-ball", order: 3));
            lotteryFirst.AddItem(new ApplicationMenuItem(TelegramMenus.Home, l["Menu:LotterySpecifyPeriod"], "~/Lottery/SpecifyPeriod", icon: "fas fa-baseball-ball", order: 3));

            context.Menu.Items.Insert(1, lotteryFirst);

        }


        administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);


        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        return;
    }
}
