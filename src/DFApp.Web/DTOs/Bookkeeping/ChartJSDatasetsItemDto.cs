using System.Collections.Generic;

namespace DFApp.Web.DTOs.Bookkeeping
{
    /// <summary>
    /// Chart.js 数据集项 DTO
    /// </summary>
    public class ChartJSDatasetsItemDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public ChartJSDatasetsItemDto()
        {
            data = new List<decimal>();
        }

        /// <summary>
        /// 数据列表（各分类的支出金额或百分比）
        /// </summary>
        public List<decimal> data { get; set; }

        /// <summary>
        /// 数据集标签（日期范围）
        /// </summary>
        public string? label { get; set; }
    }
}
