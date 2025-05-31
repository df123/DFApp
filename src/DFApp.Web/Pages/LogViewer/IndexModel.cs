using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DFApp.Permissions;

namespace DFApp.Web.Pages.LogViewer
{
    [Authorize(DFAppPermissions.LogViewer.Default)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            // 页面初始化逻辑已移至客户端
        }
    }
}
