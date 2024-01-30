using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DFApp.Queue
{
    public class QueueBase<T> : IQueueBase<T>
    {
        private readonly ConcurrentQueue<T> _receiveItems;
        private readonly SemaphoreSlim _signal;

        public QueueBase()
        {
            _receiveItems = new ConcurrentQueue<T>();
            _signal = new SemaphoreSlim(0);
        }

        public async Task<T?> GetItemAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            if (_receiveItems.TryDequeue(out T? item))
            {
                return item;
            }
            return default;
        }

        public int GetConcurrentQueueCount()
        {
            return _receiveItems.Count;
        }

        public int GetSemaphoreSlimCount()
        {
            return _signal.CurrentCount;
        }

        public void AddItem(T model)
        {
            if (model == null)
            {
                return;
            }

            _receiveItems.Enqueue(model);
            _signal.Release();
        }

        public T[] GetArray()
        {
            return _receiveItems.ToArray<T>();
        }

        public void Clear()
        {
            _receiveItems.Clear();
            while (_receiveItems.Count <= 0 && _signal.CurrentCount > 0)
            {
                _signal.Wait();
            }
        }
    }
}
