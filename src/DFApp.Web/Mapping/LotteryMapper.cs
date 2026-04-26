using System.Collections.Generic;
using System.Linq;
using DFApp.Lottery;
using Riok.Mapperly.Abstractions;
using LotteryInfoEntity = DFApp.Lottery.LotteryInfo;
using LotteryResultEntity = DFApp.Lottery.LotteryResult;
using LotteryPrizegradesEntity = DFApp.Lottery.LotteryPrizegrades;
using LotterySimulationEntity = DFApp.Lottery.LotterySimulation;
using LotteryDtoType = DFApp.Web.DTOs.Lottery.LotteryDto;
using CreateUpdateLotteryDtoType = DFApp.Web.DTOs.Lottery.CreateUpdateLotteryDto;
using LotteryResultDtoType = DFApp.Web.DTOs.Lottery.LotteryResultDto;
using CreateUpdateLotteryResultDtoType = DFApp.Web.DTOs.Lottery.CreateUpdateLotteryResultDto;
using LotteryPrizegradesDtoType = DFApp.Web.DTOs.Lottery.LotteryPrizegradesDto;
using CreateUpdateLotteryPrizegradesDtoType = DFApp.Web.DTOs.Lottery.CreateUpdateLotteryPrizegradesDto;
using ResultItemDtoType = DFApp.Web.DTOs.Lottery.ResultItemDto;
using PrizegradesItemDtoType = DFApp.Web.DTOs.Lottery.PrizegradesItemDto;
using SSQSimulationDto = DFApp.Web.DTOs.Lottery.Simulation.SSQ.LotterySimulationDto;
using SSQCreateUpdateDto = DFApp.Web.DTOs.Lottery.Simulation.SSQ.CreateUpdateLotterySimulationDto;
using KL8SimulationDto = DFApp.Web.DTOs.Lottery.Simulation.KL8.LotterySimulationDto;
using KL8CreateUpdateDto = DFApp.Web.DTOs.Lottery.Simulation.KL8.CreateUpdateLotterySimulationDto;

namespace DFApp.Web.Mapping;

/// <summary>
/// 彩票模块映射器
/// </summary>
[Mapper]
public partial class LotteryMapper
{
    // ==================== LotteryInfo 映射 ====================

    /// <summary>
    /// LotteryInfo → LotteryDto
    /// </summary>
    public partial LotteryDtoType MapToDto(LotteryInfoEntity entity);

