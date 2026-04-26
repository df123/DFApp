using System;
using Microsoft.AspNetCore.Authorization;

namespace DFApp.Web.Permissions;

/// <summary>
/// 权限需求，用于授权系统
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// 权限名称
    /// </summary>
    public string PermissionName { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="permissionName">权限名称</param>
    public PermissionRequirement(string permissionName)
    {
        if (string.IsNullOrEmpty(permissionName))
        {
            throw new ArgumentException("权限名称不能为空", nameof(permissionName));
        }

        PermissionName = permissionName;
    }
}
