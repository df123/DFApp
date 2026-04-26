using System.Collections.Generic;

namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 彩票组合投注 DTO
    /// </summary>
    public class LotteryCombinationDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LotteryCombinationDto()
        {
            Reds = new List<string>();
            Blues = new List<string>();
        }

        /// <summary>
        /// 红球号码列表
        /// </summary>
        public List<string> Reds { get; set; }

        /// <summary>
        /// 蓝球号码列表
        /// </summary>
        public List<string> Blues { get; set; }

        /// <summary>
        /// 期号
        /// </summary>
        public int Period { get; set; }
    }
}
