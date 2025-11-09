using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DFApp.Lottery
{
    public static class LotteryConst
    {
        public const string SSQ = "双色球";
        public const string KL8 = "快乐8";

        public const string SSQ_ENG = "ssq";
        public const string KL8_ENG = "kl8";

        public const string SSQ_START_CODE = "2013001";
        public const string KL8_STRAT_CODE = "2020001";

        // 代理服务器配置键
        public const string LOTTERY_PROXY_URL_KEY = "LotteryProxy:Url";
        
        /// <summary>
        /// 获取代理服务器URL
        /// </summary>
        /// <param name="configuration">配置对象</param>
        /// <returns>代理服务器URL</returns>
        public static string GetLotteryProxyUrl(IConfiguration configuration)
        {
            return configuration[LOTTERY_PROXY_URL_KEY] ?? "http://localhost:5000";
        }
    }


}
