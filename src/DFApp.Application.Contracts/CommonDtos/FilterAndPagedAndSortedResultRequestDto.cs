using DFApp.Bookkeeping.Category;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace DFApp.CommonDtos
{
    public class FilterAndPagedAndSortedResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}