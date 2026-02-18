using System;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    public interface IElectricVehicleChargingRecordRepository : IRepository<ElectricVehicleChargingRecord, Guid>
    {
    }
}