    /// <summary>
    /// CreateUpdateLotteryDto → LotteryInfo（忽略审计字段）
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryInfoEntity.ConcurrencyStamp))]
    public partial LotteryInfoEntity MapToEntity(CreateUpdateLotteryDtoType dto);

    /// <summary>
    /// LotteryDto → CreateUpdateLotteryDto
    /// </summary>
    public partial CreateUpdateLotteryDtoType MapToCreateUpdateDto(LotteryDtoType dto);

    /// <summary>
    /// 将 CreateUpdateLotteryDto 的值更新到已有 LotteryInfo 实体
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryInfoEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(LotteryInfoEntity.Id))]
    [MapperIgnoreTarget(nameof(LotteryInfoEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(LotteryInfoEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(LotteryInfoEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(LotteryInfoEntity.LastModifierId))]
    public partial void MapToEntity(CreateUpdateLotteryDtoType dto, LotteryInfoEntity entity);

    // ==================== LotteryResult 映射 ====================

    /// <summary>
    /// LotteryResult → LotteryResultDto（手动处理 Prizegrades 集合映射）
    /// </summary>
    public LotteryResultDtoType MapToDto(LotteryResultEntity entity)
    {
        return new LotteryResultDtoType
        {
            Id = entity.Id,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime,
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
            Prizegrades = entity.Prizegrades?.Select(MapToDto).ToList()
        };
    }

    /// <summary>
    /// CreateUpdateLotteryResultDto → LotteryResult（忽略审计字段，手动处理 Prizegrades 集合）
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryResultEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(LotteryResultEntity.Prizegrades))]
    public partial LotteryResultEntity MapToEntity(CreateUpdateLotteryResultDtoType dto);

    /// <summary>
    /// 将 CreateUpdateLotteryResultDto 的值更新到已有 LotteryResult 实体
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryResultEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(LotteryResultEntity.Prizegrades))]
    [MapperIgnoreTarget(nameof(LotteryResultEntity.Id))]
    [MapperIgnoreTarget(nameof(LotteryResultEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(LotteryResultEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(LotteryResultEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(LotteryResultEntity.LastModifierId))]
    public partial void MapToEntity(CreateUpdateLotteryResultDtoType dto, LotteryResultEntity entity);

    /// <summary>
    /// LotteryResultDto → CreateUpdateLotteryResultDto（手动处理 Prizegrades 集合映射）
    /// </summary>
    public CreateUpdateLotteryResultDtoType MapToCreateUpdateDto(LotteryResultDtoType dto)
    {
        return new CreateUpdateLotteryResultDtoType
        {
            Name = dto.Name,
            Code = dto.Code,
            DetailsLink = dto.DetailsLink,
            VideoLink = dto.VideoLink,
            Date = dto.Date,
            Week = dto.Week,
            Red = dto.Red,
            Blue = dto.Blue,
            Blue2 = dto.Blue2,
            Sales = dto.Sales,
            PoolMoney = dto.PoolMoney,
            Content = dto.Content,
            AddMoney = dto.AddMoney,
            AddMoney2 = dto.AddMoney2,
            Msg = dto.Msg,
            Z2Add = dto.Z2Add,
            M2Add = dto.M2Add,
            Prizegrades = dto.Prizegrades?.Select(MapToCreateUpdateDto).ToList()
        };
    }

    // ==================== LotteryPrizegrades 映射 ====================

    /// <summary>
    /// LotteryPrizegrades → LotteryPrizegradesDto
    /// </summary>
    public partial LotteryPrizegradesDtoType MapToDto(LotteryPrizegradesEntity entity);

    /// <summary>
    /// CreateUpdateLotteryPrizegradesDto → LotteryPrizegrades（忽略审计字段）
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryPrizegradesEntity.ConcurrencyStamp))]
    public partial LotteryPrizegradesEntity MapToEntity(CreateUpdateLotteryPrizegradesDtoType dto);

    /// <summary>
    /// LotteryPrizegradesDto → CreateUpdateLotteryPrizegradesDto
    /// </summary>
    public partial CreateUpdateLotteryPrizegradesDtoType MapToCreateUpdateDto(LotteryPrizegradesDtoType dto);

    // ==================== 外部数据中间 DTO 映射（新命名空间 DFApp.Web.DTOs.Lottery）====================

    /// <summary>
    /// ResultItemDto → CreateUpdateLotteryResultDto（外部数据转内部 DTO，手动处理集合）
    /// </summary>
    public CreateUpdateLotteryResultDtoType MapResultItemToCreateUpdateDto(ResultItemDtoType dto)
    {
        return new CreateUpdateLotteryResultDtoType
        {
            Name = dto.Name,
            Code = dto.Code,
            DetailsLink = dto.DetailsLink,
            VideoLink = dto.VideoLink,
            Date = dto.Date,
            Week = dto.Week,
            Red = dto.Red,
            Blue = dto.Blue,
            Blue2 = dto.Blue2,
            Sales = dto.Sales,
            PoolMoney = dto.PoolMoney,
            Content = dto.Content,
            AddMoney = dto.AddMoney,
            AddMoney2 = dto.AddMoney2,
            Msg = dto.Msg,
            Z2Add = dto.Z2Add,
            M2Add = dto.M2Add,
            Prizegrades = dto.Prizegrades?.Select(MapPrizegradesItemToCreateUpdateDto).ToList()
        };
    }

    /// <summary>
    /// PrizegradesItemDto → CreateUpdateLotteryPrizegradesDto（外部数据转内部 DTO）
    /// </summary>
    public partial CreateUpdateLotteryPrizegradesDtoType MapPrizegradesItemToCreateUpdateDto(PrizegradesItemDtoType dto);

    /// <summary>
    /// ResultItemDto → LotteryResult（忽略 Prizegrades 导航属性）
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryResultEntity.Prizegrades))]
    public partial LotteryResultEntity MapToEntityFromResultItem(ResultItemDtoType dto);

    /// <summary>
    /// PrizegradesItemDto → LotteryPrizegrades（忽略导航属性和外键）
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryPrizegradesEntity.LotteryResultId))]
    [MapperIgnoreTarget(nameof(LotteryPrizegradesEntity.Result))]
    public partial LotteryPrizegradesEntity MapToEntityFromPrizegradesItem(PrizegradesItemDtoType dto);

    // ==================== 外部数据中间 DTO 映射（旧命名空间 DFApp.Lottery，用于 JSON 反序列化）====================

    /// <summary>
    /// 旧命名空间 ResultItemDto → LotteryResult（忽略 Prizegrades 导航属性）
    /// 用于 LotteryDataFetchService 中 JSON 反序列化后的映射
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryResultEntity.Prizegrades))]
    public partial LotteryResultEntity MapToEntityFromExternalResultItem(ResultItemDto dto);

    /// <summary>
    /// 旧命名空间 PrizegradesItemDto → LotteryPrizegrades（忽略导航属性和外键）
    /// 用于 LotteryDataFetchService 中 JSON 反序列化后的映射
    /// </summary>
    [MapperIgnoreTarget(nameof(LotteryPrizegradesEntity.LotteryResultId))]
    [MapperIgnoreTarget(nameof(LotteryPrizegradesEntity.Result))]
    public partial LotteryPrizegradesEntity MapToEntityFromExternalPrizegradesItem(PrizegradesItemDto dto);

    // ==================== LotterySimulation SSQ 映射 ====================

    /// <summary>
    /// LotterySimulation → SSQ LotterySimulationDto
    /// </summary>
    public partial SSQSimulationDto MapToSSQDto(LotterySimulationEntity entity);

    /// <summary>
    /// SSQ CreateUpdateLotterySimulationDto → LotterySimulation（忽略审计字段）
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    public partial LotterySimulationEntity MapToEntityFromSSQ(SSQCreateUpdateDto dto);

    // ==================== LotterySimulation KL8 映射 ====================

    /// <summary>
    /// LotterySimulation → KL8 LotterySimulationDto
    /// </summary>
    public partial KL8SimulationDto MapToKL8Dto(LotterySimulationEntity entity);

    /// <summary>
    /// KL8 CreateUpdateLotterySimulationDto → LotterySimulation（忽略审计字段）
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    public partial LotterySimulationEntity MapToEntityFromKL8(KL8CreateUpdateDto dto);

    /// <summary>
    /// 将 SSQ CreateUpdateLotterySimulationDto 的值更新到已有 LotterySimulation 实体
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.Id))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModifierId))]
    public partial void MapToEntityFromSSQ(SSQCreateUpdateDto dto, LotterySimulationEntity entity);

    /// <summary>
    /// 将 KL8 CreateUpdateLotterySimulationDto 的值更新到已有 LotterySimulation 实体
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.Id))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModifierId))]
    public partial void MapToEntityFromKL8(KL8CreateUpdateDto dto, LotterySimulationEntity entity);

    // ==================== 旧命名空间 DTO 映射（用于服务层过渡期）====================

    /// <summary>
    /// LotteryInfo → 旧命名空间 LotteryDto（用于 CompoundLotteryService）
    /// </summary>
    public DFApp.Lottery.LotteryDto MapToExternalLotteryDto(LotteryInfoEntity entity)
    {
        return new DFApp.Lottery.LotteryDto
        {
            Id = entity.Id,
            IndexNo = entity.IndexNo,
            Number = entity.Number ?? string.Empty,
            ColorType = entity.ColorType ?? string.Empty,
            LotteryType = entity.LotteryType ?? string.Empty,
            GroupId = entity.GroupId,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    /// <summary>
    /// LotterySimulation → 旧命名空间 SSQ LotterySimulationDto（用于 LotterySSQSimulationService）
    /// </summary>
    public DFApp.Lottery.Simulation.SSQ.LotterySimulationDto MapToExternalSSQDto(LotterySimulationEntity entity)
    {
        return new DFApp.Lottery.Simulation.SSQ.LotterySimulationDto
        {
            Id = entity.Id,
            TermNumber = entity.TermNumber,
            BallType = entity.BallType,
            GameType = entity.GameType,
            GroupId = entity.GroupId,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    /// <summary>
    /// 旧命名空间 SSQ CreateUpdateLotterySimulationDto → LotterySimulation
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    public partial LotterySimulationEntity MapToEntityFromExternalSSQ(DFApp.Lottery.Simulation.SSQ.CreateUpdateLotterySimulationDto dto);

    /// <summary>
    /// 将旧命名空间 SSQ CreateUpdateLotterySimulationDto 的值更新到已有 LotterySimulation 实体
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.Id))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModifierId))]
    public partial void MapToEntityFromExternalSSQ(DFApp.Lottery.Simulation.SSQ.CreateUpdateLotterySimulationDto dto, LotterySimulationEntity entity);

    /// <summary>
    /// LotterySimulation → 旧命名空间 KL8 LotterySimulationDto（用于 LotteryKL8SimulationService）
    /// </summary>
    public DFApp.Lottery.Simulation.KL8.LotterySimulationDto MapToExternalKL8Dto(LotterySimulationEntity entity)
    {
        return new DFApp.Lottery.Simulation.KL8.LotterySimulationDto
        {
            Id = entity.Id,
            TermNumber = entity.TermNumber,
            BallType = entity.BallType,
            GameType = entity.GameType,
            GroupId = entity.GroupId,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    /// <summary>
    /// 旧命名空间 KL8 CreateUpdateLotterySimulationDto → LotterySimulation
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    public partial LotterySimulationEntity MapToEntityFromExternalKL8(DFApp.Lottery.Simulation.KL8.CreateUpdateLotterySimulationDto dto);

    /// <summary>
    /// 将旧命名空间 KL8 CreateUpdateLotterySimulationDto 的值更新到已有 LotterySimulation 实体
    /// </summary>
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.Id))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.CreatorId))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModificationTime))]
    [MapperIgnoreTarget(nameof(LotterySimulationEntity.LastModifierId))]
    public partial void MapToEntityFromExternalKL8(DFApp.Lottery.Simulation.KL8.CreateUpdateLotterySimulationDto dto, LotterySimulationEntity entity);
}
