using System;
using Volo.Abp.Application.Dtos;

namespace DFApp.ElectricVehicle
{
    public class GetGasolinePricesDto : PagedAndSortedResultRequestDto
    {
        public string? Province { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
