using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.ElectricVehicle
{
    public interface IGasolinePriceService : IApplicationService
    {
        Task<GasolinePriceDto?> GetLatestPriceAsync(string province);
        Task<GasolinePriceDto?> GetPriceByDateAsync(string province, DateTime date);
        Task RefreshGasolinePricesAsync();
        Task<PagedResultDto<GasolinePriceDto>> GetListAsync(GetGasolinePricesDto input);
    }
}
