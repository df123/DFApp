using System;
using SqlSugar;

namespace DFApp.Web.Domain;

/// <summary>
/// 应用权限授予实体，替代旧的 AbpPermissionGrants 表
/// 使用 long 自增主键避免 GUID 大小写问题
/// </summary>
[SugarTable("AppPermissionGrants")]
public class AppPermissionGrant
{
    /// <summary>
    /// 主键（自增）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 权限名称，如 DFApp.RoleManagement
    /// </summary>
    [SugarColumn(Length = 200)]
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标类型：Role 或 User
    /// </summary>
    [SugarColumn(Length = 20)]
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// 授予目标标识：角色名称（如 "admin"）或用户 ID 字符串（小写）
    /// </summary>
    [SugarColumn(Length = 100)]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
