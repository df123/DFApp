using System.Threading;
using System.Threading.Tasks;

namespace DF.Telegram.Queue
{
    public interface IQueueBase<T>
    {
        public int GetConcurrentQueueCount();
        public int GetSemaphoreSlimCount();
        public void AddItem(T model);
        public Task<T?> GetItemAsync(CancellationToken cancellationToken);
        public T[] GetArray();
        public void Clear();
    }
}
