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

        //private readonly ConcurrentDictionary<string, IDFAppBackgroundWorkerBase> _dicBackgroudWorker;

        public QueueManagement()
        {
            _dicQueue = new ConcurrentDictionary<string, IQueueBase>();
            //_dicBackgroudWorker = new ConcurrentDictionary<string, IDFAppBackgroundWorkerBase>();
        }

        public IQueueBase<T> AddQueue<T>(string queueName)//, IDFAppBackgroundWorkerBase backgroundWorker)
        {

            if (_dicQueue.ContainsKey(queueName))
            {
                throw new Exception("有相同名称的Queue");
            }

            IQueueBase<T> queue = new QueueBase<T>();
            _dicQueue.TryAdd(queueName, queue);

            //_dicBackgroudWorker.TryAdd(queueName, backgroundWorker);

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

            //_ = RunBackgroud(queueName);

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

        //private async Task RunBackgroud(string queueName)
        //{
        //    IDFAppBackgroundWorkerBase workerBase = _dicBackgroudWorker[queueName];

        //    if (workerBase.ExecuteTask!.IsCompleted)
        //    {
        //        await workerBase.StartAsync(default);
        //    }

        //}


    }
}
