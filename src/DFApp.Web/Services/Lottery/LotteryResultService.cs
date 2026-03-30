using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Lottery;
using DFApp.Web.Data;
using DFApp.Web.Infrastructure;
using DFApp.Web.Permissions;

namespace DFApp.Web.Services.Lottery;

/// <summary>
/// 彩票结果服务
/// </summary>
public class LotteryResultService : CrudServiceBase<LotteryResult, long, LotteryResultDto, CreateUpdateLotteryResultDto, CreateUpdateLotteryResultDto>
{
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
        // TODO: 使用 Mapperly 映射实体到 DTO
        return new LotteryResultDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            DetailsLink = entity.DetailsLink,
            VideoLink = entity.VideoLink,
            Date = entity.Date,
            Week = entity.Week,
            Red = entity.Red,
            Blue = entity.Blue,
            Blue2 = entity.Blue2,
            Sales = entity.Sales,
            PoolMoney = entity.PoolMoney,
            Content = entity.Content,
            AddMoney = entity.AddMoney,
            AddMoney2 = entity.AddMoney2,
            Msg = entity.Msg,
            Z2Add = entity.Z2Add,
            M2Add = entity.M2Add,
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
    /// <returns>彩票结果实体</returns>
    protected override LotteryResult MapToEntity(CreateUpdateLotteryResultDto input)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        return new LotteryResult
        {
            Name = input.Name,
            Code = input.Code,
            DetailsLink = input.DetailsLink,
            VideoLink = input.VideoLink,
            Date = input.Date,
            Week = input.Week,
            Red = input.Red,
            Blue = input.Blue,
            Blue2 = input.Blue2,
            Sales = input.Sales,
            PoolMoney = input.PoolMoney,
            Content = input.Content,
            AddMoney = input.AddMoney,
            AddMoney2 = input.AddMoney2,
            Msg = input.Msg,
            Z2Add = input.Z2Add,
            M2Add = input.M2Add
        };
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">创建/更新 DTO</param>
    /// <param name="entity">彩票结果实体</param>
    protected override void MapToEntity(CreateUpdateLotteryResultDto input, LotteryResult entity)
    {
        // TODO: 使用 Mapperly 映射 DTO 到实体
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.DetailsLink = input.DetailsLink;
        entity.VideoLink = input.VideoLink;
        entity.Date = input.Date;
        entity.Week = input.Week;
        entity.Red = input.Red;
        entity.Blue = input.Blue;
        entity.Blue2 = input.Blue2;
        entity.Sales = input.Sales;
        entity.PoolMoney = input.PoolMoney;
        entity.Content = input.Content;
        entity.AddMoney = input.AddMoney;
        entity.AddMoney2 = input.AddMoney2;
        entity.Msg = input.Msg;
        entity.Z2Add = input.Z2Add;
        entity.M2Add = input.M2Add;
    }
}
