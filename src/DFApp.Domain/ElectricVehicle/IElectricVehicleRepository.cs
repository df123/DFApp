using System;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    public interface IElectricVehicleRepository : IRepository<ElectricVehicle, Guid>
    {
    }
}
