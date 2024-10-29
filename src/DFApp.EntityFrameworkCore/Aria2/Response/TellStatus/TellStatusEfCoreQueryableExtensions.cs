using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFApp.Aria2.Response.TellStatus
{
    public static class TellStatusEfCoreQueryableExtensions
    {
        public static IQueryable<TellStatusResult> IncludeSub(this IQueryable<TellStatusResult> queryable,
            bool include = true)
        {
            if (!include)
            {
                return queryable;
            }

            return queryable.Include(x => x.Files!).ThenInclude(x => x.Uris);
        }
    }
}
