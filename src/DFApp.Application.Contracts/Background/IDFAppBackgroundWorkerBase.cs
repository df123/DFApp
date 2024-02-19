using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;

namespace DFApp.Background
{
    public interface IDFAppBackgroundWorkerBase: IBackgroundWorker
    {
        Task? ExecuteTask { get; }

        string ModuleName { get; }

        DateTime StartTime { get; }

        DateTime RestartTime { get;  }

        int RestartCount { get; }

        bool HasError { get; }

        int ErrorCount { get; }

        string ErrorDescription { get; }

        Task RestartAsync(CancellationToken cancellationToken = default);

    }
}
