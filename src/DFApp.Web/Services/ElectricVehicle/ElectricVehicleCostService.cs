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
    /// <summary>
    /// 该模块记录按创建者隔离：非管理员只能访问自己创建的记录
    /// </summary>
    protected override bool RequireOwnerCheck => true;

    private readonly ISqlSugarRepository<ElectricVehicleEntity, Guid> _vehicleRepository;
    private readonly ISqlSugarRepository<GasolinePrice, Guid> _gasolinePriceRepository;
    private readonly IConfigurationInfoRepository _configurationInfoRepository;
    private readonly ISqlSugarRepository<ElectricVehicleMileageRecord, Guid> _mileageRecordRepository;
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
    public ElectricVehicleCostService(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker,
        ISqlSugarRepository<ElectricVehicleCost, Guid> repository,
        ISqlSugarRepository<ElectricVehicleEntity, Guid> vehicleRepository,
        ISqlSugarRepository<GasolinePrice, Guid> gasolinePriceRepository,
        IConfigurationInfoRepository configurationInfoRepository,
        ISqlSugarRepository<ElectricVehicleMileageRecord, Guid> mileageRecordRepository)
        : base(currentUser, permissionChecker, repository)
    {
        _vehicleRepository = vehicleRepository;
        _gasolinePriceRepository = gasolinePriceRepository;
        _configurationInfoRepository = configurationInfoRepository;
        _mileageRecordRepository = mileageRecordRepository;
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
        var query = await ApplyOwnerFilterAsync(Repository.GetQueryable());

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

        var electricCosts = await FilterOwnedAsync(await Repository.GetListAsync(expression));

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

        // 里程快照：时间段锚点与油费分段共用。
        // 来源：车辆管理页"里程"按钮独立更新、充电记录联动更新（sql/28 已回填历史充电里程）。
        var mileageRecordQuery = _mileageRecordRepository.GetQueryable();
        if (input.VehicleId.HasValue)
        {
            var vehicleId = input.VehicleId.Value;
            mileageRecordQuery = mileageRecordQuery.Where(x => x.VehicleId == vehicleId);
        }
        var mileageRecords = mileageRecordQuery
            .OrderBy(x => x.RecordedTime)
            .ToList();
        // 油费分段使用的快照窗口
        List<ElectricVehicleMileageRecord> segmentSnapshots = new();

        // 获取选定日期范围内的行驶里程
        decimal electricVehicleMileage = 0;

        if (isAllTime)
        {
            // 全部时间：直接使用车辆总里程，油费从最早快照开始分段
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
            segmentSnapshots = mileageRecords;
        }
        else
        {
            // 特定时间范围：起点=开始日期当天或之前最近一条快照（无则取范围内最早一条），
            // 终点=结束日期当天或之前最近一条，区间里程=终点−起点。
            if (mileageRecords.Any())
            {
                var firstAnchor = mileageRecords.LastOrDefault(x => x.RecordedTime <= input.StartDate)
                    ?? mileageRecords.First();
                var lastAnchor = mileageRecords.LastOrDefault(x => x.RecordedTime <= input.EndDate);
                if (lastAnchor != null && lastAnchor.Mileage > firstAnchor.Mileage)
                {
                    electricVehicleMileage = lastAnchor.Mileage - firstAnchor.Mileage;
                }
                var firstIdx = mileageRecords.IndexOf(firstAnchor);
                var lastIdx = lastAnchor == null ? -1 : mileageRecords.IndexOf(lastAnchor);
                if (lastIdx >= firstIdx)
                {
                    segmentSnapshots = mileageRecords.GetRange(firstIdx, lastIdx - firstIdx + 1);
                }
            }

            // 没有可用里程快照时，使用车辆总里程
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

        decimal oilVehicleTotalCost = 0;
        decimal oilVehicleFuelCost = 0;

        if (electricVehicleMileage > 0 && segmentSnapshots.Count >= 2)
        {
            // 获取所有油价数据
            var allPrices = _gasolinePriceRepository.GetQueryable()
                .Where(x => x.Province == province)
                .OrderByDescending(x => x.Date)
                .ToList();

            // 获取最新油价作为默认值
            var latestPrice = allPrices.FirstOrDefault();
            var defaultGasolinePrice = latestPrice != null ? GetGasolinePriceByGrade(latestPrice, gasolineGrade) : 0;

            // 相邻里程快照分段：段里程 × 段结束时点油价（快照节奏为按月，即"按月油价"对比）
            decimal totalCalculatedMileage = 0;

            for (int i = 0; i < segmentSnapshots.Count - 1; i++)
            {
                var segment = segmentSnapshots[i + 1].Mileage - segmentSnapshots[i].Mileage;
                if (segment <= 0)
                {
                    continue;
                }

                // 时间段查询时，开始日期之前完成的段不计入本期
                if (!isAllTime && segmentSnapshots[i + 1].RecordedTime <= input.StartDate)
                {
                    continue;
                }

                totalCalculatedMileage += segment;

                var segmentEndTime = segmentSnapshots[i + 1].RecordedTime;
                var price = allPrices
                    .Where(x => x.Date <= segmentEndTime)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault();

                var gasolinePrice = defaultGasolinePrice;
                if (price != null)
                {
                    gasolinePrice = GetGasolinePriceByGrade(price, gasolineGrade);
                }

                if (gasolinePrice > 0)
                {
                    oilVehicleTotalCost += segment / 100 * fuelConsumption * gasolinePrice;
                }
            }

            // 跳过的异常段（里程回退等）按最新油价兜底
            var remainingMileage = electricVehicleMileage - totalCalculatedMileage;
            if (remainingMileage > 0 && defaultGasolinePrice > 0)
            {
                oilVehicleTotalCost += remainingMileage / 100 * fuelConsumption * defaultGasolinePrice;
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
