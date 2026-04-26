using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.Configuration;
using DFApp.Web.Data;

namespace DFApp.Web.Data.Configuration
{
    /// <summary>
    /// 配置信息仓储接口
    /// </summary>
    public interface IConfigurationInfoRepository : ISqlSugarReadOnlyRepository<ConfigurationInfo, long>
    {
        /// <summary>
        /// 获取指定配置的值
        /// </summary>
        /// <param name="configurationName">配置名称</param>
        /// <param name="moduleName">模块名称（支持空字符串）</param>
        /// <returns>配置值</returns>
        Task<string> GetConfigurationInfoValue(string configurationName, string moduleName);

        /// <summary>
        /// 获取指定模块的所有配置参数
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>配置信息列表</returns>
        Task<List<ConfigurationInfo>> GetAllParametersInModule(string moduleName);
    }
}
