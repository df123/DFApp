using System;
using System.Threading.Tasks;
using DFApp.IP;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services.IP;

/// <summary>
/// 动态 IP 服务
/// </summary>
public class DynamicIPService : CrudServiceBase<DynamicIP, Guid, DynamicIPDto, CreateUpdateDynamicIPDto, CreateUpdateDynamicIPDto>
{
    private readonly IPMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    public DynamicIPService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<DynamicIP, Guid> repository)
        : base(currentUser, permissionChecker, repository)
    {
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">动态 IP 实体</param>
    /// <returns>动态 IP DTO</returns>
    protected override DynamicIPDto MapToGetOutputDto(DynamicIP entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <returns>动态 IP 实体</returns>
    protected override DynamicIP MapToEntity(CreateUpdateDynamicIPDto input)
    {
        var entity = _mapper.MapToEntity(input);
        entity.IP = input.IP ?? string.Empty;
        entity.Port = input.Port ?? string.Empty;
        return entity;
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <param name="entity">动态 IP 实体</param>
    protected override void MapToEntity(CreateUpdateDynamicIPDto input, DynamicIP entity)
    {
        entity.IP = input.IP ?? string.Empty;
        entity.Port = input.Port ?? string.Empty;
    }
}
