using DFApp.Bookkeeping.Category;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.Bookkeeping.Expenditure
{
    public class FilterAndPagedAndSortedResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}