using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using LotteryResultDto = DFApp.Web.DTOs.Lottery.LotteryResultDto;
using CreateUpdateLotteryResultDto = DFApp.Web.DTOs.Lottery.CreateUpdateLotteryResultDto;

namespace DFApp.Web.Services.Lottery;

/// <summary>
/// 彩票结果服务
/// </summary>
public class LotteryResultService : CrudServiceBase<LotteryResult, long, LotteryResultDto, CreateUpdateLotteryResultDto, CreateUpdateLotteryResultDto>
{
    private readonly LotteryMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">仓储接口</param>
    public LotteryResultService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<LotteryResult, long> repository)
        : base(currentUser, permissionChecker, repository)
    {
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">彩票结果实体</param>
    /// <returns>彩票结果 DTO</returns>
    protected override LotteryResultDto MapToGetOutputDto(LotteryResult entity)
    {
        return _mapper.MapToDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <returns>彩票结果实体</returns>
    protected override LotteryResult MapToEntity(CreateUpdateLotteryResultDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <param name="entity">彩票结果实体</param>
    protected override void MapToEntity(CreateUpdateLotteryResultDto input, LotteryResult entity)
    {
        _mapper.MapToEntity(input, entity);
    }
}
