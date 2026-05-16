namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 彩票常量 DTO，包含彩票类型及其对应的号码范围信息
    /// </summary>
    public class LotteryConstDto
    {
        /// <summary>
        /// 无参构造函数
        /// </summary>
        public LotteryConstDto()
        {
            LotteryType = string.Empty;
            LotteryTypeEng = string.Empty;
        }

        /// <summary>
        /// 彩票类型（中文名称，如 "双色球"、"快乐8"）
        /// </summary>
        public string LotteryType { get; set; }

        /// <summary>
        /// 彩票类型（英文名称，如 "ssq"、"kl8"）
        /// </summary>
        public string LotteryTypeEng { get; set; }

        /// <summary>
        /// 红球最小值
        /// </summary>
        public int RedMin { get; set; }

        /// <summary>
        /// 红球最大值
        /// </summary>
        public int RedMax { get; set; }

        /// <summary>
        /// 红球个数
        /// </summary>
        public int RedCount { get; set; }

        /// <summary>
        /// 蓝球最小值
        /// </summary>
        public int BlueMin { get; set; }

        /// <summary>
        /// 蓝球最大值
        /// </summary>
        public int BlueMax { get; set; }

        /// <summary>
        /// 蓝球个数
        /// </summary>
        public int BlueCount { get; set; }
    }
}
