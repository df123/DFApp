using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DFApp.Queue
{
    public interface IBackgroundTaskQueue
    {
        void EnqueueTask(Func<IServiceScopeFactory, CancellationToken, Task> task);

        Task<Func<IServiceScopeFactory, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);

    }

}
