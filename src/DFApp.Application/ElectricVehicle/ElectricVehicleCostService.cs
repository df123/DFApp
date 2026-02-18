using System;
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

        public ElectricVehicleCostService(
            IRepository<ElectricVehicleCost, Guid> repository,
            IRepository<DFApp.ElectricVehicle.ElectricVehicle, Guid> vehicleRepository,
            IGasolinePriceRepository gasolinePriceRepository,
            IConfigurationInfoRepository configurationInfoRepository) 
            : base(repository)
        {
            _vehicleRepository = vehicleRepository;
            _gasolinePriceRepository = gasolinePriceRepository;
            _configurationInfoRepository = configurationInfoRepository;
            
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
            
            // 获取总里程
            decimal electricVehicleMileage = 0;
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
            
            var electricVehicleCostPerKm = electricVehicleMileage > 0 ? electricVehicleTotalCost / electricVehicleMileage : 0;
            
            // 获取最新油价
            var latestPrice = await _gasolinePriceRepository.GetLatestPriceAsync(province);
            
            if (latestPrice == null)
            {
                throw new UserFriendlyException($"未找到{province}的油价数据，请刷新油价");
            }
            
            // 获取对应标号的油价
            decimal currentGasolinePrice = GetGasolinePriceByGrade(latestPrice, gasolineGrade);
            
            // 计算油车数据
            var oilVehicleCostPerKm = fuelConsumption * currentGasolinePrice / 100;
            var oilVehicleTotalCost = electricVehicleMileage * oilVehicleCostPerKm;
            var oilVehicleFuelCost = oilVehicleTotalCost;
            
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
