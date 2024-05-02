using DFApp.Aria2.Response.TellStatus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.Aria2
{
    public interface IAria2Service: ICrudAppService<TellStatusResultDto, long>
    {
        Task<string> GetExternalLink(long id);
    }
}
