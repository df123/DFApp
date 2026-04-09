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

namespace DFApp.Web.Services.Identity;

/// <summary>
/// 权限授予管理应用服务，使用新的 AppPermissionGrant 实体
/// </summary>
public class PermissionGrantManagementAppService : AppServiceBase
{
    private readonly ISqlSugarRepository<AppPermissionGrant, long> _appPermissionGrantRepository;
    private readonly ILogger<PermissionGrantManagementAppService> _logger;

    public PermissionGrantManagementAppService(
        ISqlSugarRepository<AppPermissionGrant, long> appPermissionGrantRepository,
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ILogger<PermissionGrantManagementAppService> logger)
        : base(currentUser, permissionChecker)
    {
        _appPermissionGrantRepository = appPermissionGrantRepository;
        _logger = logger;
    }

    /// <summary>
    /// 查询指定 Provider 的已授予权限
    /// </summary>
    public async Task<List<PermissionGrantDto>> GetListAsync(GetPermissionGrantListDto input)
    {
        await CheckPermissionAsync(DFAppPermissions.PermissionGrantManagement.Default);

        var grants = await _appPermissionGrantRepository.GetQueryable()
            .Where(pg => pg.ProviderType == input.ProviderType && pg.ProviderKey == input.ProviderKey)
            .ToListAsync();

        return grants.Select(MapToDto).ToList();
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
