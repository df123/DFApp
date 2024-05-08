using DFApp.Bookkeeping.Category;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFApp.Bookkeeping.Expenditure
{
    public class CreateUpdateBookkeepingExpenditureDto
    {
        public DateTime ExpenditureDate { get; set; }
        public decimal Expenditure { get; set; }
        public string? Remark {  get; set; }    
        public long CategoryId { get; set; }
    }
}
