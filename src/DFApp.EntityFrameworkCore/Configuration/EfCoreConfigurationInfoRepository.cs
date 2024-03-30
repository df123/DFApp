using DFApp.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DFApp.Configuration
{
    public class EfCoreConfigurationInfoRepository : EfCoreRepository<DFAppDbContext, ConfigurationInfo, long>, IConfigurationInfoRepository
    {
        public EfCoreConfigurationInfoRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<ConfigurationInfo>> GetAllParametersInModule(string moduleName)
        {
            var dbSet = await GetDbSetAsync();
            var infos = dbSet.Where(x => x.ModuleName == moduleName).ToList();
            if (infos == null || infos.Count <= 0)
            {
                throw new UserFriendlyException("配置参数不存在");
            }

            return infos.ToList();
        }

        public async Task<string> GetConfigurationInfoValue(string configurationName, string moduleName)
        {

            var dbSet = await GetDbSetAsync();

            ConfigurationInfo? info = dbSet.FirstOrDefault(x => (x.ModuleName == moduleName || x.ModuleName == string.Empty) && x.ConfigurationName == configurationName);
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
    }
}
