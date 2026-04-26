using System.ComponentModel.DataAnnotations;

namespace DFApp.Web.DTOs.IP;

/// <summary>
/// 创建/更新动态 IP DTO
/// </summary>
public class CreateUpdateDynamicIPDto
{
    [Required]
    [StringLength(15)]
    public string? IP { get; set; }

    [Required]
    [StringLength(5)]
    public string? Port { get; set; }
}
