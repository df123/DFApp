using System.Collections.Generic;

namespace DFApp.Web.DTOs.Media
{
    /// <summary>
    /// 图表数据 DTO（按聊天标题分组统计）
    /// </summary>
    public class ChartDataDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public ChartDataDto()
        {
            Labels = new List<string>();
            Datas = new List<int>();
        }

        /// <summary>
        /// 标签列表（聊天标题）
        /// </summary>
        public List<string> Labels { get; set; }

        /// <summary>
        /// 数据列表（每个标签对应的数量）
        /// </summary>
        public List<int> Datas { get; set; }
    }
}
