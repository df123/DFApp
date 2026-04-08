using Microsoft.Extensions.Configuration;

namespace DFApp.Web.DTOs.Lottery
{
    /// <summary>
    /// 彩票常量，定义彩票类型和相关配置
    /// </summary>
    public static class LotteryConst
    {
        /// <summary>
        /// 双色球中文名称
        /// </summary>
        public const string SSQ = "双色球";

        /// <summary>
        /// 快乐8中文名称
        /// </summary>
        public const string KL8 = "快乐8";

        /// <summary>
        /// 双色球英文名称（用于代理API请求）
        /// </summary>
        public const string SSQ_ENG = "ssq";

        /// <summary>
        /// 快乐8英文名称（用于代理API请求）
        /// </summary>
        public const string KL8_ENG = "kl8";

        /// <summary>
        /// 双色球起始期号代码
        /// </summary>
        public const string SSQ_START_CODE = "2003001";

        /// <summary>
        /// 快乐8起始期号代码
        /// </summary>
        public const string KL8_STRAT_CODE = "2019001";

        /// <summary>
        /// 获取彩票代理服务器URL
        /// </summary>
        /// <param name="configuration">配置对象</param>
        /// <returns>代理服务器地址</returns>
        public static string GetLotteryProxyUrl(IConfiguration configuration)
        {
            return configuration["LotteryProxy:Url"] ?? "http://localhost:5000";
        }
    }
}
