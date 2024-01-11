using DFApp.Bookkeeping.Category;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Bookkeeping.Expenditure
{
    public class BookkeepingExpenditureDto : AuditedEntityDto<long>
    {
        public DateTime ExpenditureDate { get; set; }
        public decimal Expenditure { get; set; }
        public BookkeepingCategoryDto Category { get; set; } = new BookkeepingCategoryDto();
        public long CategoryId { get; set; }
    }
}
