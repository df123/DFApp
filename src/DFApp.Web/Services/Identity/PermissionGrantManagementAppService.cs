using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DFApp.Web.Data;
using DFApp.Web.Domain;
using DFApp.Web.DTOs.Identity;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DFApp.Web.Services.Identity;

/// <summary>
/// 权限授予管理应用服务，使用新的 AppPermissionGrant 实体
/// </summary>
public class PermissionGrantManagementAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<AppPermissionGrant, long> _appPermissionGrantRepository;
    private readonly ISqlSugarClient _db;
    private readonly ILogger<PermissionGrantManagementAppService> _logger;

    public PermissionGrantManagementAppService(
        ISqlSugarRepository<AppPermissionGrant, long> appPermissionGrantRepository,
        ISqlSugarClient db,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ILogger<PermissionGrantManagementAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _appPermissionGrantRepository = appPermissionGrantRepository;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 获取带分组树结构的权限列表（前端期望的格式）
    /// </summary>
    public async Task<PermissionTreeResultDto> GetListAsync(GetPermissionGrantListDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.PermissionGrantManagement.Default);

        // 1. 通过反射读取 DFAppPermissions 中的权限定义
        var permissionGroups = typeof(DFAppPermissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        // 2. 构建权限树结构（按分组收集所有权限）
        var groupResults = new List<PermissionGroupResultDto>();
        // 记录所有已定义的权限名称，用于查询授予状态
        var allPermissionNames = new HashSet<string>();

        foreach (var group in permissionGroups)
        {
            var groupDto = new PermissionGroupResultDto
            {
                Name = group.Name,
                DisplayName = group.Name
            };

            var fields = group.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string) && f.IsLiteral)
                .ToList();

            foreach (var field in fields)
            {
                var permissionName = (string)field.GetRawConstantValue()!;
                allPermissionNames.Add(permissionName);
                groupDto.Permissions.Add(new PermissionWithGrantDto
                {
                    Name = permissionName,
                    DisplayName = field.Name,
                    ParentName = null,
                    IsGranted = false
                });
            }

            // 推断父子关系：如果权限 B 的值以权限 A 的值 + "." 为前缀，则 A 是 B 的父权限
            foreach (var permission in groupDto.Permissions)
            {
                // 找到最直接的父权限（最长的匹配前缀）
                var parent = groupDto.Permissions
                    .Where(p => p.Name != permission.Name && permission.Name.StartsWith(p.Name + "."))
                    .OrderByDescending(p => p.Name.Length)
                    .FirstOrDefault();

                permission.ParentName = parent?.Name;
            }

            if (groupDto.Permissions.Count > 0)
            {
                groupResults.Add(groupDto);
            }
        }

        // 3. 查询当前 Provider 的直接授予权限
        var directGrants = await _appPermissionGrantRepository.GetQueryable()
            .Where(pg => pg.ProviderType == input.ProviderType && pg.ProviderKey == input.ProviderKey)
            .ToListAsync();

        var directGrantedNames = directGrants.Select(g => g.PermissionName).ToHashSet();

        // 4. 查询该 Provider 关联的所有授予来源（用于显示"通过角色授予"等提示）
        var allGrantedProviders = new Dictionary<string, List<PermissionGrantedInfoDto>>();

        if (input.ProviderType == "User")
        {
            // 查询用户直接授予的权限
            foreach (var grant in directGrants)
            {
                AddGrantedProvider(allGrantedProviders, grant.PermissionName, "User", input.ProviderKey);
            }

            // 查询用户所属角色，以及通过角色授予的权限
            if (Guid.TryParse(input.ProviderKey, out var userId))
            {
                var roleIds = (await _db.Queryable<DFApp.Identity.UserRole>()
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync())
                    .ToHashSet();

                if (roleIds.Count > 0)
                {
                    var roles = await _db.Queryable<DFApp.Identity.Role>()
                        .Where(r => roleIds.Contains(r.Id))
                        .ToListAsync();

                    var roleNames = roles.Select(r => r.Name).ToList();

                    // 查询这些角色授予的所有权限
                    var roleGrants = await _appPermissionGrantRepository.GetQueryable()
                        .Where(pg => pg.ProviderType == "Role" && roleNames.Contains(pg.ProviderKey))
                        .ToListAsync();

                    foreach (var grant in roleGrants)
                    {
                        AddGrantedProvider(allGrantedProviders, grant.PermissionName, "Role", grant.ProviderKey);
                    }
                }
            }
        }
        else if (input.ProviderType == "Role")
        {
            // 角色的授予来源只有自身
            foreach (var grant in directGrants)
            {
                AddGrantedProvider(allGrantedProviders, grant.PermissionName, "Role", input.ProviderKey);
            }
        }

        // 5. 合并授予状态到权限树
        foreach (var group in groupResults)
        {
            foreach (var permission in group.Permissions)
            {
                // 判断是否已授予（任一来源授予即为已授予）
                if (allGrantedProviders.TryGetValue(permission.Name, out var providers))
                {
                    permission.IsGranted = true;
                    permission.GrantedProviders = providers;
                }

                // 如果父权限被授予，子权限也视为已授予
                if (permission.ParentName != null && !permission.IsGranted)
                {
                    if (directGrantedNames.Contains(permission.ParentName) ||
                        allGrantedProviders.ContainsKey(permission.ParentName))
                    {
                        permission.IsGranted = true;
                    }
                }
            }
        }

        // 6. 构建返回结果
        var providerTypeDisplay = input.ProviderType == "Role" ? "角色" : "用户";
        return new PermissionTreeResultDto
        {
            EntityDisplayName = $"{providerTypeDisplay}: {input.ProviderKey}",
            Groups = groupResults
        };
    }

    private static void AddGrantedProvider(
        Dictionary<string, List<PermissionGrantedInfoDto>> dict,
        string permissionName,
        string providerName,
        string providerKey)
    {
        if (!dict.TryGetValue(permissionName, out var list))
        {
            list = new List<PermissionGrantedInfoDto>();
            dict[permissionName] = list;
        }

        // 避免重复添加
        if (!list.Any(p => p.ProviderName == providerName && p.ProviderKey == providerKey))
        {
            list.Add(new PermissionGrantedInfoDto
            {
                ProviderName = providerName,
                ProviderKey = providerKey
            });
        }
    }

    /// <summary>
    /// 获取所有权限定义（通过反射从 DFAppPermissions 静态类读取，不需要查数据库）
    /// </summary>
    public async Task<List<PermissionDefinitionDto>> GetAllPermissionsAsync()
    {
        await CheckPermissionAsync(DFAppPermissions.PermissionGrantManagement.Default);

        var result = new List<PermissionDefinitionDto>();
        var permissionGroups = typeof(DFAppPermissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var group in permissionGroups)
        {
            var groupName = group.Name;
            var fields = group.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string) && f.IsLiteral);

            foreach (var field in fields)
            {
                result.Add(new PermissionDefinitionDto
                {
                    GroupName = groupName,
                    Name = (string)field.GetRawConstantValue()!,
                    DisplayName = field.Name
                });
            }
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// 全量替换指定 Provider 的权限（事务）
    /// </summary>
    public async Task UpdateAsync(UpdatePermissionsDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.PermissionGrantManagement.Grant);

        try
        {
            _appPermissionGrantRepository.BeginTran();

            // 删除该 Provider 下的所有权限
            var existingGrants = await _appPermissionGrantRepository.GetQueryable()
                .Where(pg => pg.ProviderType == input.ProviderType && pg.ProviderKey == input.ProviderKey)
                .ToListAsync();

            if (existingGrants.Count > 0)
            {
                await _appPermissionGrantRepository.DeleteAsync(existingGrants);
            }

            // 批量插入新权限
            foreach (var permissionName in input.PermissionNames.Distinct())
            {
                var grant = new AppPermissionGrant
                {
                    PermissionName = permissionName,
                    ProviderType = input.ProviderType,
                    ProviderKey = input.ProviderKey
                };
                await _appPermissionGrantRepository.InsertAsync(grant);
            }

            _appPermissionGrantRepository.CommitTran();
            _logger.LogInformation("全量替换权限：ProviderType={ProviderType}, ProviderKey={ProviderKey}, 权限数量={Count}",
                input.ProviderType, input.ProviderKey, input.PermissionNames.Count);
        }
        catch
        {
            _appPermissionGrantRepository.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 增量授予权限，跳过已存在的
    /// </summary>
    public async Task GrantAsync(GrantPermissionsDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.PermissionGrantManagement.Grant);

        // 查询已存在的权限
        var existingGrants = await _appPermissionGrantRepository.GetQueryable()
            .Where(pg => pg.ProviderType == input.ProviderType && pg.ProviderKey == input.ProviderKey)
            .ToListAsync();

        var existingNames = existingGrants.Select(g => g.PermissionName).ToHashSet();
        var grantedCount = 0;

        foreach (var permissionName in input.PermissionNames.Distinct())
        {
            if (existingNames.Contains(permissionName))
            {
                continue;
            }

            var grant = new AppPermissionGrant
            {
                PermissionName = permissionName,
                ProviderType = input.ProviderType,
                ProviderKey = input.ProviderKey
            };
            await _appPermissionGrantRepository.InsertAsync(grant);
            grantedCount++;
        }

        _logger.LogInformation("增量授予权限：ProviderType={ProviderType}, ProviderKey={ProviderKey}, 新增={GrantedCount} 条",
            input.ProviderType, input.ProviderKey, grantedCount);
    }

    /// <summary>
    /// 撤销权限
    /// </summary>
    public async Task RevokeAsync(RevokePermissionsDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.PermissionGrantManagement.Revoke);

        var existingGrants = await _appPermissionGrantRepository.GetQueryable()
            .Where(pg => pg.ProviderType == input.ProviderType && pg.ProviderKey == input.ProviderKey)
            .ToListAsync();

        var namesToRevoke = input.PermissionNames.ToHashSet();
        var toDelete = existingGrants.Where(g => namesToRevoke.Contains(g.PermissionName)).ToList();

        if (toDelete.Count > 0)
        {
            await _appPermissionGrantRepository.DeleteAsync(toDelete);
        }

        _logger.LogInformation("撤销权限：ProviderType={ProviderType}, ProviderKey={ProviderKey}, 撤销={RevokedCount} 条",
            input.ProviderType, input.ProviderKey, toDelete.Count);
    }

    private static PermissionGrantDto MapToDto(AppPermissionGrant grant)
    {
        return new PermissionGrantDto
        {
            Id = grant.Id,
            PermissionName = grant.PermissionName,
            ProviderType = grant.ProviderType,
            ProviderKey = grant.ProviderKey
        };
    }
}
