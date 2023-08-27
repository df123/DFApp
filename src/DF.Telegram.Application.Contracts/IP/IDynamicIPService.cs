using DF.Telegram.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace DF.Telegram.IP
{
    public interface IDynamicIPService : ICrudAppService<
        DynamicIPDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateDynamicIPDto>
    {
    }
}
