using DFApp.Bookkeeping.Expenditure;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Bookkeeping.Category
{
    public class BookkeepingCategoryDto : AuditedEntityDto<long>
    {
        public string Category { get; set; } = null!;
    }
}
