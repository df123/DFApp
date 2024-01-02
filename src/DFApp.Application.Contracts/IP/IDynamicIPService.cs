using DFApp.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.IP
{
    public interface IDynamicIPService : ICrudAppService<
        DynamicIPDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateDynamicIPDto>
    {
    }
}
