using System.Threading.Tasks;

namespace DFApp.Web.Permissions;

/// <summary>
/// 权限检查接口
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// 检查当前用户是否拥有指定权限
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    /// <returns>如果拥有权限返回 true，否则返回 false</returns>
    Task<bool> IsGrantedAsync(string permissionName);
}
