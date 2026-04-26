using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Account;
using DFApp.Identity;
using DFApp.Web.Data;
using DFApp.Web.DTOs.Identity;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;

namespace DFApp.Web.Services.Identity;

/// <summary>
/// 用户角色管理应用服务
/// </summary>
public class UserRoleManagementAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<UserRole, Guid> _userRoleRepository;
    private readonly ISqlSugarReadOnlyRepository<Role, Guid> _roleReadOnlyRepository;
    private readonly ISqlSugarReadOnlyRepository<User, Guid> _userReadOnlyRepository;
    private readonly ILogger<UserRoleManagementAppService> _logger;

    public UserRoleManagementAppService(
        ISqlSugarRepository<UserRole, Guid> userRoleRepository,
        ISqlSugarReadOnlyRepository<Role, Guid> roleReadOnlyRepository,
        ISqlSugarReadOnlyRepository<User, Guid> userReadOnlyRepository,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ILogger<UserRoleManagementAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _userRoleRepository = userRoleRepository;
        _roleReadOnlyRepository = roleReadOnlyRepository;
        _userReadOnlyRepository = userReadOnlyRepository;
        _logger = logger;
    }

    /// <summary>
    /// 获取用户的角色列表，返回 UserRoleDto（包含角色名称）
    /// </summary>
    public async Task<List<UserRoleDto>> GetUserRolesAsync(Guid userId)
    {
        await CheckPermissionAsync(DFAppPermissions.UserRoleManagement.Default);

        var userIdUpper = userId.ToString().ToUpperInvariant();

        // 查询用户角色关联（UserId 比较用 UPPER）
        var userRoles = await _userRoleRepository.GetQueryable()
            .Where("UPPER(UserId) = @UserId", new { UserId = userIdUpper })
            .ToListAsync();

        if (userRoles.Count == 0)
        {
            return new List<UserRoleDto>();
        }

        // 查询所有角色（数量少，全量加载后在内存中匹配）
        var allRoles = await _roleReadOnlyRepository.GetQueryable()
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        var user = await _userReadOnlyRepository.GetQueryable()
            .Where("UPPER(Id) = @Id", new { Id = userIdUpper })
            .Select(u => new { u.Id, u.UserName })
            .FirstAsync();

        var result = new List<UserRoleDto>();
        foreach (var ur in userRoles)
        {
            var role = allRoles.FirstOrDefault(r => r.Id == ur.RoleId);
            result.Add(new UserRoleDto
            {
                UserId = ur.UserId,
                UserName = user?.UserName ?? string.Empty,
                RoleId = ur.RoleId,
                RoleName = role?.Name ?? string.Empty
            });
        }

        return result;
    }

    /// <summary>
    /// 获取角色下的用户列表
    /// </summary>
    public async Task<List<UserRoleDto>> GetUsersByRoleAsync(Guid roleId)
    {
        await CheckPermissionAsync(DFAppPermissions.UserRoleManagement.Default);

        var roleIdUpper = roleId.ToString().ToUpperInvariant();

        // 查询角色关联的用户（RoleId 比较用 UPPER）
        var userRoles = await _userRoleRepository.GetQueryable()
            .Where("UPPER(RoleId) = @RoleId", new { RoleId = roleIdUpper })
            .ToListAsync();

        if (userRoles.Count == 0)
        {
            return new List<UserRoleDto>();
        }

        // 查询角色名称
        var role = await _roleReadOnlyRepository.GetQueryable()
            .Where("UPPER(Id) = @Id", new { Id = roleIdUpper })
            .Select(r => new { r.Id, r.Name })
            .FirstAsync();

        // 查询所有用户（全量加载后在内存中匹配）
        var userIds = userRoles.Select(ur => ur.UserId).ToList();
        var allUsers = await _userReadOnlyRepository.GetQueryable()
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        var result = new List<UserRoleDto>();
        foreach (var ur in userRoles)
        {
            var user = allUsers.FirstOrDefault(u => u.Id == ur.UserId);
            result.Add(new UserRoleDto
            {
                UserId = ur.UserId,
                UserName = user?.UserName ?? string.Empty,
                RoleId = ur.RoleId,
                RoleName = role?.Name ?? string.Empty
            });
        }

        return result;
    }

    /// <summary>
    /// 批量分配角色，跳过已存在的
    /// </summary>
    public async Task AssignAsync(AssignUserRolesDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.UserRoleManagement.Assign);

        var userIdUpper = input.UserId.ToString().ToUpperInvariant();

        // 查询用户已有的角色
        var existingRoles = await _userRoleRepository.GetQueryable()
            .Where("UPPER(UserId) = @UserId", new { UserId = userIdUpper })
            .ToListAsync();

        var existingRoleIds = existingRoles.Select(ur => ur.RoleId).ToHashSet();
        var assignedCount = 0;

        foreach (var roleId in input.RoleIds)
        {
            if (existingRoleIds.Contains(roleId))
            {
                continue;
            }

            var userRole = new UserRole
            {
                UserId = input.UserId,
                RoleId = roleId
            };
            await _userRoleRepository.InsertAsync(userRole);
            assignedCount++;
        }

        _logger.LogInformation("批量分配角色：UserId={UserId}, 新增 {AssignedCount} 个角色",
            input.UserId, assignedCount);
    }

    /// <summary>
    /// 批量移除角色
    /// </summary>
    public async Task RemoveAsync(RemoveUserRolesDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.UserRoleManagement.Remove);

        var userIdUpper = input.UserId.ToString().ToUpperInvariant();

        // 查询用户的所有角色关联
        var existingRoles = await _userRoleRepository.GetQueryable()
            .Where("UPPER(UserId) = @UserId", new { UserId = userIdUpper })
            .ToListAsync();

        var roleIdsToRemove = input.RoleIds.ToHashSet();
        var toDelete = existingRoles.Where(ur => roleIdsToRemove.Contains(ur.RoleId)).ToList();

        if (toDelete.Count > 0)
        {
            await _userRoleRepository.DeleteAsync(toDelete);
        }

        _logger.LogInformation("批量移除角色：UserId={UserId}, 移除 {RemovedCount} 个角色",
            input.UserId, toDelete.Count);
    }
}
