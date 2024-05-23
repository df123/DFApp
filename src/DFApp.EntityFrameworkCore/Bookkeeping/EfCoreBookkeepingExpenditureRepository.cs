using DFApp.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DFApp.Bookkeeping
{
    public class EfCoreBookkeepingExpenditureRepository : EfCoreRepository<DFAppDbContext, BookkeepingExpenditure, long>, IBookkeepingExpenditureRepository
    {
        public EfCoreBookkeepingExpenditureRepository(IDbContextProvider<DFAppDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public override async Task<IQueryable<BookkeepingExpenditure>> WithDetailsAsync()
        {
            return (await GetQueryableAsync()).IncludeSub();
        }

    }
}
