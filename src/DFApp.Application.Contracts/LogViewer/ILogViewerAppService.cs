using System.Collections.Generic;
using System.Threading.Tasks;
using DFApp.LogViewer.Dtos;
using Volo.Abp.Application.Services;

namespace DFApp.LogViewer
{
    public interface ILogViewerAppService : IApplicationService
    {
        Task<List<LogFileDto>> GetLogFilesAsync();
        
        Task<string> GetLogContentAsync(string fileName, bool isTail = true);
    }
}
