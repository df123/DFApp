using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DFApp.ElectricVehicle
{
    public interface IGasolinePriceRepository : IRepository<GasolinePrice, Guid>
    {
        Task<GasolinePrice?> GetLatestPriceAsync(string province);
        Task<GasolinePrice?> GetPriceByDateAsync(string province, DateTime date);
    }
}
