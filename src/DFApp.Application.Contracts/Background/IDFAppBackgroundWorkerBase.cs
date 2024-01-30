using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DFApp.Background
{
    public interface IDFAppBackgroundWorkerBase
    {
        Task? ExecuteTask { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

    }
}
