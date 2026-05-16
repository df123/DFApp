using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Web.Data;
using DFApp.Web.Data.Configuration;
using DFApp.Web.Infrastructure;
using DFApp.Web.Mapping;
using DFApp.Web.Permissions;
using ElectricVehicleCostDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleCostDto;
using CreateUpdateElectricVehicleCostDto = DFApp.Web.DTOs.ElectricVehicle.CreateUpdateElectricVehicleCostDto;
using ElectricVehicleDto = DFApp.Web.DTOs.ElectricVehicle.ElectricVehicleDto;
using OilCostComparisonDto = DFApp.Web.DTOs.ElectricVehicle.OilCostComparisonDto;
using OilCostComparisonRequestDto = DFApp.Web.DTOs.ElectricVehicle.OilCostComparisonRequestDto;

using ElectricVehicleEntity = DFApp.ElectricVehicle.ElectricVehicle;

namespace DFApp.Web.Services.ElectricVehicle;

/// <summary>
/// 电动车成本记录服务
/// </summary>
public class ElectricVehicleCostService : CrudServiceBase<
    ElectricVehicleCost,
    Guid,
    ElectricVehicleCostDto,
    CreateUpdateElectricVehicleCostDto,
    CreateUpdateElectricVehicleCostDto>
{
    private readonly ISqlSugarRepository<ElectricVehicleEntity, Guid> _vehicleRepository;
    private readonly ISqlSugarRepository<GasolinePrice, Guid> _gasolinePriceRepository;
    private readonly IConfigurationInfoRepository _configurationInfoRepository;
    private readonly ISqlSugarRepository<ElectricVehicleChargingRecord, Guid> _chargingRecordRepository;
    private readonly ElectricVehicleMapper _mapper = new();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentUser">当前用户</param>
    /// <param name="permissionChecker">权限检查器</param>
    /// <param name="repository">成本记录仓储接口</param>
    /// <param name="vehicleRepository">车辆仓储接口</param>
    /// <param name="gasolinePriceRepository">油价仓储接口</param>
    /// <param name="configurationInfoRepository">配置信息仓储接口</param>
    /// <param name="chargingRecordRepository">充电记录仓储接口</param>
    public ElectricVehicleCostService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<ElectricVehicleCost, Guid> repository,
        ISqlSugarRepository<ElectricVehicleEntity, Guid> vehicleRepository,
        ISqlSugarRepository<GasolinePrice, Guid> gasolinePriceRepository,
        IConfigurationInfoRepository configurationInfoRepository,
        ISqlSugarRepository<ElectricVehicleChargingRecord, Guid> chargingRecordRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _vehicleRepository = vehicleRepository;
        _gasolinePriceRepository = gasolinePriceRepository;
        _configurationInfoRepository = configurationInfoRepository;
        _chargingRecordRepository = chargingRecordRepository;
    }

    /// <summary>
    /// 根据过滤条件分页查询成本记录
    /// 原始代码使用 WithDetailsAsync 导航查询 Vehicle，现改为外键查询
    /// </summary>
    /// <param name="filter">过滤关键字</param>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页结果</returns>
    public async Task<(List<ElectricVehicleCostDto> Items, int TotalCount)> GetFilteredListAsync(
        string? filter, int pageIndex, int pageSize)
    {
        var query = Repository.GetQueryable();

        // 应用过滤条件
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var filterLower = filter.ToLower();

            // 原始代码通过导航属性 x.Vehicle.Name 过滤，现改为子查询
            var matchingVehicleIds = _vehicleRepository.GetQueryable()
                .Where(v => v.Name != null && v.Name.ToLower().Contains(filterLower))
                .Select(v => v.Id)
                .ToList();

            query = query.Where(x =>
                matchingVehicleIds.Contains(x.VehicleId)
                || (x.Remark != null && x.Remark.ToLower().Contains(filterLower)));
        }

        // 获取总数
        var totalCount = query.Count();

        // 默认按成本日期降序排序
        var items = query
            .OrderByDescending(x => x.CostDate)
            .ToPageList(pageIndex, pageSize);

        // 获取关联的车辆信息（替代导航查询）
        var vehicleIds = items.Select(x => x.VehicleId).Distinct().ToList();
        var vehicles = await _vehicleRepository.GetListAsync(x => vehicleIds.Contains(x.Id));
        var vehicleDict = vehicles.ToDictionary(x => x.Id);

        // 手动映射 DTO
        var dtos = new List<ElectricVehicleCostDto>();
        foreach (var entity in items)
        {
            var dto = MapToGetOutputDto(entity);
            if (vehicleDict.TryGetValue(entity.VehicleId, out var vehicle))
            {
                dto.Vehicle = MapVehicleToDto(vehicle);
            }
            dtos.Add(dto);
        }

        return (dtos, totalCount);
    }

    /// <summary>
    /// 获取油电成本对比数据
    /// </summary>
    /// <param name="input">对比请求参数</param>
    /// <returns>油电成本对比 DTO</returns>
    public async Task<OilCostComparisonDto> GetOilCostComparisonAsync(OilCostComparisonRequestDto input)
    {
        // 从配置获取油车参数
        string province = "山东";
        GasolineGrade gasolineGrade = GasolineGrade.H95;
        decimal fuelConsumption = 8;

        try
        {
            province = await _configurationInfoRepository.GetConfigurationInfoValue("OilProvince", "DFApp.ElectricVehicle");
            if (string.IsNullOrWhiteSpace(province))
            {
                province = "山东";
            }
        }
        catch
        {
            province = "山东";
        }

        try
        {
            var gradeStr = await _configurationInfoRepository.GetConfigurationInfoValue("OilGasolineGrade", "DFApp.ElectricVehicle");
            if (int.TryParse(gradeStr, out int grade))
            {
                gasolineGrade = (GasolineGrade)grade;
            }
            else
            {
                gasolineGrade = GasolineGrade.H95;
            }
        }
        catch
        {
            gasolineGrade = GasolineGrade.H95;
        }

        try
        {
            var consumptionStr = await _configurationInfoRepository.GetConfigurationInfoValue("OilFuelConsumption", "DFApp.ElectricVehicle");
            if (decimal.TryParse(consumptionStr, out decimal consumption))
            {
                fuelConsumption = consumption;
            }
        }
        catch
        {
            fuelConsumption = 8;
        }

        // 从数据库查询电车成本
        var expression = BuildExpression(input.StartDate, input.EndDate, input.IsBelongToSelf);
        if (input.VehicleId.HasValue)
        {
            var vehicleId = input.VehicleId.Value;
            var parameter = expression.Parameters[0];
            var vehicleCondition = Expression.Equal(
                Expression.Property(parameter, nameof(ElectricVehicleCost.VehicleId)),
                Expression.Constant(vehicleId, typeof(Guid)));
            var combinedBody = Expression.AndAlso(expression.Body, vehicleCondition);
            expression = Expression.Lambda<Func<ElectricVehicleCost, bool>>(combinedBody, parameter);
        }

        var electricCosts = await Repository.GetListAsync(expression);

        // 计算电车数据
        var electricChargingCost = electricCosts
            .Where(x => x.CostType == CostType.Charging)
            .Sum(x => x.Amount);

        var electricOtherCost = electricCosts
            .Where(x => x.CostType != CostType.Charging)
            .Sum(x => x.Amount);

        var electricVehicleTotalCost = electricCosts.Sum(x => x.Amount);

        // 判断是否是"全部时间"（开始日期很早）
        var isAllTime = input.StartDate.Year <= 2000;

        // 获取选定日期范围内的行驶里程
        decimal electricVehicleMileage = 0;

        if (isAllTime)
        {
            // 全部时间：直接使用车辆总里程
            if (input.VehicleId.HasValue)
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(input.VehicleId.Value);
                if (vehicle != null)
                {
                    electricVehicleMileage = vehicle.TotalMileage;
                }
            }
            else if (electricCosts.Any())
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(electricCosts.First().VehicleId);
                if (vehicle != null)
                {
                    electricVehicleMileage = vehicle.TotalMileage;
                }
            }
        }
        else
        {
            // 特定时间范围：计算该范围内的里程差
            var mileageQuery = _chargingRecordRepository.GetQueryable();
            if (input.VehicleId.HasValue)
            {
                mileageQuery = mileageQuery.Where(x => x.VehicleId == input.VehicleId.Value);
            }
            var chargingRecordsInPeriod = mileageQuery
                .Where(x => x.ChargingDate >= input.StartDate && x.ChargingDate <= input.EndDate && x.CurrentMileage.HasValue)
                .OrderBy(x => x.ChargingDate)
                .ToList();

            if (chargingRecordsInPeriod.Count >= 2)
            {
                electricVehicleMileage = chargingRecordsInPeriod.Last().CurrentMileage!.Value - chargingRecordsInPeriod.First().CurrentMileage!.Value;
            }
            else if (chargingRecordsInPeriod.Count == 1)
            {
                electricVehicleMileage = chargingRecordsInPeriod[0].CurrentMileage!.Value;
            }

            // 如果没有充电记录，使用车辆总里程
            if (electricVehicleMileage == 0)
            {
                if (input.VehicleId.HasValue)
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(input.VehicleId.Value);
                    if (vehicle != null)
                    {
                        electricVehicleMileage = vehicle.TotalMileage;
                    }
                }
                else if (electricCosts.Any())
                {
                    var vehicle = await _vehicleRepository.GetByIdAsync(electricCosts.First().VehicleId);
                    if (vehicle != null)
                    {
                        electricVehicleMileage = vehicle.TotalMileage;
                    }
                }
            }
        }

        var electricVehicleCostPerKm = electricVehicleMileage > 0 ? electricVehicleTotalCost / electricVehicleMileage : 0;

        // 获取充电记录，用于计算对应时间段的油价
        var chargingQuery = _chargingRecordRepository.GetQueryable();
        var chargingRecords = chargingQuery
            .Where(x => x.ChargingDate >= input.StartDate && x.ChargingDate <= input.EndDate)
            .OrderBy(x => x.ChargingDate)
            .ToList();

        decimal oilVehicleTotalCost = 0;
        decimal oilVehicleFuelCost = 0;

        if (electricVehicleMileage > 0 && chargingRecords.Any())
        {
            // 获取所有油价数据
            var allPrices = _gasolinePriceRepository.GetQueryable()
                .Where(x => x.Province == province)
                .OrderByDescending(x => x.Date)
                .ToList();

            // 获取最新油价作为默认值
            var latestPrice = allPrices.FirstOrDefault();
            var defaultGasolinePrice = latestPrice != null ? GetGasolinePriceByGrade(latestPrice, gasolineGrade) : 0;

            // 计算油车在相同里程下的油费
            decimal previousMileage = 0;
            decimal totalCalculatedMileage = 0;

            for (int i = 0; i < chargingRecords.Count; i++)
            {
                var record = chargingRecords[i];
                var currentMileage = record.CurrentMileage ?? 0;

                if (currentMileage <= previousMileage)
                {
                    continue;
                }

                var mileage = currentMileage - previousMileage;
                totalCalculatedMileage += mileage;

                // 查找充电日期对应的油价（最接近的历史油价）
                var chargingDate = record.ChargingDate;
                var price = allPrices
                    .Where(x => x.Date <= chargingDate)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault();

                var gasolinePrice = defaultGasolinePrice;
                if (price != null)
                {
                    gasolinePrice = GetGasolinePriceByGrade(price, gasolineGrade);
                }

                if (gasolinePrice > 0)
                {
                    var oilCost = mileage / 100 * fuelConsumption * gasolinePrice;
                    oilVehicleTotalCost += oilCost;
                }

                previousMileage = currentMileage;
            }

            // 如果有剩余里程没有充电记录覆盖，使用最新油价计算
            var remainingMileage = electricVehicleMileage - totalCalculatedMileage;
            if (remainingMileage > 0 && defaultGasolinePrice > 0)
            {
                var oilCost = remainingMileage / 100 * fuelConsumption * defaultGasolinePrice;
                oilVehicleTotalCost += oilCost;
            }

            oilVehicleFuelCost = oilVehicleTotalCost;
        }

        // 计算油车每公里成本（基于总油费和总里程）
        var oilVehicleCostPerKm = electricVehicleMileage > 0 ? oilVehicleTotalCost / electricVehicleMileage : 0;

        // 获取最新油价用于显示
        var currentGasolinePrice = 0m;
        try
        {
            // TODO: IGasolinePriceRepository.GetLatestPriceAsync 未迁移，使用伪代码替代
            var latestPrice = _gasolinePriceRepository.GetQueryable()
                .Where(x => x.Province == province)
                .OrderByDescending(x => x.Date)
                .ToList()
                .FirstOrDefault();
            if (latestPrice != null)
            {
                currentGasolinePrice = GetGasolinePriceByGrade(latestPrice, gasolineGrade);
            }
        }
        catch { }

        // 计算对比
        var savings = oilVehicleTotalCost - electricVehicleTotalCost;
        var savingsPercentage = oilVehicleTotalCost > 0 ? (savings / oilVehicleTotalCost * 100) : 0;

        return new OilCostComparisonDto
        {
            // 电车数据
            ElectricVehicleTotalCost = electricVehicleTotalCost,
            ElectricVehicleMileage = electricVehicleMileage,
            ElectricVehicleCostPerKm = electricVehicleCostPerKm,
            ElectricChargingCost = electricChargingCost,
            ElectricOtherCost = electricOtherCost,

            // 油车数据
            OilVehicleCostPerKm = oilVehicleCostPerKm,
            OilVehicleTotalCost = oilVehicleTotalCost,
            OilVehicleFuelCost = oilVehicleFuelCost,

            // 对比数据
            Savings = savings,
            SavingsPercentage = savingsPercentage,
            Province = province,
            CurrentGasolinePrice = currentGasolinePrice,
            GasolineGrade = gasolineGrade,
            FuelConsumption = fuelConsumption,

            // 时间范围
            StartDate = input.StartDate,
            EndDate = input.EndDate
        };
    }

    /// <summary>
    /// 构建日期范围查询表达式
    /// </summary>
    /// <param name="start">开始日期</param>
    /// <param name="end">结束日期</param>
    /// <param name="isBelongToSelf">是否属于自己</param>
    /// <returns>查询表达式</returns>
    private Expression<Func<ElectricVehicleCost, bool>> BuildExpression(DateTime start, DateTime end, bool? isBelongToSelf)
    {
        Expression<Func<ElectricVehicleCost, bool>> expression = x => x.CostDate >= start && x.CostDate <= end;

        if (isBelongToSelf.HasValue)
        {
            var isSelf = isBelongToSelf.Value;
            var parameter = expression.Parameters[0];
            var selfCondition = Expression.Equal(
                Expression.Property(parameter, nameof(ElectricVehicleCost.IsBelongToSelf)),
                Expression.Constant(isSelf, typeof(bool)));
            var combinedBody = Expression.AndAlso(expression.Body, selfCondition);
            expression = Expression.Lambda<Func<ElectricVehicleCost, bool>>(combinedBody, parameter);
        }

        return expression;
    }

    /// <summary>
    /// 根据油号获取油价
    /// </summary>
    /// <param name="price">油价实体</param>
    /// <param name="grade">油号</param>
    /// <returns>油价</returns>
    private decimal GetGasolinePriceByGrade(GasolinePrice price, GasolineGrade grade)
    {
        return grade switch
        {
            GasolineGrade.H92 => price.Price92H ?? 0,
            GasolineGrade.H95 => price.Price95H ?? 0,
            GasolineGrade.H98 => price.Price98H ?? 0,
            _ => 0
        };
    }

    /// <summary>
    /// 将实体映射为输出 DTO
    /// </summary>
    /// <param name="entity">成本记录实体</param>
    /// <returns>成本记录 DTO</returns>
    protected override ElectricVehicleCostDto MapToGetOutputDto(ElectricVehicleCost entity)
    {
        return _mapper.MapToCostDto(entity);
    }

    /// <summary>
    /// 将创建输入 DTO 映射为实体
    /// </summary>
    /// <param name="input">创建输入 DTO</param>
    /// <returns>成本记录实体</returns>
    protected override ElectricVehicleCost MapToEntity(CreateUpdateElectricVehicleCostDto input)
    {
        return _mapper.MapToEntity(input);
    }

    /// <summary>
    /// 将更新输入 DTO 映射到现有实体
    /// </summary>
    /// <param name="input">更新输入 DTO</param>
    /// <param name="entity">成本记录实体</param>
    protected override void MapToEntity(CreateUpdateElectricVehicleCostDto input, ElectricVehicleCost entity)
    {
        var mapped = _mapper.MapToEntity(input);
        entity.VehicleId = mapped.VehicleId;
        entity.CostType = mapped.CostType;
        entity.CostDate = mapped.CostDate;
        entity.Amount = mapped.Amount;
        entity.IsBelongToSelf = mapped.IsBelongToSelf;
        entity.Remark = mapped.Remark;
    }

    /// <summary>
    /// 将车辆实体映射为 DTO
    /// </summary>
    /// <param name="vehicle">车辆实体</param>
    /// <returns>车辆 DTO</returns>
    private ElectricVehicleDto MapVehicleToDto(ElectricVehicleEntity vehicle)
    {
        return _mapper.MapToDto(vehicle);
    }
}
