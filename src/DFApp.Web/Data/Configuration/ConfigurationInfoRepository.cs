using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFApp.Configuration;
using DFApp.Web.Data;
using SqlSugar;
using Volo.Abp;

namespace DFApp.Web.Data.Configuration
{
    /// <summary>
    /// 配置信息仓储实现
    /// </summary>
    public class ConfigurationInfoRepository : SqlSugarReadOnlyRepository<ConfigurationInfo, long>, IConfigurationInfoRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db">SqlSugar 客户端</param>
        public ConfigurationInfoRepository(ISqlSugarClient db) : base(db)
        {
        }

        /// <summary>
        /// 获取指定配置的值
        /// </summary>
        /// <param name="configurationName">配置名称</param>
        /// <param name="moduleName">模块名称（支持空字符串）</param>
        /// <returns>配置值</returns>
        public async Task<string> GetConfigurationInfoValue(string configurationName, string moduleName)
        {
            var info = await GetFirstOrDefaultAsync(x => x.ConfigurationName == configurationName && (x.ModuleName == moduleName || x.ModuleName == string.Empty));

            if (info == null)
            {
                throw new UserFriendlyException("配置参数不存在");
            }

            if (info.ConfigurationValue == null)
            {
                throw new UserFriendlyException("配置参数值不存在");
            }

            return info.ConfigurationValue;
        }

        /// <summary>
        /// 获取指定模块的所有配置参数
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>配置信息列表</returns>
        public async Task<List<ConfigurationInfo>> GetAllParametersInModule(string moduleName)
        {
            var infos = await GetListAsync(x => x.ModuleName == moduleName);

            if (infos == null || infos.Count <= 0)
            {
                throw new UserFriendlyException("配置参数不存在");
            }

            return infos;
        }
    }
}
