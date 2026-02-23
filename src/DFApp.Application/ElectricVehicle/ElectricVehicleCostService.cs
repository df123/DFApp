using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DFApp.ElectricVehicle;
using DFApp.Permissions;
using DFApp.CommonDtos;
using DFApp.Configuration;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    [Authorize(DFAppPermissions.ElectricVehicleCost.Default)]
    public class ElectricVehicleCostService : CrudAppService<
        ElectricVehicleCost,
        ElectricVehicleCostDto,
        Guid,
        FilterAndPagedAndSortedResultRequestDto,
        CreateUpdateElectricVehicleCostDto>, IElectricVehicleCostService
    {
        private readonly IRepository<DFApp.ElectricVehicle.ElectricVehicle, Guid> _vehicleRepository;
        private readonly IGasolinePriceRepository _gasolinePriceRepository;
        private readonly IConfigurationInfoRepository _configurationInfoRepository;
        private readonly IRepository<ElectricVehicleChargingRecord, Guid> _chargingRecordRepository;

        public ElectricVehicleCostService(
            IRepository<ElectricVehicleCost, Guid> repository,
            IRepository<DFApp.ElectricVehicle.ElectricVehicle, Guid> vehicleRepository,
            IGasolinePriceRepository gasolinePriceRepository,
            IConfigurationInfoRepository configurationInfoRepository,
            IRepository<ElectricVehicleChargingRecord, Guid> chargingRecordRepository)
            : base(repository)
        {
            _vehicleRepository = vehicleRepository;
            _gasolinePriceRepository = gasolinePriceRepository;
            _configurationInfoRepository = configurationInfoRepository;
            _chargingRecordRepository = chargingRecordRepository;

            GetPolicyName = DFAppPermissions.ElectricVehicleCost.Default;
            GetListPolicyName = DFAppPermissions.ElectricVehicleCost.Default;
            CreatePolicyName = DFAppPermissions.ElectricVehicleCost.Create;
            UpdatePolicyName = DFAppPermissions.ElectricVehicleCost.Edit;
            DeletePolicyName = DFAppPermissions.ElectricVehicleCost.Delete;
        }

        protected override async Task<System.Linq.IQueryable<ElectricVehicleCost>> CreateFilteredQueryAsync(FilterAndPagedAndSortedResultRequestDto input)
        {
            var query = await Repository.WithDetailsAsync();
            
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var filter = input.Filter.ToLower();
                query = query.Where(x => 
                    (x.Vehicle.Name != null && x.Vehicle.Name.ToLower().Contains(filter)) ||
                    (x.Remark != null && x.Remark.ToLower().Contains(filter)));
            }
            
            return query;
        }

        public override async Task<PagedResultDto<ElectricVehicleCostDto>> GetListAsync(FilterAndPagedAndSortedResultRequestDto input)
        {
            var query = await CreateFilteredQueryAsync(input);
            var totalCount = await AsyncExecuter.CountAsync(query);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            var entities = await AsyncExecuter.ToListAsync(query);

            var vehicleIds = entities.Select(x => x.VehicleId).Distinct().ToList();
            var vehicles = await _vehicleRepository.GetListAsync(x => vehicleIds.Contains(x.Id));
            var vehicleDict = vehicles.ToDictionary(x => x.Id);

            var dtos = new List<ElectricVehicleCostDto>();
            foreach (var entity in entities)
            {
                var dto = await MapToGetListOutputDtoAsync(entity);
                if (vehicleDict.TryGetValue(entity.VehicleId, out var vehicle))
                {
                    dto.Vehicle = new ElectricVehicleDto
                    {
                        Id = vehicle.Id,
                        Name = vehicle.Name,
                        Brand = vehicle.Brand,
                        Model = vehicle.Model,
                        LicensePlate = vehicle.LicensePlate,
                        PurchaseDate = vehicle.PurchaseDate,
                        BatteryCapacity = vehicle.BatteryCapacity,
                        TotalMileage = vehicle.TotalMileage,
                        Remark = vehicle.Remark,
                        CreationTime = vehicle.CreationTime
                    };
                }
                dtos.Add(dto);
            }

            return new PagedResultDto<ElectricVehicleCostDto>(totalCount, dtos);
        }

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
                expression = expression.And(x => x.VehicleId == input.VehicleId.Value);
            }
            
            var electricCosts = await ReadOnlyRepository.GetListAsync(expression, true);
            
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
                    var vehicle = await _vehicleRepository.GetAsync(input.VehicleId.Value);
                    electricVehicleMileage = vehicle.TotalMileage;
                }
                else if (electricCosts.Any())
                {
                    var vehicle = await _vehicleRepository.GetAsync(electricCosts.First().VehicleId);
                    electricVehicleMileage = vehicle.TotalMileage;
                }
            }
            else
            {
                // 特定时间范围：计算该范围内的里程差
                var mileageQuery = await _chargingRecordRepository.GetQueryableAsync();
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
                    electricVehicleMileage = chargingRecordsInPeriod.Last().CurrentMileage.Value - chargingRecordsInPeriod.First().CurrentMileage.Value;
                }
                else if (chargingRecordsInPeriod.Count == 1)
                {
                    electricVehicleMileage = chargingRecordsInPeriod[0].CurrentMileage.Value;
                }

                // 如果没有充电记录，使用车辆总里程
                if (electricVehicleMileage == 0)
                {
                    if (input.VehicleId.HasValue)
                    {
                        var vehicle = await _vehicleRepository.GetAsync(input.VehicleId.Value);
                        electricVehicleMileage = vehicle.TotalMileage;
                    }
                    else if (electricCosts.Any())
                    {
                        var vehicle = await _vehicleRepository.GetAsync(electricCosts.First().VehicleId);
                        electricVehicleMileage = vehicle.TotalMileage;
                    }
                }
            }

            var electricVehicleCostPerKm = electricVehicleMileage > 0 ? electricVehicleTotalCost / electricVehicleMileage : 0;

            // 获取充电记录，用于计算对应时间段的油价
            var chargingQuery = await _chargingRecordRepository.GetQueryableAsync();
            var chargingRecords = chargingQuery
                .Where(x => x.ChargingDate >= input.StartDate && x.ChargingDate <= input.EndDate)
                .OrderBy(x => x.ChargingDate)
                .ToList();

            decimal oilVehicleTotalCost = 0;
            decimal oilVehicleFuelCost = 0;

            if (electricVehicleMileage > 0 && chargingRecords.Any())
            {
                // 获取所有油价数据
                var priceQuery = await _gasolinePriceRepository.GetQueryableAsync();
                var allPrices = priceQuery
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
                var latestPrice = await _gasolinePriceRepository.GetLatestPriceAsync(province);
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

        private Expression<Func<ElectricVehicleCost, bool>> BuildExpression(DateTime start, DateTime end, bool? isBelongToSelf)
        {
            Expression<Func<ElectricVehicleCost, bool>> expression = x => x.CostDate >= start && x.CostDate <= end;

            if (isBelongToSelf.HasValue)
            {
                expression = expression.And(x => x.IsBelongToSelf == isBelongToSelf.Value);
            }

            return expression;
        }

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
    }
}
