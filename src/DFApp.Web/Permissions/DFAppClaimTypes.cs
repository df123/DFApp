namespace DFApp.Web.Permissions;

/// <summary>
/// JWT Claim 类型常量，统一管理所有自定义 Claim 的类型名称
/// </summary>
public static class DFAppClaimTypes
{
    /// <summary>
    /// 权限 Claim 类型
    /// </summary>
    public const string Permission = "Permission";

    /// <summary>
    /// 角色 Claim 类型
    /// </summary>
    public const string Role = "role";
}
