using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;
using DFApp.DataFilters;

namespace DFApp.Bookkeeping
{
    public class BookkeepingCategory : AuditedAggregateRoot<long>, ISoftDelete, ICreatorId
    {

        public string Category { get; set; } = null!;

        public List<BookkeepingExpenditure> Expenditures { get; set; } = null!;

        public bool IsDeleted { get; set; }
    }
}
