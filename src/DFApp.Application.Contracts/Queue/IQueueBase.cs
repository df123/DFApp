using System.Threading;
using System.Threading.Tasks;

namespace DFApp.Queue
{
    public interface IQueueBase<T> : IQueueBase
    {
        public void AddItem(T model);
        public Task<T?> GetItemAsync(CancellationToken cancellationToken);
        public T[] GetArray();
        public bool ResetSignal();
    }

    public interface IQueueBase
    {
        public int GetConcurrentQueueCount();
        public int GetSemaphoreSlimCount();
        public void Clear();
    }

}
