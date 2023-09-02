using DF.Telegram.EntityFrameworkCore;
using DF.Telegram.Helper;
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

namespace DF.Telegram.Media
{
    public class EfCoreMediaRepository : EfCoreRepository<TelegramDbContext, MediaInfo, long>, IMediaRepository
    {
        private readonly IDataFilter _dataFilter;
        public EfCoreMediaRepository(IDbContextProvider<TelegramDbContext> dbContextProvider, IDataFilter dataFilter) : base(dbContextProvider)
        {
            _dataFilter = dataFilter;
        }


        public async Task<List<MediaInfo>> GetAllTitleNotNullContainSoftDelete()
        {
            using (_dataFilter.Disable<ISoftDelete>())
            {
                var dbSet = await GetDbSetAsync();
                return await dbSet.Where(item => (!string.IsNullOrWhiteSpace(item.Title))).ToListAsync();
            }
        }


        public async Task<MediaInfo[]> GetAllContainSoftDelete()
        {
            using (_dataFilter.Disable<ISoftDelete>())
            {
                var dbSet = await GetDbSetAsync();
                return await dbSet.Where(item => true).ToArrayAsync();
            }
        }

        public async Task<MediaInfo[]> GetByAccessHashID(long accessHash, long tId, long size)
        {
            using (_dataFilter.Disable<ISoftDelete>())
            {
                var dbSet = await GetDbSetAsync();
                return await dbSet.Where(m => m.AccessHash == accessHash && m.TID == tId && m.Size == size).ToArrayAsync();
            }
        }

        public async Task<MediaInfo[]> GetByValueSHA1([NotNull] string valueSHA1)
        {
            using (_dataFilter.Disable<ISoftDelete>())
            {
                var dbSet = await GetDbSetAsync();
                return await dbSet.Where(m => m.ValueSHA1 == valueSHA1).ToArrayAsync();
            }
        }

        public async Task<long> GetDownloadsSize()
        {
            using (_dataFilter.Disable<ISoftDelete>())
            {
                DateTime todayAtZero = DateTimeHelper.GetTodayAtZero();
                DateTime tomorrowAtZero = DateTimeHelper.GetTomorrowAtZero();
                var dbSet = await GetDbSetAsync();
                return await dbSet.Where(m =>
                        m.LastModificationTime >= todayAtZero &&
                        m.LastModificationTime < tomorrowAtZero).SumAsync(x => x.Size);
            }
        }

        public async Task<MediaInfo[]> GetMediaNotReturn()
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet.Where(m => (!string.IsNullOrWhiteSpace(m.SavePath))).ToArrayAsync();
        }
    }
}
