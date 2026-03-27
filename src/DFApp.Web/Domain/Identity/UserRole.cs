using System;
using SqlSugar;

namespace DFApp.Identity;

/// <summary>
/// 用户角色关联实体
/// </summary>
[SugarTable("AbpUserRoles")]
public class UserRole
{
    /// <summary>
    /// 用户 ID（主键）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, ColumnName = "UserId")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色 ID（主键）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, ColumnName = "RoleId")]
    public Guid RoleId { get; set; }

    /// <summary>
    /// 租户 ID
    /// </summary>
    [SugarColumn(ColumnName = "TenantId")]
    public Guid? TenantId { get; set; }
}
