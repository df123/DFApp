using System.Drawing;
using System.Threading.Tasks;
using DFApp.Localization;
using DFApp.Permissions;
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

        administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 2);

        return;
    }
}
