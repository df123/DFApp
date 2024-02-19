using DFApp.Background;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;

namespace DFApp.Queue
{
    public class QueueManagement : IQueueManagement
    {
        private readonly ConcurrentDictionary<string, IQueueBase> _dicQueue;
        public QueueManagement()
        {
            _dicQueue = new ConcurrentDictionary<string, IQueueBase>();
        }

        public IQueueBase<T> AddQueue<T>(string queueName)
        {

            if (_dicQueue.ContainsKey(queueName))
            {
                IQueueBase<T>? queueBase = _dicQueue[queueName] as IQueueBase<T>;
                if (queueBase == null)
                {
                    throw new Exception("相同名称，不同类型");
                }
                else
                {
                    return queueBase;
                }
            }

            IQueueBase<T> queue = new QueueBase<T>();
            _dicQueue.TryAdd(queueName, queue);

            return queue;
        }

        public void AddQueueValue<T>(string queueName, T queueValue)
        {
            if (!_dicQueue.ContainsKey(queueName))
            {
                AddQueue<T>(queueName);
            }

            IQueueBase<T>? queue = _dicQueue[queueName] as IQueueBase<T>;



            if (queue == null)
            {
                throw new Exception("没有找到对应泛型的队列");
            }

            queue.AddItem(queueValue);
        }

        public IQueueBase<T> GetQueue<T>(string queueName)
        {
            if (!_dicQueue.ContainsKey(queueName))
            {
                AddQueue<T>(queueName);
            }

            IQueueBase<T>? queue = _dicQueue[queueName] as IQueueBase<T>;

            if (queue == null)
            {
                throw new Exception("没有找到对应泛型的队列");
            }

            return queue;

        }
    }
}
