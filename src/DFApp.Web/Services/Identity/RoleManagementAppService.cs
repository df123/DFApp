using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Identity;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.DTOs;
using DFApp.Web.DTOs.Identity;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Identity;

/// <summary>
/// 角色管理应用服务
/// </summary>
public class RoleManagementAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<Role, Guid> _roleRepository;
    private readonly ISqlSugarRepository<UserRole, Guid> _userRoleRepository;
    private readonly ISqlSugarRepository<AppPermissionGrant, long> _appPermissionGrantRepository;
    private readonly ILogger<RoleManagementAppService> _logger;

    public RoleManagementAppService(
        ISqlSugarRepository<Role, Guid> roleRepository,
        ISqlSugarRepository<UserRole, Guid> userRoleRepository,
        ISqlSugarRepository<AppPermissionGrant, long> appPermissionGrantRepository,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ILogger<RoleManagementAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _appPermissionGrantRepository = appPermissionGrantRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取角色分页列表，支持按名称搜索
    /// </summary>
    public async Task<PagedResultDto<RoleDto>> GetListAsync(GetRoleListDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.RoleManagement.Default);

        var query = _roleRepository.GetQueryable();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(r => r.Name.Contains(input.Filter));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Name)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        var dtos = items.Select(MapToRoleDto).ToList();

        return new PagedResultDto<RoleDto>(total, dtos);
    }

    /// <summary>
    /// 获取所有角色列表（不分页）
    /// </summary>
    public async Task<List<RoleDto>> GetAllListAsync()
    {
        await CheckPermissionAsync(DFAppPermissions.RoleManagement.Default);

        var roles = await _roleRepository.GetQueryable()
            .OrderBy(r => r.Name)
            .ToListAsync();

        return roles.Select(MapToRoleDto).ToList();
    }

    /// <summary>
    /// 获取单个角色详情
    /// </summary>
    public async Task<RoleDto> GetAsync(Guid id)
    {
        await CheckPermissionAsync(DFAppPermissions.RoleManagement.Default);

        var role = await _roleRepository.GetQueryable()
            .Where("UPPER(Id) = @Id", new { Id = id.ToString().ToUpperInvariant() })
            .FirstAsync();

        EnsureEntityExists(role, $"角色不存在，ID：{id}");

        return MapToRoleDto(role);
    }

    /// <summary>
    /// 创建角色，自动生成 NormalizedName 并检查名称唯一性
    /// </summary>
    public async Task<RoleDto> CreateAsync(CreateRoleDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.RoleManagement.Create);

        var normalizedName = input.Name.ToUpperInvariant();

        // 检查角色名称唯一性
        var existingRole = await _roleRepository.GetFirstOrDefaultAsync(r => r.NormalizedName == normalizedName);
        if (existingRole != null)
        {
            throw new BusinessException($"角色名称已存在：{input.Name}");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            NormalizedName = normalizedName,
            IsDefault = input.IsDefault,
            IsStatic = input.IsStatic,
            IsPublic = input.IsPublic,
            ExtraProperties = "{}"
        };

        await _roleRepository.InsertAsync(role);
        _logger.LogInformation("创建角色：{RoleName}，ID：{RoleId}", role.Name, role.Id);

        return MapToRoleDto(role);
    }

    /// <summary>
    /// 更新角色，检查名称唯一性（排除自身）
    /// </summary>
    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.RoleManagement.Update);

        var role = await _roleRepository.GetQueryable()
            .Where("UPPER(Id) = @Id", new { Id = id.ToString().ToUpperInvariant() })
            .FirstAsync();

        EnsureEntityExists(role, $"角色不存在，ID：{id}");

        var oldName = role.Name;
        var normalizedName = input.Name.ToUpperInvariant();

        // 检查名称唯一性（排除自身）
        var existingRole = await _roleRepository.GetFirstOrDefaultAsync(r => r.NormalizedName == normalizedName);
        if (existingRole != null && existingRole.Id != role.Id)
        {
            throw new BusinessException($"角色名称已存在：{input.Name}");
        }

        // 如果角色名称变更，同步更新 AppPermissionGrants 中的 ProviderKey
        if (oldName != input.Name)
        {
            var oldPermissions = await _appPermissionGrantRepository.GetQueryable()
                .Where(pg => pg.ProviderType == "Role" && pg.ProviderKey == oldName)
                .ToListAsync();

            foreach (var perm in oldPermissions)
            {
                perm.ProviderKey = input.Name;
            }

            if (oldPermissions.Count > 0)
            {
                await _appPermissionGrantRepository.UpdateAsync(oldPermissions);
                _logger.LogInformation("角色重命名：同步更新 {Count} 条权限记录的 ProviderKey", oldPermissions.Count);
            }
        }

        role.Name = input.Name;
        role.NormalizedName = normalizedName;
        role.IsDefault = input.IsDefault;
        role.IsStatic = input.IsStatic;
        role.IsPublic = input.IsPublic;

        await _roleRepository.UpdateAsync(role);
        _logger.LogInformation("更新角色：{RoleName}，ID：{RoleId}", role.Name, role.Id);

        return MapToRoleDto(role);
    }

    /// <summary>
    /// 删除角色，级联删除 UserRoles 和 AppPermissionGrants，IsStatic 角色不可删除
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await CheckPermissionAsync(DFAppPermissions.RoleManagement.Delete);

        var role = await _roleRepository.GetQueryable()
            .Where("UPPER(Id) = @Id", new { Id = id.ToString().ToUpperInvariant() })
            .FirstAsync();

        EnsureEntityExists(role, $"角色不存在，ID：{id}");

        if (role.IsStatic)
        {
            throw new BusinessException($"静态角色不可删除：{role.Name}");
        }

        var roleIdUpper = role.Id.ToString().ToUpperInvariant();

        // 级联删除用户角色关联
        var userRoles = await _userRoleRepository.GetQueryable()
            .Where("UPPER(RoleId) = @RoleId", new { RoleId = roleIdUpper })
            .ToListAsync();
        if (userRoles.Count > 0)
        {
            await _userRoleRepository.DeleteAsync(userRoles);
        }

        // 级联删除角色级别的权限授予（ProviderType="Role", ProviderKey=角色名称）
        var permissionGrants = await _appPermissionGrantRepository.GetQueryable()
            .Where(pg => pg.ProviderType == "Role" && pg.ProviderKey == role.Name)
            .ToListAsync();
        if (permissionGrants.Count > 0)
        {
            await _appPermissionGrantRepository.DeleteAsync(permissionGrants);
        }

        await _roleRepository.DeleteAsync(role);
        _logger.LogInformation("删除角色：{RoleName}，ID：{RoleId}，级联删除 {UserRoleCount} 条用户角色关联，{PermissionCount} 条权限授予",
            role.Name, role.Id, userRoles.Count, permissionGrants.Count);
    }

    private static RoleDto MapToRoleDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName,
            IsDefault = role.IsDefault,
            IsStatic = role.IsStatic,
            IsPublic = role.IsPublic,
            CreationTime = role.CreationTime
        };
    }
}
