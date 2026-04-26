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

/// <summary>
/// 权限授予来源信息
/// </summary>
public class PermissionGrantedInfoDto
{
    /// <summary>
    /// 授予者类型（Role 或 User）
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// 授予者标识（角色名称或用户 ID）
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;
}

/// <summary>
/// 带授予状态的权限定义
/// </summary>
public class PermissionWithGrantDto
{
    /// <summary>
    /// 权限名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 权限显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 父级权限名称
    /// </summary>
    public string? ParentName { get; set; }

    /// <summary>
    /// 是否已授予
    /// </summary>
    public bool IsGranted { get; set; }

    /// <summary>
    /// 允许的提供者列表（保留字段，前端兼容用）
    /// </summary>
    public List<object> AllowedProviders { get; set; } = new();

    /// <summary>
    /// 授予来源列表
    /// </summary>
    public List<PermissionGrantedInfoDto> GrantedProviders { get; set; } = new();
}

/// <summary>
/// 权限分组结果
/// </summary>
public class PermissionGroupResultDto
{
    /// <summary>
    /// 分组名称（权限前缀，如 DFApp.UserManagement）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分组显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称键（保留字段，前端兼容用）
    /// </summary>
    public string DisplayNameKey { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称资源（保留字段，前端兼容用）
    /// </summary>
    public string DisplayNameResource { get; set; } = string.Empty;

    /// <summary>
    /// 分组内权限列表
    /// </summary>
    public List<PermissionWithGrantDto> Permissions { get; set; } = new();
}

/// <summary>
/// 权限树结果（前端期望的完整响应结构）
/// </summary>
public class PermissionTreeResultDto
{
    /// <summary>
    /// 实体显示名称
    /// </summary>
    public string EntityDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 权限分组列表
    /// </summary>
    public List<PermissionGroupResultDto> Groups { get; set; } = new();
}
