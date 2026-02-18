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
        Task RefreshGasolinePricesAsync(RefreshGasolinePriceDto input);
        Task<PagedResultDto<GasolinePriceDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    }

    public class RefreshGasolinePriceDto
    {
        public string Province { get; set; } = "山东";
    }
}
