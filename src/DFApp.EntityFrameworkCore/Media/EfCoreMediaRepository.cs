using DFApp.EntityFrameworkCore;
using DFApp.Helper;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DFApp.Media
{
    public class EfCoreMediaRepository : EfCoreRepository<DFAppDbContext, MediaInfo, long>, IMediaRepository
    {
        public EfCoreMediaRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<long> GetDownloadsSize()
        {
            DateTime todayAtZero = DateTimeHelper.GetTodayAtZero();
            DateTime tomorrowAtZero = DateTimeHelper.GetTomorrowAtZero();
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(m =>
                    m.LastModificationTime >= todayAtZero &&
                    m.LastModificationTime < tomorrowAtZero).SumAsync(x => x.Size);
        }

        public async Task<MediaInfo[]> GetMediaNotReturn()
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(m => (!string.IsNullOrWhiteSpace(m.SavePath))).ToArrayAsync();
        }
    }
}
