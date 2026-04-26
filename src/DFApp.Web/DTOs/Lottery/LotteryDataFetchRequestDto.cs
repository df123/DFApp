namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 彩票数据获取请求 DTO
    /// </summary>
    public class LotteryDataFetchRequestDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LotteryDataFetchRequestDto()
        {
            LotteryType = string.Empty;
            DayStart = string.Empty;
            DayEnd = string.Empty;
        }

        /// <summary>
        /// 彩票类型（英文代码，如 "ssq"、"kl8"）
        /// </summary>
        public string LotteryType { get; set; }

        /// <summary>
        /// 开始日期（格式：yyyy-MM-dd）
        /// </summary>
        public string DayStart { get; set; }

        /// <summary>
        /// 结束日期（格式：yyyy-MM-dd）
        /// </summary>
        public string DayEnd { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageNo { get; set; }

        /// <summary>
        /// 是否保存到数据库
        /// </summary>
        public bool SaveToDatabase { get; set; }
    }
}
