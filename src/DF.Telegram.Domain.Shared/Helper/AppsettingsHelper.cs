using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace DF.Telegram.Helper
{
    public class AppsettingsHelper
    {
        #nullable disable
        public static IConfiguration Configuration { get; set; }
        #nullable restore

        public AppsettingsHelper(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// 封装要操作的字符
        /// </summary>
        /// <param name="sections">节点配置</param>
        /// <returns></returns>
        public static string app(params string[] sections)
        {
            try
            {

                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception) { }

            return "";
        }
    }
}