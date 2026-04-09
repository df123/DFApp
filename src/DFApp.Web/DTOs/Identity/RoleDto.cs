using System;
using System.ComponentModel.DataAnnotations;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.Identity;

/// <summary>
/// 角色信息 DTO
/// </summary>
public class RoleDto : EntityDto<Guid>
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 标准化后的角色名称
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认角色
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否为静态角色（不可删除）
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 是否为公开角色
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 创建角色请求 DTO
/// </summary>
public class CreateRoleDto
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Required(ErrorMessage = "角色名称不能为空")]
    [StringLength(256, ErrorMessage = "角色名称长度不能超过256个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认角色
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否为静态角色
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 是否为公开角色
    /// </summary>
    public bool IsPublic { get; set; }
}

/// <summary>
/// 更新角色请求 DTO
/// </summary>
public class UpdateRoleDto
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Required(ErrorMessage = "角色名称不能为空")]
    [StringLength(256, ErrorMessage = "角色名称长度不能超过256个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认角色
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否为静态角色
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 是否为公开角色
    /// </summary>
    public bool IsPublic { get; set; }
}

/// <summary>
/// 获取角色列表请求 DTO
/// </summary>
public class GetRoleListDto
{
    /// <summary>
    /// 跳过数量
    /// </summary>
    public int SkipCount { get; set; } = 0;

    /// <summary>
    /// 每页数量
    /// </summary>
    public int MaxResultCount { get; set; } = 10;

    /// <summary>
    /// 过滤关键字
    /// </summary>
    public string? Filter { get; set; }
}
