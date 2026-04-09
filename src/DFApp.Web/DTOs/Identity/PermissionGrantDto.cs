using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Identity;

/// <summary>
/// 权限授予信息 DTO
/// </summary>
public class PermissionGrantDto : EntityDto<long>
{
    /// <summary>
    /// 权限名称
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标类型（Role 或 User）
    /// </summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标标识（角色名称或用户 ID）
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;
}

/// <summary>
/// 获取权限授予列表请求 DTO
/// </summary>
public class GetPermissionGrantListDto
{
    /// <summary>
    /// 授予目标类型（Role 或 User）
    /// </summary>
    [Required(ErrorMessage = "提供者类型不能为空")]
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标标识（角色名称或用户 ID）
    /// </summary>
    [Required(ErrorMessage = "提供者标识不能为空")]
    public string ProviderKey { get; set; } = string.Empty;
}

/// <summary>
/// 更新权限请求 DTO
/// </summary>
public class UpdatePermissionsDto
{
    /// <summary>
    /// 授予目标类型（Role 或 User）
    /// </summary>
    [Required(ErrorMessage = "提供者类型不能为空")]
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标标识（角色名称或用户 ID）
    /// </summary>
    [Required(ErrorMessage = "提供者标识不能为空")]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// 要设置的权限名称列表
    /// </summary>
    public List<string> PermissionNames { get; set; } = new();
}

/// <summary>
/// 授予权限请求 DTO
/// </summary>
public class GrantPermissionsDto
{
    /// <summary>
    /// 授予目标类型（Role 或 User）
    /// </summary>
    [Required(ErrorMessage = "提供者类型不能为空")]
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标标识（角色名称或用户 ID）
    /// </summary>
    [Required(ErrorMessage = "提供者标识不能为空")]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// 要授予的权限名称列表
    /// </summary>
    public List<string> PermissionNames { get; set; } = new();
}

/// <summary>
/// 撤销权限请求 DTO
/// </summary>
public class RevokePermissionsDto
{
    /// <summary>
    /// 授予目标类型（Role 或 User）
    /// </summary>
    [Required(ErrorMessage = "提供者类型不能为空")]
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标标识（角色名称或用户 ID）
    /// </summary>
    [Required(ErrorMessage = "提供者标识不能为空")]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// 要撤销的权限名称列表
    /// </summary>
    public List<string> PermissionNames { get; set; } = new();
}

/// <summary>
/// 权限定义 DTO（用于展示权限树结构）
/// </summary>
public class PermissionDefinitionDto
{
    /// <summary>
    /// 权限所属分组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 权限名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父级权限名称
    /// </summary>
    public string? ParentName { get; set; }

    /// <summary>
    /// 权限显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
