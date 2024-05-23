using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFApp.Bookkeeping
{
    public static class BookkeepingExpenditureQueryableExtensions
    {
        public static IQueryable<BookkeepingExpenditure> IncludeSub(this IQueryable<BookkeepingExpenditure> queryable,
            bool include = true)
        {
            if (!include)
            {
                return queryable;
            }
            return queryable.Include(x => x.Category);
        }
    }
}
