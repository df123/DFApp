using System.Collections.Generic;

namespace DFApp.Web.DTOs.Bookkeeping
{
    /// <summary>
    /// Chart.js 图表数据 DTO
    /// </summary>
    public class ChartJSDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public ChartJSDto()
        {
            labels = new List<string>();
            datasets = new List<ChartJSDatasetsItemDto>();
        }

        /// <summary>
        /// 支出总额
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// 比较期支出总额
        /// </summary>
        public decimal CompareTotal { get; set; }

        /// <summary>
        /// 差额（当前期 - 比较期）
        /// </summary>
        public decimal DifferenceTotal { get; set; }

        /// <summary>
        /// 标签列表（分类名称）
        /// </summary>
        public List<string> labels { get; set; }

        /// <summary>
        /// 数据集列表
        /// </summary>
        public List<ChartJSDatasetsItemDto> datasets { get; set; }
    }
}
