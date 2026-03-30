using System;
using System.Threading.Tasks;
using DFApp.IP;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services.IP;

/// <summary>
/// 动态 IP 服务
/// </summary>
public class DynamicIPService : CrudServiceBase<DynamicIP, Guid, DynamicIPDto, CreateUpdateDynamicIPDto, CreateUpdateDynamicIPDto>
{
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
        // TODO: 使用 Mapperly 映射实体到 DTO
        return new DynamicIPDto
        {
            Id = entity.Id,
            IP = entity.IP,
            Port = entity.Port,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <returns>动态 IP 实体</returns>
    protected override DynamicIP MapToEntity(CreateUpdateDynamicIPDto input)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        return new DynamicIP
        {
            IP = input.IP!,
            Port = input.Port!
        };
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <param name="entity">动态 IP 实体</param>
    protected override void MapToEntity(CreateUpdateDynamicIPDto input, DynamicIP entity)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        entity.IP = input.IP!;
        entity.Port = input.Port!;
    }
}
