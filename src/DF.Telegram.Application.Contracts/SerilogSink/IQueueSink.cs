using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Services;

namespace DF.Telegram.SerilogSink
{
    public interface IQueueSink: IApplicationService
    {
        List<string> GetLogs();
    }
}
