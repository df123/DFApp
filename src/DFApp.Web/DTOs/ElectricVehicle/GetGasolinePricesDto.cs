using System;
using DFApp.Web.DTOs;

namespace DFApp.Web.DTOs.ElectricVehicle;

/// <summary>
/// 获取汽油价格列表请求 DTO
/// </summary>
public class GetGasolinePricesDto : PagedAndSortedResultRequestDto
{
    public string? Province { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
