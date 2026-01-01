using System.Collections.Generic;

namespace DFApp.Aria2
{
    /// <summary>
    /// IP 地理位置 DTO
    /// </summary>
    public class IpGeolocationDto
    {
        /// <summary>
        /// 状态 (success/fail)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 查询的 IP 地址
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// 国家
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        public string? CountryCode { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 错误消息（失败时）
        /// </summary>
        public string? Message { get; set; }
    }

    /// <summary>
    /// 批量查询 IP 地理位置 DTO
    /// </summary>
    public class BatchIpGeolocationQueryDto
    {
        /// <summary>
        /// IP 地址列表（最多100个）
        /// </summary>
        public List<string> Ips { get; set; } = new List<string>();
    }
}
