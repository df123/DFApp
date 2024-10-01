using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DFApp.Queue
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<IServiceScopeFactory, CancellationToken, Task>> _items = new();

        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void EnqueueTask(Func<IServiceScopeFactory, CancellationToken, Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _items.Enqueue(task);
            _signal.Release();
        }

        public async Task<Func<IServiceScopeFactory, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _items.TryDequeue(out var task);

            if (task == null)
            {
                throw new InvalidOperationException("DequeueAsync returned null task");
            }

            return task;
        }
    }
}
