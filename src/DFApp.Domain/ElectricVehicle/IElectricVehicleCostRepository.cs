using System;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    public interface IElectricVehicleCostRepository : IRepository<ElectricVehicleCost, Guid>
    {
    }
}
