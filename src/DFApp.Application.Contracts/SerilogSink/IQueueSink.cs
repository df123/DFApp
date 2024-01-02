using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;

namespace DFApp.SerilogSink
{
    public interface IQueueSink: IApplicationService
    {
        List<string> GetLogs();
    }
}
