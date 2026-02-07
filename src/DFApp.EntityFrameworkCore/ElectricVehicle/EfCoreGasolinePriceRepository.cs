using DFApp.ElectricVehicle;
using DFApp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DFApp.EntityFrameworkCore
{
    public class EfCoreGasolinePriceRepository : EfCoreRepository<DFAppDbContext, GasolinePrice, Guid>, IGasolinePriceRepository
    {
        public EfCoreGasolinePriceRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<GasolinePrice?> GetLatestPriceAsync(string province)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.Province == province)
                .OrderByDescending(x => x.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<GasolinePrice?> GetPriceByDateAsync(string province, DateTime date)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.Province == province && x.Date.Date == date.Date)
                .FirstOrDefaultAsync();
        }
    }
}
