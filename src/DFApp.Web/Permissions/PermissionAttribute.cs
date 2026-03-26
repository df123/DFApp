using System;

namespace DFApp.Web.Permissions;

/// <summary>
/// 权限特性，用于标记控制器或操作需要特定权限
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAttribute : Attribute
{
    /// <summary>
    /// 权限名称
    /// </summary>
    public string PermissionName { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    public PermissionAttribute(string permissionName)
    {
        if (string.IsNullOrEmpty(permissionName))
        {
            throw new ArgumentException("权限名称不能为空", nameof(permissionName));
        }

        PermissionName = permissionName;
    }
}
