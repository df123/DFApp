using DFApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DFApp.SerilogSink
{
    [Authorize(DFAppPermissions.LogSink.QueueSink)]
    public class QueueSinkService : ApplicationService, IQueueSink
    {
        public List<string> GetLogs()
        {
            QueueMessage[] dtos = QueueSinkData.Queue.ToArray();

            List<string> logs = dtos.Select(x => x.Msg).ToList();

            logs.Reverse();

            return logs;
        }
    }
}
