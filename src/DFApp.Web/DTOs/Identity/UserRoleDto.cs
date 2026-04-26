using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DFApp.Web.DTOs.Identity;

/// <summary>
/// 用户角色关联信息 DTO
/// </summary>
public class UserRoleDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}

/// <summary>
/// 获取用户角色列表请求 DTO
/// </summary>
public class GetUserRoleListDto
{
    /// <summary>
    /// 按用户ID筛选
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 按角色ID筛选
    /// </summary>
    public Guid? RoleId { get; set; }
}

/// <summary>
/// 分配用户角色请求 DTO
/// </summary>
public class AssignUserRolesDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 要分配的角色ID列表
    /// </summary>
    [Required(ErrorMessage = "角色列表不能为空")]
    public List<Guid> RoleIds { get; set; } = new();
}

/// <summary>
/// 移除用户角色请求 DTO
/// </summary>
public class RemoveUserRolesDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 要移除的角色ID列表
    /// </summary>
    [Required(ErrorMessage = "角色列表不能为空")]
    public List<Guid> RoleIds { get; set; } = new();
}
