using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;
using System.ComponentModel.DataAnnotations.Schema;
using DFApp.DataFilters;

namespace DFApp.Bookkeeping
{
    public class BookkeepingExpenditure : AuditedAggregateRoot<long>, ISoftDelete, ICreatorId
    {
        [Column(TypeName = "Date")]
        public DateTime ExpenditureDate { get; set; }
        public decimal Expenditure { get; set; }
        public BookkeepingCategory? Category { get; set; }
        public long CategoryId { get;set; }
        public bool IsDeleted { get; set; }
    }
}
