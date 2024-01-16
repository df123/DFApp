using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace DFApp.Bookkeeping
{
    public interface IBookkeepingExpenditureRepository : IRepository<BookkeepingExpenditure, long>
    {
    }
}
