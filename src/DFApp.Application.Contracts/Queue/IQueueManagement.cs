using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DFApp.Background;

namespace DFApp.Queue
{
    public interface IQueueManagement
    {
        IQueueBase<T> AddQueue<T>(string queueName);

        void AddQueueValue<T>(string queueName, T queueValue);

        IQueueBase<T> GetQueue<T>(string queueName);



    }
}
