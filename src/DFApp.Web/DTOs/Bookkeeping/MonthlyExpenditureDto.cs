using System.Collections.Generic;

namespace DFApp.Web.DTOs.Bookkeeping
{
    /// <summary>
    /// 月度支出统计 DTO
    /// </summary>
    public class MonthlyExpenditureDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public MonthlyExpenditureDto()
        {
            Labels = new List<string>();
            TotalData = new List<decimal>();
            SelfData = new List<decimal>();
            NonSelfData = new List<decimal>();
        }

        /// <summary>
        /// 标签列表（月份，如 "2024/1"）
        /// </summary>
        public List<string> Labels { get; set; }

        /// <summary>
        /// 每月总支出数据
        /// </summary>
        public List<decimal> TotalData { get; set; }

        /// <summary>
        /// 每月自己的支出数据
        /// </summary>
        public List<decimal> SelfData { get; set; }

        /// <summary>
        /// 每月非自己的支出数据
        /// </summary>
        public List<decimal> NonSelfData { get; set; }

        /// <summary>
        /// 月均总支出
        /// </summary>
        public decimal TotalAverage { get; set; }

        /// <summary>
        /// 月均自己的支出
        /// </summary>
        public decimal SelfAverage { get; set; }

        /// <summary>
        /// 月均非自己的支出
        /// </summary>
        public decimal NonSelfAverage { get; set; }
    }
}
